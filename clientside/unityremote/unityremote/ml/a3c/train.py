#!/usr/bin/env python3

import os
import os.path as osp
import time
from threading import Thread

import easy_tf_log
import tensorflow as tf

from . import utils
from . import utils_tensorflow
from .env import make_envs
from .params import parse_args
from .utils_tensorflow import make_lr, make_optimizer
from .worker import Worker
from .network import Network

os.environ['TF_CPP_MIN_LOG_LEVEL'] = '1'  # filter out INFO messages


def make_networks(n_workers, obs_shape, n_actions, value_loss_coef, entropy_bonus, max_grad_norm,
                  optimizer, detailed_logs, debug, env_defs):
    # https://www.tensorflow.org/api_docs/python/tf/Graph notes that graph construction isn't
    # thread-safe. So we all do all graph construction serially before starting the worker threads.

    make_inference_network = env_defs['make_inference_network']

    # Create shared parameters
    with tf.variable_scope('global'):
        make_inference_network(obs_shape, n_actions)

    # Create per-worker copies of shared parameters
    worker_networks = []
    for worker_n in range(n_workers):
        create_summary_ops = (worker_n == 0)
        worker_name = "worker_{}".format(worker_n)
        network = Network(scope=worker_name, n_actions=n_actions, entropy_bonus=entropy_bonus,
                          value_loss_coef=value_loss_coef, max_grad_norm=max_grad_norm,
                          optimizer=optimizer, add_summaries=create_summary_ops, 
                          state_shape=env_defs['state_shape'], 
                          make_inference_network=env_defs['make_inference_network'],
                          detailed_logs=detailed_logs, debug=debug)
        worker_networks.append(network)

    return worker_networks


def make_workers(sess, envs, networks, n_workers, log_dir):
    print("Starting {} workers".format(n_workers))
    workers = []
    for worker_n in range(n_workers):
        worker_name = "worker_{}".format(worker_n)
        worker_log_dir = osp.join(log_dir, worker_name)
        w = Worker(sess=sess, env=envs[worker_n], network=networks[worker_n],
                   log_dir=worker_log_dir)
        workers.append(w)

    return workers


def run_worker(worker, n_steps_to_run, steps_per_update, step_counter, update_counter):
    while int(step_counter) < n_steps_to_run:
        steps_ran = worker.run_update(steps_per_update)
        step_counter.increment(steps_ran)
        update_counter.increment(1)

def start_worker_threads(workers, n_steps, steps_per_update, step_counter, update_counter):
    worker_threads = []
    for worker in workers:
        def f():
            run_worker(worker, n_steps, steps_per_update, step_counter, update_counter)
        thread = Thread(target=f)
        thread.start()
        worker_threads.append(thread)
    return worker_threads


def run_manager(worker_threads, sess, lr, step_counter, update_counter, log_dir, saver,
                wake_interval_seconds, ckpt_interval_seconds):
    checkpoint_file = osp.join(log_dir, 'checkpoints', 'network.ckpt')

    ckpt_timer = utils.Timer(duration_seconds=ckpt_interval_seconds)
    ckpt_timer.reset()

    step_rate = utils.RateMeasure()
    step_rate.reset(int(step_counter))

    while True:
        time.sleep(wake_interval_seconds)

        steps_per_second = step_rate.measure(int(step_counter))
        easy_tf_log.tflog('misc/steps_per_second', steps_per_second)
        easy_tf_log.tflog('misc/steps', int(step_counter))
        easy_tf_log.tflog('misc/updates', int(update_counter))
        easy_tf_log.tflog('misc/lr', sess.run(lr))

        alive = [t.is_alive() for t in worker_threads]

        if ckpt_timer.done() or not any(alive):
            saver.save(sess, checkpoint_file, int(step_counter))
            print("Checkpoint saved to '{}'".format(checkpoint_file))
            ckpt_timer.reset()

        if not any(alive):
            break


def run(env_defs, kargs=None):
    args, lr_args, log_dir, preprocess_wrapper = parse_args(kargs)
    easy_tf_log.set_dir(log_dir)

    utils_tensorflow.set_random_seeds(args.seed)
    sess = tf.Session()

    envs = make_envs(args.env_id, preprocess_wrapper, args.max_n_noops, args.n_workers,
                     args.seed, args.debug, log_dir, env_defs)

    step_counter = utils.TensorFlowCounter(sess)
    update_counter = utils.TensorFlowCounter(sess)
    lr = make_lr(lr_args, step_counter.value)
    optimizer = make_optimizer(lr)
    networks = make_networks(n_workers=args.n_workers, obs_shape=envs[0].observation_space.shape,
                             n_actions=envs[0].action_space.n, value_loss_coef=args.value_loss_coef,
                             entropy_bonus=args.entropy_bonus, max_grad_norm=args.max_grad_norm,
                             optimizer=optimizer, detailed_logs=args.detailed_logs,
                             debug=args.debug, env_defs=env_defs)

    global_vars = tf.trainable_variables('global')
    # Why save_relative_paths=True?
    # So that the plain-text 'checkpoint' file written uses relative paths, so that we can restore
    # from checkpoints created on another machine.
    saver = tf.train.Saver(global_vars, max_to_keep=1, save_relative_paths=True)
    if args.load_ckpt:
        print("Restoring from checkpoint '{}'...".format(args.load_ckpt), end='', flush=True)
        saver.restore(sess, args.load_ckpt)
        print("done!")
    else:
        sess.run(tf.global_variables_initializer())

    workers = make_workers(sess, envs, networks, args.n_workers, log_dir)

    worker_threads = start_worker_threads(workers, args.n_steps, args.steps_per_update,
                                          step_counter, update_counter)

    run_manager(worker_threads, sess, lr, step_counter, update_counter, log_dir, saver,
                args.manager_wake_interval_seconds, args.ckpt_interval_seconds)

    for env in envs:
        env.close()


if __name__ == '__main__':
    from unityremote.core import environment_definitions
    run(environment_definitions)
