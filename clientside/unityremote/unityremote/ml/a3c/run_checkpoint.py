#!/usr/bin/env python3

"""
Run a trained agent from a checkpoint.
"""
import argparse
import time
import gym
import numpy as np
import tensorflow as tf
from .preprocessing import generic_preprocess
from threading import Thread
import threading

def run(env_defs, kargs=None):
    args = parse_args(kargs)
    network_created = False
    for i in range(args.n_workers):
        env = gym.make(args.env_id)
        env.configure(env_defs, i)
        env = generic_preprocess(env, max_n_noops=0)
        if not network_created:
            sess, obs_placeholder, action_probs_op = \
                get_network(args.ckpt_dir, env.observation_space.shape, env.action_space.n, env_defs)
            network_created = True
        t = Thread(target=run_agent, args=(env, sess, obs_placeholder, action_probs_op))
        t.start()

def parse_args(kargs=None):
    parser = argparse.ArgumentParser()
    parser.add_argument("env_id")
    parser.add_argument("ckpt_dir")
    parser.add_argument('--n_workers', type=int, default=1)
    if not kargs:
        args = parser.parse_args()
    else:
        args = parser.parse_args(args=kargs)
    return args


def get_network(ckpt_dir, obs_shape, n_actions, env_defs):
    sess = tf.Session()
    make_inference_network = env_defs['make_inference_network']
    with tf.variable_scope('global'):
        obs_placeholder, _, action_probs_op, _, _ = \
            make_inference_network(obs_shape, n_actions, debug=False)

    ckpt_file = tf.train.latest_checkpoint(ckpt_dir)
    if not ckpt_file:
        raise Exception("Couldn't find checkpoint in '{}'".format(ckpt_dir))
    print("Loading checkpoint from '{}'".format(ckpt_file))
    saver = tf.train.Saver()
    saver.restore(sess, ckpt_file)

    return sess, obs_placeholder, action_probs_op


def run_agent(env, sess, obs_placeholder, action_probs_op):
    while True:
        obs = env.reset()
        episode_reward = 0
        done = False
        while not done:
            feed_dict = {obs_placeholder: [obs]}
            action_probs = sess.run(action_probs_op, feed_dict)[0]
            action = np.random.choice(env.action_space.n, p=action_probs)
            obs, reward, done, _ = env.step(action)
            episode_reward += reward
            env.render()
            time.sleep(1 / 60.0)
        print("Episode reward:", episode_reward)


if __name__ == '__main__':
    run()
