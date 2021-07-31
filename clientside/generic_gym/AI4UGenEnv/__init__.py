from gym.envs.registration import register
from AI4UGenEnv.envs.env import GenericEnvironment

register (
    id='AI4UGenEnv-v0',
    entry_point='AI4UGenEnv.envs:GenericEnvironment',
)

