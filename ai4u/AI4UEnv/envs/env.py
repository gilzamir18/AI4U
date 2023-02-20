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

  def __init__(self, controller_class=BasicGymController, rid=None, server_IP="127.0.0.1", server_port=8080, sleep=0.1, buffer_size=8192):
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
    controller = startasdaemon(rid, controllers_classes, server_IP, server_port, buffer_size, sleep)[0]
    self.controller = controller
    self.reset()
    if self.controller.action_space is not None:
      self.action_space = self.controller.action_space
    else:
      self.action_space = None

    if self.controller.observation_space is not None:
      self.observation_space = self.controller.observation_space
    else:
      self.observation_space = None
  
    self.rid =  rid
    self.step_callback = None
  
  def set_stepcallback(self, step_callback):
    self.step_callback = step_callback

  def step(self, action):
    state = self.controller.get_state(self.controller.request_step(action))
    if self.step_callback is not None:
      self.step_callback(action, state)
    return state

  def seed(self, seed=0):
    self.controller.seed(seed)

  def reset(self):
    self.last_action = None
    return self.controller.request_reset()

  def render(self, mode='human'):
    return self.controller.render()

  def close (self):
    return self.controller.close()
