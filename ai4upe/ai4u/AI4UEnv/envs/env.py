# -*- coding: utf-8 -*-
import threading as td
import numpy as np
import io
import random
import gym
from gym import error, spaces, utils
from gym.utils import seeding
from ai4u.controllers import BasicGymController
from ai4u.appserver import startasdaemon

class GenericEnvironment(gym.Env):
  """Custom Environment that follows gym interface"""
  metadata = {'render.modes': ['human']}

  def __init__(self, controller_class=BasicGymController, rid=None, server_IP="127.0.0.1", server_port=8080, sleep=0.1):
    super(GenericEnvironment, self).__init__()
    # Define action and observation space
    # They must be gym.spaces objects
    # Example when using discrete actions:
    if rid is None:
      rid = ["0"]
    elif type(rid) is str:
      rid = [rid]
    elif type(rid) is int:
      rid = [str(rid)]
    else:
      raise TypeError("Unsupported type of ids parameter: ", type(rid))

    controllers_classes =  [controller_class]
    controller = startasdaemon(rid, controllers_classes, server_IP, server_port, sleep)[0]
    self.controller = controller
    self.action_space = self.controller.action_space
    self.observation_space = self.controller.observation_space
    self.rid =  rid

  def step(self, action):
    return self.controller.get_state(self.controller.request_step(action))

  def seed(self, seed=0):
    self.controller.seed(seed)

  def reset(self):
    return self.controller.request_reset()
  
  def render(self, mode='human'):
    return self.controller.render()

  def close (self):
    return self.controller.close()
