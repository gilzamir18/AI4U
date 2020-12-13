from ai4u.ml.a3c.train import run as run_train
from ai4u.ml.a3c.run_checkpoint import run as run_test
from ai4u.utils import environment_definitions
import AI4UGym
from AI4UGym import BasicAgent
import numpy as np
import argparse
from collections import deque
from ai4u.utils import image_decode

#NETWORKdef make_inference_network(obs_shape, n_actions, debug=False, extra_inputs_shape=None):
#NETWORK    import tensorflow as tf
#NETWORK    from ai4u.ml.a3c.multi_scope_train_op import make_train_op 
#NETWORK    from ai4u.ml.a3c.utils_tensorflow import make_grad_histograms, make_histograms, make_rmsprop_histograms, logit_entropy, make_copy_ops
#NETWORK    observations = tf.placeholder(tf.float32, [None] + list(obs_shape))
#NETWORK    normalized_obs = tf.keras.layers.Lambda(lambda x : x/#NUMOBJ)(observations)
#NETWORK    conv1 = tf.keras.layers.Conv2D(128, (2,2), (1,1), activation='relu', name='conv1')(normalized_obs)
#NETWORK    if debug:
#NETWORK	    conv1 = tf.Print(conv1, [observations], message='\ndebug observations:', summarize=2147483647)
#NETWORK    conv2 = tf.keras.layers.Conv2D(128, (2,2), (2,2), activation='relu', name='conv2')(conv1)
#NETWORK    flattened = tf.keras.layers.Flatten()(conv2)
#NETWORK    hidden1 = tf.keras.layers.Dense(512, activation='relu', name='hidden1')(flattened)
#NETWORK    hidden2 = tf.keras.layers.Dense(64, activation='relu', name='hidden2')(hidden1)
#NETWORK    action_logits = tf.keras.layers.Dense(n_actions, activation=None, name='action_logits')(hidden2)
#NETWORK    action_probs = tf.nn.softmax(action_logits)
#NETWORK    values = tf.layers.Dense(1, activation=None, name='value')(hidden2)
#NETWORK    values = values[:, 0]
#NETWORK    layers = [conv1, conv2, hidden1, hidden2]
#NETWORK    return observations, action_logits, action_probs, values, layers


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
        #RAYCASTING1self.history = deque(maxlen=#HISTSIZE)
        #RAYCASTING2for _ in range(#HISTSIZE):
            #RAYCASTING1self.history.append( np.zeros( (#SHAPE1, #SHAPE2) ) )

    def __get_state__(self, env_info):
        li, img = get_state_from_fields(env_info)
        state = None
        if img is not None:
            self.history.append(img)
            frameseq = np.array(self.history, dtype=np.float32)
            frameseq = np.moveaxis(frameseq, 0, -1)
            if li is None:
                state = [li, frameseq]
            else:
                state = frameseq
        elif li is not None:
            state = li
        return state

    def reset(self, env):
        env_info = env.remoteenv.step("restart")
        return self.__get_state__(env_info)

    def act(self, env, action, info=None):
        reward = 0
        envinfo = {}
        for _ in range(8):
            envinfo = env.one_stepfv(action)
            reward += envinfo['reward']
            if envinfo['done']:
                break
        return self.__get_state__(envinfo), reward, envinfo['done'], envinfo

def parse_args():
    parser = argparse.ArgumentParser()
    parser.add_argument("--run",
                        choices=['train', 'test'],
                        default='train')
    parser.add_argument('--path', default='.')
    parser.add_argument('--preprocessing', choices=['generic', 'user_defined'])
    return parser.parse_args()

def make_env_def():
        #DISABLE51environment_definitions['state_shape'] = #TPL_INPUT_SHAPE
        #DISABLE52environment_definitions['extra_inputs_shape'] = (#ARRAY_SIZE,)
        #NETWORKenvironment_definitions['make_inference_network'] = make_inference_network
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
