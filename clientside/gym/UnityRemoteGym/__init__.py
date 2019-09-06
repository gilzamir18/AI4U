from gym.envs.registration import register

register (
    id='UnityRemote-v0',
    entry_point='UnityRemoteGym.envs:UnityRemoteEnvironment',
)

