from gym.envs.registration import register
from AI4UGym.envs.env import BasicAgent

register (
    id='AI4U-v0',
    entry_point='AI4UGym.envs:AI4UEnvironment',
)

