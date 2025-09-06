# -*- coding: utf-8 -*-
import gymnasium as gym
from ai4u.controllers import BasicGymController
from ai4u.appserver import startasdaemon
from ai4u.utils import rid_generator
import json
import types

class GenericEnvironment(gym.Env):
  """Custom Environment that follows gym interface"""
  metadataobj = None
  envidx = 0
  controllers = None
  def __init__(self, controller_class=BasicGymController, rid=None, event_callback=None, config=None, **kargs):
    super(GenericEnvironment, self).__init__()
    # Define action and observation space
    # They must be gym.spaces objects
    # Example when using discrete actions:
    if config is None:
      config = {'server_IP': "127.0.0.1", 'server_port': 8080, 'waittime':0.0, 'buffer_size': 81920}
    self.render_mode = 'human'
    if rid is None:
      rid = ["0"]
    elif type(rid) is str:
      rid = [rid]
    elif type(rid) is int:
      rid = [str(rid)]
    elif type(rid) is list:
      pass
    else:
      raise TypeError("Unsupported type of ids parameter: ", type(rid))

    if not GenericEnvironment.controllers:
      if type(controller_class) is list:
        controller_classes = controller_class
      else:
        controller_classes =  [controller_class] * len(rid)    

      GenericEnvironment.controllers = {}
      controllers = startasdaemon(rid, controller_classes, config)
      for i in range(len(controllers)):
        GenericEnvironment.controllers[rid[i]] = controllers[i]
      GenericEnvironment.envidx = 0
    self.rid = rid[GenericEnvironment.envidx]
    self.controller = GenericEnvironment.controllers[self.rid]
    self.event_callback = event_callback
    if "metadata" in config:
      self.controller.metadataobj = json.loads(config["metadata"])
      self.observation_space, self.action_space = BasicGymController.get_env_spaces(self.controller.metadata)
    elif "observation_space" in config and "action_space" in config:
      self.observation_space = config['observation_space']
      self.action_space = config['action_space']
    else:
      self.controller.request_config()
      GenericEnvironment.metadataobj = self.controller.metadatamodel
      print("env ", GenericEnvironment.envidx, " was started!")
      print(GenericEnvironment.metadataobj)
      if self.controller.action_space is not None:
        self.action_space = self.controller.action_space
      else:
        self.action_space = None
      if self.controller.observation_space is not None:
        self.observation_space = self.controller.observation_space
      else:
        self.observation_space = None
    GenericEnvironment.envidx += 1

  def step(self, action):
    info = self.controller.request_step(action)
    state = self.controller.transform_state(info)
    if self.event_callback is not None:
      self.event_callback.on_step(action, state, info)
    return state

  def reset(self, seed=None, **options):
    if not hasattr(self, "seed"):
      self.seed = None
    
    if seed is not None:
      self.seed = seed
    #if not GenericEnvironment.metadataobj: 
    #  self.controller.request_config()
    #  GenericEnvironment.metadataobj = self.controller.metadatamodel
    reset_returned_data = self.controller.request_reset()
    if self.event_callback is not None:
      self.event_callback.on_reset(reset_returned_data)
    return reset_returned_data

  def render(self, mode='human'):
    return self.controller.render()

  def close (self):
    return self.controller.close()
  
