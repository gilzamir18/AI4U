from unityremote.ml.a3c.train import run as run_train
from unityremote.ml.a3c.run_checkpoint import run as run_test
from unityremote.utils import environment_definitions
import UnityRemoteGym
from UnityRemoteGym import BasicAgent
import numpy as np
import argparse

'''
This method extract environemnt state from a remote environment response.
'''
def get_state_from_fields(fields):
    return np.array([fields['tx'], fields['tz'], fields['vx'], fields['vz'], fields['x'], fields['y'],  fields['z']])


'''
It's necessary overloading the BasicAgent because server response (remote environment) don't have default field 'frame' as state.
'''
class Agent(BasicAgent):
    def __init__(self):
        BasicAgent.__init__(self)

    def act(self, env, action, info=None):
        envinfo = env.one_step(action)
        state = get_state_from_fields(envinfo)
        return state, envinfo['reward'], envinfo['done'], envinfo

def parse_args():
    parser = argparse.ArgumentParser()
    parser.add_argument("--run",
                        choices=['train', 'test'],
                        default='train')
    parser.add_argument('--path', default='.')
    parser.add_argument('--preprocessing', choices=['generic', 'user_defined'])
    return parser.parse_args()

def make_env_def():
        environment_definitions['state_shape'] = (7,)
        environment_definitions['action_shape'] = (5,)
        environment_definitions['actions'] = [('fx',0.1), ('fx', -0.1), ('fz', 0.1), ('fz', -0.1), ('noop', 0.0)]
        environment_definitions['action_meaning'] = ['tx_right', 'tx_left', 'tz_toward', 'tz_backward', 'NOOP']
        environment_definitions['agent'] = Agent

def train():
        args = ['--n_workers=2', 'UnityRemote-v0']
        make_env_def()
        run_train(environment_definitions, args)

def test(path):
        args = ['UnityRemote-v0', path,]
        make_env_def()
        run_test(environment_definitions, args)


if __name__ == '__main__':
   args = parse_args()
   if args.run == "train":
        train()
   elif args.run == "test":
        test(args.path)
