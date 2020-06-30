from gym.envs.registration import register
from UnityRemoteGym.envs.env import BasicAgent

register (
    id='UnityRemote-v0',
    entry_point='UnityRemoteGym.envs:UnityRemoteEnvironment',
)

