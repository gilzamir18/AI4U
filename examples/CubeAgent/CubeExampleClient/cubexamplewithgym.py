import gym
import UnityRemoteGym
from unityremote.utils import environment_definitions as env_def

env = gym.make("UnityRemote-v0")
env_def['actions'] = [('tx', 10)]
env_def['actions_meaning'] = ['horizontal movement']
env_def['input_port'] = 8080
env_def['output_port'] = 8081
env.configure(env_def)
env.reset()
env.step(0)
