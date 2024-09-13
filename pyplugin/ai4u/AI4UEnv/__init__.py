from gymnasium.envs.registration import register
from AI4UEnv.envs.env import GenericEnvironment

register (
    id='AI4UEnv-v0',
    entry_point='AI4UEnv.envs:GenericEnvironment',
)
