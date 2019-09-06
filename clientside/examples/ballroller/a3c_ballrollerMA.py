from unityremote.ml.a3c.train import run as run_train
from unityremote.ml.a3c.run_checkpoint import run as run_test
from unityremote.core import environment_definitions
import UnityRemoteGym
import numpy as np
import argparse

def parse_args():
    parser = argparse.ArgumentParser()
    parser.add_argument("--run",
                        choices=['train', 'test'],
                        default='train')
    parser.add_argument('--path', default='.')
    return parser.parse_args();

def get_state_from_fields(fields):
	return np.array([fields['tx'], fields['tz'], fields['vx'], fields['vz'], fields['x'], fields['y'],	fields['z']])

def state_wrapper(fields):
        state = get_state_from_fields(fields)
        done = fields['done']
        reward = fields['reward']
        info = fields
        return (state, reward, done, info)

def train():
        args = ['--n_workers=2', 'UnityRemote-v0']
        environment_definitions['state_shape'] = (7,)
        environment_definitions['action_shape'] = (5,)
        environment_definitions['actions'] = [('fx',0.1), ('fx', -0.1), ('fz', 0.1), ('fz', -0.1), ('noop', 0.0)]
        environment_definitions['action_meaning'] = ['tx_right', 'tx_left', 'tz_toward', 'tz_backward', 'NOOP']
        environment_definitions['state_wrapper'] = state_wrapper
        run_train(environment_definitions, args)

def test(path):
        args = ['UnityRemote-v0', path, '--n_workers=2']
        environment_definitions['state_shape'] = (7,)
        environment_definitions['action_shape'] = (5,)
        environment_definitions['actions'] = [('fx',0.1), ('fx', -0.1), ('fz', 0.1), ('fz', -0.1), ('noop', 0.0)]
        environment_definitions['action_meaning'] = ['tx_right', 'tx_left', 'tz_toward', 'tz_backward', 'NOOP']
        environment_definitions['state_wrapper'] = state_wrapper
        run_test(environment_definitions, args)


if __name__ == '__main__':
   args = parse_args()
   if args.run == "train":
        train()
   elif args.run == "test":
        test(args.path)
