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


def run_agent(env, sess, obs_placeholder, action_probs_op, network):
    objph = None
    extraph = None
    if type(obs_placeholder) is tuple:
        obsph = obs_placeholder[0]
        extraph = obs_placeholder[1]
    else:
        objph = observations


    state_h = None
    state_c = None
    initial_state_h = None
    initial_state_c = None
    if network.rnn_size > 0:
        initial_state_h = []
        initial_state_c = []
        for i in range(network.rnn_size):
            initial_state_h.append(np.zeros(network.rnn_input_shapes[i][-1]))
            initial_state_c.append(np.zeros(network.rnn_input_shapes[i][-1]))
        initial_state_h = np.array(initial_state_h)
        initial_state_c = np.array(initial_state_c)

    while True:
        if network.rnn_size > 0:
            state_h = initial_state_h.copy()
            state_c = initial_state_c.copy()

        obs = env.reset()

        episode_reward = 0
        done = False

        while not done:
            if network.rnn_size > 0:
                feed_dict = None
                if extraph is None:
                    feed_dict = {obsph: [obs], network.rnn_stateh: [state_h], network.rnn_statec: [state_c]}
                else:
                    feed_dict = {obsph: [obs[0]], extraph: [obs[1]], network.rnn_stateh: [state_h], network.rnn_statec: [state_c]}
                
                action_probs = sess.run( [action_probs_op, ], feed_dict)[0]

                outputs = \
                    sess.run([action_probs_op] + network.rnn_output_ops,
                                feed_dict=feed_dict)

                action_probs = outputs[0][0]
                list_state_h = []
                list_state_c = []
                k = 0
                for _ in range(network.rnn_size):
                    list_state_h.append(outputs[1][0])
                    list_state_c.append(outputs[1+k][0])
                    k += 2
                state_h = np.array(list_state_h, dtype=np.float32)
                state_c = np.array(list_state_c, dtype=np.float32)
            else:
                feed_dict = None
                if extraph is None:
                    feed_dict = {obsph: [obs]}
                else:
                    feed_dict = {obsph: [obs[0]], extraph: [obs[1]]}
                action_probs = sess.run(action_probs_op, feed_dict)[0]
            action = np.random.choice(env.action_space.n, p=action_probs)
            obs, reward, done, _ = env.step(action, (action_probs, 0))
            episode_reward += reward
            env.render()
            #time.sleep(1 / 60.0)
        print("Episode reward:", episode_reward)


if __name__ == '__main__':
    run()
