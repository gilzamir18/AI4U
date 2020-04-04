from collections import deque

import cv2
import numpy as np
from gym import spaces
from gym.core import Wrapper, ObservationWrapper, RewardWrapper
from gym.spaces import Box
from unityremote.utils import image_decode

"""
Observation preprocessing and environment tweaks.

Section 8 ("Experimental Setup") of the paper says:
  "The Atari experiments used the same input preprocessing as (Mnih et al., 2015)
   and an action repeat of 4."

'Mnih et al., 2015' is 'Human-level control through deep reinforcement learning'.
The relevant parts of that paper's Methods section are summarised below.


# Observation preprocessing:

'Preprocessing':
  - "First, to encode a single frame we take the maximum value for each pixel colour value over the
     frame being encoded and the previous frame. This was necessary to remove flickering that is
     present in games where some objects appear only in even frames while other objects appear only
     in odd frames, an artefact caused by the limited number of sprites Atari 2600 can display at
     once."
  - "Second, we then extract the Y channel, also known as luminance, from the RGB frame and rescale
     it to 84 x 84."
  - "The function phi from algorithm 1 described below applies this preprocessing to the m most
     recent frames and stacks them to produce the input to the Q-function, in which m = 4, although
     the algorithm is robust to different values of m (for example, 3 or 5)."
'Training details':
  - "Following previous approaches to playing Atari 2600 games, we also use a simple frame-skipping
     technique. More precisely, the agent sees and selects actions on every kth frame instead of
     every frame, and its last action is repeated on skipped frames. Because running the emulator
     forward for one step requires much less computation than having the agent select an action,
     this technique allows the agent to play roughly k times more games without significantly
     increasing the runtime. We use k = 4 for all games."

There's some ambiguity about what order to apply these steps in. I think the right order should be:

1. Max over subsequent frames
   So - observation 0: max. over frames 0 and 1
        observation 1: max. over frames 1 and 2
        etc.

2. Extract luminance and scale

3. Skip frames
   So - observation 0: max. over frames 0 and 1
        observation 1: max. over frames 4 and 5
        etc.

4. Stack frames
   So - frame stack 0: max. over frames 0 and 1
                       max. over frames 4 and 5
                       max. over frames 8 and 9
                       max. over frames 12 and 13

        frame stack 1: max. over frames 4 and 5
                       max. over frames 8 and 9
                       max. over frames 12 and 13
                       max. over frames 16 and 17

The main ambiguity is whether frame skipping or frame stacking should be done first.
Above we've assumed frame skipping should be done first. If we did frame stacking first, we would
only look at every 4th frame stack, giving:

- Frame stack 0: max. over frames 0 and 1
                 max. over frames 1 and 2
                 max. over frames 2 and 3
                 max. over frames 3 and 4

- Frame stack 4: max. over frames 4 and 5
                 max. over frames 5 and 6
                 max. over frames 6 and 7
                 max. over frames 7 and 8

Note that there's a big difference: frame skip then frame stack gives the agent much less temporal
scope than frame stack then frame skip. In the former, the agent has access to 12 frames' worth of
observations, whereas in the latter, only 4 frames' worth.


## Environment tweaks

'Training details':
- "As the scale of scores varies greatly from game to game, we clipped all positive rewards at 1 and
   all negative rewards at -1, leaving 0 rewards unchanged."
- "For games where there is a life counter, the Atari 2600 emulator also sends the number of lives
   left in thegame, which is then used to mark the end of an episode during training."

'Evaluation procedure':
- "The trained agents were evaluated by playing each game 30 times for up to 5 min each time with
   different initial random conditions ('no-op'; see Extended Data Table 1)."
  
Extended Data Table 1 lists "no-op max" as 30 (set in params.py).
   
We implement all these steps using a modular set of wrappers, heavily inspired by Baselines'
atari_wrappers.py (https://git.io/vhWWG).
"""


def get_noop_action_index(env):
    action_meanings = env.unwrapped.get_action_meanings()
    try:
        noop_action_index = action_meanings.index('NOOP')
        return noop_action_index
    except ValueError:
        raise Exception("Unsure about environment's no-op action")


class MaxWrapper(Wrapper):
    """
    Take maximum pixel values over pairs of frames.
    """

    def __init__(self, env):
        Wrapper.__init__(self, env)
        self.frame_pairs = deque(maxlen=2)

    def reset(self):
        obs = self.env.reset()
        self.frame_pairs.append(obs)

        # The first frame returned should be the maximum of frames 0 and 1.
        # We get frame 0 from env.reset(). For frame 1, we take a no-op action.
        noop_action_index = get_noop_action_index(self.env)
        obs, _, done, _ = self.env.step(noop_action_index)
        if done:
            raise Exception("Environment signalled done during initial frame "
                            "maxing")
        self.frame_pairs.append(obs)

        return np.max(self.frame_pairs, axis=0)

    def step(self, action):
        obs, reward, done, info = self.env.step(action)
        self.frame_pairs.append(obs)
        obs_maxed = np.max(self.frame_pairs, axis=0)
        return obs_maxed, reward, done, info


