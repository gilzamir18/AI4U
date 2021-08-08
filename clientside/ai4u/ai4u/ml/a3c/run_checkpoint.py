#!/usr/bin/env python3

"""
Run a trained agent from a checkpoint.
"""
import argparse
import time
import gym
import numpy as np
from ai4u.ml.a3c import preprocessing
from ai4u.ml.a3c.network import Network
from ai4u.ml.a3c.preprocessing import generic_preprocess
from threading import Thread
import threading
import tensorflow
from collections import OrderedDict
import copy

if tensorflow.__version__ >= "2":
    import tensorflow.compat.v1 as tf
    tf.disable_v2_behavior()
else:
    import tensorflow as tf


def run(env_defs, kargs=None):
    args, preprocess_wrapper = parse_args(env_defs, kargs)
    network_created = False
    for i in range(args.n_workers):
        env = gym.make(args.env_id)
        env.configure(env_defs, i)
        env = preprocess_wrapper(env, max_n_noops=0)
        if not network_created:
            sess, obs_placeholder, action_probs_op, network = \
                get_network(args.ckpt_dir, env.observation_space.shape, env.action_space.n, env_defs)
            network_created = True
        t = Thread(target=run_agent, args=(env, sess, obs_placeholder, action_probs_op, network))
        t.start()

def parse_args(env_defs, kargs=None):
    parser = argparse.ArgumentParser()
    parser.add_argument("env_id")
    parser.add_argument("ckpt_dir")
    parser.add_argument('--n_workers', type=int, default=1)
    parser.add_argument("--preprocessing",
                        choices=['generic', 'user_defined'],
                        default='generic')

    if not kargs:
        args = parser.parse_args()
    else:
        args = parser.parse_args(args=kargs)


    if args.preprocessing == 'generic':
        preprocess_wrapper = preprocessing.generic_preprocess
    elif args.preprocessing == 'user_defined':
        preprocess_wrapper = env_defs['preprocessing']

    return args, preprocess_wrapper


def get_network(ckpt_dir, obs_shape, n_actions, env_defs):
    sess = tf.Session()
    make_inference_network = env_defs['make_inference_network']

    extra_inputs_shape = None
    if 'extra_inputs_shape' in env_defs:
        extra_inputs_shape = env_defs['extra_inputs_shape']

    network = Network(scope="global", n_actions=n_actions, entropy_bonus=None,
                        value_loss_coef=None, max_grad_norm=None,
                        optimizer=None, add_summaries=None, 
                        state_shape=env_defs['state_shape'], 
                        make_inference_network=env_defs['make_inference_network'],
                        detailed_logs=None, debug=False, extra_inputs_shape=extra_inputs_shape, training=False)
    
    with tf.variable_scope('global'):
        obs_placeholder, _, action_probs_op, _, _ = \
            make_inference_network(obs_shape, n_actions, debug=False, extra_inputs_shape=extra_inputs_shape, network=network)

    ckpt_file = tf.train.latest_checkpoint(ckpt_dir)
    if not ckpt_file:
        raise Exception("Couldn't find checkpoint in '{}'".format(ckpt_dir))
    print("Loading checkpoint from '{}'".format(ckpt_file))
    saver = tf.train.Saver()
    saver.restore(sess, ckpt_file)

    return sess, obs_placeholder, action_probs_op, network
    
def fillwithbank(feed_dict, banks):
    for key in banks:
        bank = banks[key]
        feed_dict[bank.input] = [bank.value] 

def setnewmemorytobanks(banks, start_idx, outputs):
    for key in banks:
        bank = banks[key]
        nu = len(bank.update)
        if not bank.updatebylayer or nu == 0:
            bank.value = outputs[start_idx][0]
            start_idx += 1
        else:
            result = []
            for _ in range(len(bank.update)):
                result.append(outputs[start_idx][0])
                start_idx += 1
            bank.value = np.array(result, dtype=np.float32)

def run_agent(env, sess, obs_placeholder, action_probs_op, network):
    objph = None
    extraph = None
    if type(obs_placeholder) is tuple:
        obsph = obs_placeholder[0]
        extraph = obs_placeholder[1]
    else:
        obsph = obs_placeholder

    bank_ops = []

    for key in network.memory_bank:
        b = network.memory_bank[key]
        if len(b.update) == 0:
            bank_ops.append(b.input)
        else:
            bank_ops += b.update

    while True:
        obs = env.reset()

        episode_reward = 0
        done = False

        if len(network.memory_bank) > 0:
            for key in network.memory_bank:
                network.memory_bank[key].reset()

        while not done:
            feed_dict = None
            if extraph is None:
                feed_dict = {obsph: [obs]}
            else:
                feed_dict = {obsph: [obs[0]], extraph: [obs[1]]}

            fillwithbank(feed_dict, network.memory_bank)
            
            outputs = sess.run([action_probs_op] + bank_ops, feed_dict)
            
            action_probs = outputs[0][0]

            setnewmemorytobanks(network.memory_bank, 1, outputs)

            action = np.random.choice(env.action_space.n, p=action_probs)
            obs, reward, done, _ = env.step(action, (action_probs, 0))
            episode_reward += reward
            env.render()
            #time.sleep(1 / 60.0)

        print("Episode reward:", episode_reward)


if __name__ == '__main__':
    run()
