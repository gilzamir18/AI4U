# -*- coding: utf-8 -*-
import gymnasium as gym
from ai4u.controllers import BasicGymController
from ai4u.appserver import startasdaemon

class GenericEnvironment(gym.Env):
  """Custom Environment that follows gym interface"""
  metadata = {}
  envidx = 0
  controllers = None
  def __init__(self, controller_class=BasicGymController, rid=None, event_callback=None, config=None):
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

    if type(controller_class) is list:
      controller_classes = controller_class
    else:
      controller_classes =  [controller_class] * len(rid)    

    if not GenericEnvironment.controllers:
      GenericEnvironment.controllers = {}
      controllers = startasdaemon(rid, controller_classes, config)
      for i in range(len(controllers)):
        GenericEnvironment.controllers[rid[i]] = controllers[i]
    
    self.rid = rid[GenericEnvironment.envidx]
    self.controller = GenericEnvironment.controllers[self.rid]
    GenericEnvironment.envidx += 1
    self.event_callback = event_callback
    if "metadata" in config:
      self.controller.metadata = config["metadata"]
      self.observation_space, self.action_space = BasicGymController.get_env_spaces(self.controller.metadata)
    else:
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

    reset_returned_data =  self.controller.request_reset()    
    if self.event_callback is not None:
      self.event_callback.on_reset(reset_returned_data)
    return reset_returned_data

  def render(self, mode='human'):
    return self.controller.render()

  def close (self):
    return self.controller.close()
  