class ExtractImageWrapper(Wrapper):
    """
    Convert observations from bytearray to image
    """
    def reset(self):
        state = self.env.reset()
        self.state = image_decode(state)

    def step(self, action):
        obs, reward, done, info = self.env.step(action)
        return image_decode(obs), reward, done, info


class ExtractLuminanceAndScaleWrapper(ObservationWrapper):
    """
    Convert observations from colour to grayscale, then scale to 84 x 84
    """

    def __init__(self, env):
        ObservationWrapper.__init__(self, env)
        # Important so that gym's play.py picks up the right resolution
        self.observation_space = spaces.Box(low=0, high=255, shape=(84, 84), dtype=np.uint8)

    def observation(self, obs):
        obs = cv2.cvtColor(obs, cv2.COLOR_RGB2GRAY)
        # Bilinear interpolation
        obs = cv2.resize(obs, (84, 84), interpolation=cv2.INTER_LINEAR)
        return obs


class FrameStackWrapper(Wrapper):
    """
    Stack the most recent 4 frames together.
    """

    def __init__(self, env):
        Wrapper.__init__(self, env)
        self.frame_stack = deque(maxlen=4)
        low = np.tile(env.observation_space.low[..., np.newaxis], 4)
        high = np.tile(env.observation_space.high[..., np.newaxis], 4)
        dtype = env.observation_space.dtype
        self.observation_space = Box(low=low, high=high, dtype=dtype)

    def _get_obs(self):
        obs = np.array(self.frame_stack)
        # Switch from (4, 84, 84) to (84, 84, 4), so that we have the right order for inputting
        # directly into the convnet with the default channels_last
        obs = np.moveaxis(obs, 0, -1)
        return obs

    def reset(self):
        obs = self.env.reset()
        self.frame_stack.append(obs)
        # The first observation returned should be a stack of observations 0 through 3. We get
        # observation 0 from env.reset(). For the rest, we take no-op actions.
        noop_action_index = get_noop_action_index(self.env)
        for _ in range(3):
            obs, _, done, _ = self.env.step(noop_action_index)
            if done:
                raise Exception("Environment signalled done during initial "
                                "frame stack")
            self.frame_stack.append(obs)
        return self._get_obs()

    def step(self, action):
        obs, reward, done, info = self.env.step(action)
        self.frame_stack.append(obs)
        return self._get_obs(), reward, done, info


class FrameSkipWrapper(Wrapper):
    def __init__(self, env, k=4):
        Wrapper.__init__(self, env)
        self.k = k

    """
    Repeat the chosen action for k frames, only returning the last frame.
    """
    def reset(self):
        return self.env.reset()

    def step(self, action):
        reward_sum = 0
        for _ in range(self.k):
            obs, reward, done, info = self.env.step(action)
            reward_sum += reward
            if done:
                break
        return obs, reward_sum, done, info


class RandomStartWrapper(Wrapper):
    """
    Start each episode with a random number of no-ops.
    """

    def __init__(self, env, max_n_noops):
        Wrapper.__init__(self, env)
        self.max_n_noops = max_n_noops

    def step(self, action):
        return self.env.step(action)

    def reset(self):
        obs = self.env.reset()
        n_noops = np.random.randint(low=0, high=self.max_n_noops + 1)
        noop_action_index = get_noop_action_index(self.env)
        for _ in range(n_noops):
            obs, _, done, _ = self.env.step(noop_action_index)
            if done:
                obs = self.env.reset()
        return obs

class NormalizeObservationsWrapper(ObservationWrapper):
    """
    Normalize observations to range [0, 1].
    """

    def __init__(self, env):
        ObservationWrapper.__init__(self, env)
        self.observation_space = spaces.Box(low=0.0, high=1.0, shape=env.observation_space.shape,
                                            dtype=np.float32)

    def observation(self, obs):
        return obs / 255.0


class ClipRewardsWrapper(RewardWrapper):
    """
    Clip rewards to range [-1, +1].
    """

    def reward(self, reward):
        return np.clip(reward, -1, +1)


class EndEpisodeOnLifeLossWrapper(Wrapper):
    """
    Send 'episode done' when life lost. (Baselines' atari_wrappers.py claims that this helps with
    value estimation. I guess it makes it clear that only actions since the last loss of life
    contributed significantly to any rewards in the present.)
    """
    def __init__(self, env):
        Wrapper.__init__(self, env)
        self.done_because_life_lost = False
        self.reset_obs = None

    def step(self, action):
        lives_before = self.env.unwrapped.ale.lives()
        obs, reward, done, info = self.env.step(action)
        lives_after = self.env.unwrapped.ale.lives()

        if done:
            self.done_because_life_lost = False
        elif lives_after < lives_before:
            self.done_because_life_lost = True
            self.reset_obs = obs
            done = True

        return obs, reward, done, info

    def reset(self):
        assert self.done_because_life_lost is not None
        # If we sent the 'episode done' signal after a loss of a life, then we'll probably get a
        # reset signal next. But we shouldn't actually reset! We should just keep on playing until
        # the /real/ end-of-episode.
        if self.done_because_life_lost:
            self.done_because_life_lost = None
            return self.reset_obs
        else:
            return self.env.reset()

def generic_preprocess(env, max_n_noops):
    return env

