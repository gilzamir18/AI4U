from ai4u.ml.a3c.train import run as run_train
from ai4u.ml.a3c.run_checkpoint import run as run_test
from ai4u.utils import environment_definitions
import AI4UGym
from AI4UGym import BasicAgent
import numpy as np
import argparse

def to_image(img):
    imgdata = image_decode(img, #IW, #IH)
    return imgdata

'''
This method extract environemnt state from a remote environment response.
'''
def get_state_from_fields(fields):
    #TPL_RETURN_STATE

'''
It's necessary overloading the BasicAgent because server response (remote environment) don't have default field 'frame' as state.
'''
class Agent(BasicAgent):
    def __init__(self):
        BasicAgent.__init__(self)

    def reset(self, env):
        env_info = env.remoteenv.step("restart")
        return get_state_from_fields(env_info)

    def act(self, env, action, info=None):
        reward = 0
        for _ in range(#SKIP_FRAMES):
            envinfo = env.one_step(action)
            reward += envinfo['reward']
            if envinfo['done']:
                break
        state = get_state_from_fields(envinfo)
        return state, reward, envinfo['done'], envinfo


def parse_args():
    parser = argparse.ArgumentParser()
    parser.add_argument("--run",
                        choices=['train', 'test'],
                        default='train')
    parser.add_argument('--path', default='.')
    parser.add_argument('--preprocessing', choices=['generic', 'user_defined'])
    return parser.parse_args()

def make_env_def():
        environment_definitions['state_shape'] = #TPL_INPUT_SHAPE
        #DISABLE52 environment_definitions['extra_inputs_shape'] = (#ARRAY_SIZE,)
        environment_definitions['action_shape'] = #TPL_OUTPUT_SHAPE
        environment_definitions['actions'] = #TPL_ACTIONS
        environment_definitions['agent'] = Agent

def train():
        args = ['--n_workers=#WORKERS', 'AI4U-v0']
        make_env_def()
        run_train(environment_definitions, args)

def test(path):
        args = ['AI4U-v0', path,]
        make_env_def()
        run_test(environment_definitions, args)


if __name__ == '__main__':
   args = parse_args()
   if args.run == "train":
        train()
   elif args.run == "test":
        test(args.path)
