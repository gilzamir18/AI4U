# -*- coding: utf-8 -*-
from unityremote.core import RemoteEnv
from unityremote.utils import environment_definitions
import threading as td
import numpy as np
import io
import random
from time import sleep
import gym
from gym import error, spaces, utils
from gym.utils import seeding
import os
import platform
from threading import Thread
import threading
import inspect

class AleWrapper:
    def __init__(self, env):
        self.env = env

    def lives(self):
        return self.env.nlives

class Environment(gym.Env):
    metadata = {'render.modes': ['human']}
    def __init__(self):
        self.ale = AleWrapper(self)
    
    def configure(self, environment_definitions, port_inc=0):
        self.action_space = spaces.Discrete(environment_definitions['action_shape'][0])
        min_value = environment_definitions['min_value']
        max_value = environment_definitions['max_value']
        state_shape = environment_definitions['state_shape']
        state_type = environment_definitions['state_type']
        self.observation_space = spaces.Box(low=min_value, high=max_value, shape=state_shape, dtype=state_type)
        self.n_envs = environment_definitions['n_envs']
        self.actions = environment_definitions['actions']
        self.action_meaning = environment_definitions['action_meaning']

        if inspect.isclass(environment_definitions['state_wrapper']):
            self.state_wrapper = environment_definitions['state_wrapper']()
        else:
            self.state_wrapper = environment_definitions['state_wrapper']
        
        host = environment_definitions['host']
        input_port = environment_definitions['input_port']
        output_port = environment_definitions['output_port']
        self.remoteenv = RemoteEnv(host, output_port+port_inc, input_port+port_inc)
        self.remoteenv.open(0)
        self.nlives = 1

    def get_action_meanings(self):
        return [self.action_meaning[i] for i in range(len(self.actions))]


    def reset(self):
        fields = self.remoteenv.step('restart')
        self.state, self.reward, self.done, self.info = self.state_wrapper(fields, self)
        return self.state
 
    def render(self, mode='human', close=False):
        return self.state
       
    def close(self):
        self.remoteenv.close()

    def step(self, action, info=None):
        fields = self.remoteenv.step(self.actions[action][0], self.actions[action][1])
        return self.state_wrapper(fields, self, action, info)

    def __del__(self):
        self.remoteenv.close()
