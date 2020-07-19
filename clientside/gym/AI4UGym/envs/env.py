# -*- coding: utf-8 -*-
from ai4u.core import RemoteEnv
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

class BasicAgent:
    environment_definitions = None
    environment_port_id = 0

    def reset(self, env):
        envinfo = env.remoteenv.step('restart')
        if 'state' in envinfo:
            return envinfo['state']
        else:
            return None

    def render(self):
        pass

    def seed(self, seed=None):
        self.np_random, seed1 = seeding.np_random(seed)
        env.remoteenv.step('seed', seed1)
        return [seed1]

    def act(self, env, action, info=None):
        envinfo = env.one_step(action)
        state = None
        reward = 0
        if 'state' in envinfo:
            state = envinfo['state']

        if 'reward' in envinfo:
            reward = envinfo['reward']

        done = True
        if 'done' in envinfo:
            done = envinfo['done']

        return state, reward, done, {}


class Environment(gym.Env):
    metadata = {'render.modes': ['human']}
    def __init__(self):
        super(Environment, self).__init__()
        self.ale = AleWrapper(self)
        self.configureFlag = False
        self.id = 0
        if not (BasicAgent.environment_definitions is None):
            self.configure(BasicAgent.environment_definitions, BasicAgent.environment_port_id)
            BasicAgent.environment_port_id += 1

    def configure(self, environment_definitions, port_inc=0):
        self.action_space = spaces.Discrete(environment_definitions['action_shape'][0])
        min_value = environment_definitions['min_value']
        max_value = environment_definitions['max_value']
        state_shape = environment_definitions['state_shape']
        state_type = environment_definitions['state_type']
        self.observation_space = spaces.Box(low=min_value, high=max_value, shape=state_shape, dtype=state_type)
        self.n_envs = environment_definitions['n_envs']
        self.actions = environment_definitions['actions']
        self.verbose = False

        if 'verbose' in environment_definitions:
            self.verbose = environment_definitions['verbose']

        if 'action_meaning' in environment_definitions:
            self.action_meaning = environment_definitions['action_meaning']
        else:
            self.action_meaning = ['action']*len(self.actions)

        if 'seed' in environment_definitions:
            self.seed = environment_definitions['seed']
        else:
            self.seed = 0

        if 'agent' in environment_definitions:
            if inspect.isclass(environment_definitions['agent']):
                self.agent = environment_definitions['agent']()
            else:
                raise Exception("Agent object is not a class!!!!")
        else:
            self.agent = BasicAgent()
        
        host = environment_definitions['host']
        input_port = environment_definitions['input_port']
        output_port = environment_definitions['output_port']
        self.remoteenv = RemoteEnv(host, output_port+port_inc, input_port+port_inc)
        self.id = port_inc
        self.remoteenv.verbose = self.verbose
        self.remoteenv.open(0)
        self.nlives = 1
        self.state = None
        self.configureFlag = True

    def seed(self, seed=None):
        self.agent.seed(seed)

    def get_action_meanings(self):
        self.__check_configuration_()
        return [self.action_meaning[i] for i in range(len(self.actions))]

    def reset(self):
        self.__check_configuration_()
        return self.agent.reset(self)
 
    def render(self, mode='human', close=False):
        self.__check_configuration_()
        self.agent.render()
       
    def close(self):
        self.__check_configuration_()
        self.remoteenv.close()

    def one_step(self, action):
        return self.remoteenv.step(self.actions[action][0], self.actions[action][1])

    def step(self, action, info=None):
        self.__check_configuration_()
        return self.agent.act(self, action, info)

    def __del__(self):
        self.__check_configuration_()
        self.remoteenv.close()

    def __check_configuration_(self):
        if not self.configureFlag:
            raise Exception("The environment is not configured. Try to set up the environment before trying again!!!")