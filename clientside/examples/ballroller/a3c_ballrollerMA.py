from unityremote.ml.a3c.train import run as run_train
from unityremote.ml.a3c.run_checkpoint import run as run_test
from unityremote.utils import environment_definitions
import UnityRemoteGym
import numpy as np
import argparse
from gym.core import Wrapper

from unityremote.ml.a3c.preprocessing import RandomStartWrapper, FrameSkipWrapper, EndEpisodeOnLifeLossWrapper, ClipRewardsWrapper


class TestWrapper(Wrapper):
    """
    Start each episode with a random number of no-ops.
    """

    def __init__(self, env):
        Wrapper.__init__(self, env)

    def step(self, action):
        #Do anything before send your result
        return self.env.step(action)

    def reset(self):
        return self.env.reset()

def test_preprocessing(env, max_n_noops, clip_rewards=True):
    env = RandomStartWrapper(env, max_n_noops)
    env = FrameSkipWrapper(env)
    env = EndEpisodeOnLifeLossWrapper(env)
    if clip_rewards:
        env = ClipRewardsWrapper(env)
    env = TestWrapper(env)
    return env


def parse_args():
    parser = argparse.ArgumentParser()
    parser.add_argument("--run",
                        choices=['train', 'test'],
                        default='train')
    parser.add_argument('--path', default='.')
    parser.add_argument('--preprocessing', choices=['generic, image_image', 'external'])
    return parser.parse_args()

def get_state_from_fields(fields):
	return np.array([fields['tx'], fields['tz'], fields['vx'], fields['vz'], fields['x'], fields['y'],	fields['z']])

def state_wrapper(fields, env):
        state = get_state_from_fields(fields)
        done = fields['done']
        reward = fields['reward']
        info = fields
        return (state, reward, done, info)

def make_env_def():
        environment_definitions['state_shape'] = (7,)
        environment_definitions['action_shape'] = (5,)
        environment_definitions['actions'] = [('fx',0.1), ('fx', -0.1), ('fz', 0.1), ('fz', -0.1), ('noop', 0.0)]
        environment_definitions['action_meaning'] = ['tx_right', 'tx_left', 'tz_toward', 'tz_backward', 'NOOP']
        environment_definitions['state_wrapper'] = state_wrapper
        environment_definitions['preprocessing'] = test_preprocessing

def train():
        args = ['--n_workers=2', '--preprocessing=external', 'UnityRemote-v0']
        make_env_def()
        run_train(environment_definitions, args)

def test(path):
        args = ['UnityRemote-v0', path, '--n_workers=2']
        make_env_def()
        run_test(environment_definitions, args)


if __name__ == '__main__':
   args = parse_args()
   if args.run == "train":
        train()
   elif args.run == "test":
        test(args.path)
