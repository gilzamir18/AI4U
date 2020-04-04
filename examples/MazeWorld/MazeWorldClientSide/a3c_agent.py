from unityremote.ml.a3c.train import run as run_train
from unityremote.ml.a3c.run_checkpoint import run as run_test
from unityremote.ml.a3c.preprocessing import FrameSkipWrapper
from unityremote.utils import environment_definitions
import UnityRemoteGym
import numpy as np
import argparse
from gym.core import Wrapper
from unityremote.utils import image_from_str
from collections import deque
import time

IMAGE_SHAPE = (40, 40, 2)
ARRAY_SIZE = 7
ACTION_SIZE = 7

def make_inference_network(obs_shape, n_actions, debug=False, extra_inputs_shape=None):
    import tensorflow as tf
    from unityremote.ml.a3c.multi_scope_train_op import make_train_op 
    from unityremote.ml.a3c.utils_tensorflow import make_grad_histograms, make_histograms, make_rmsprop_histograms, \
        logit_entropy, make_copy_ops

    observations = tf.placeholder(tf.float32, [None] + list(obs_shape))
    proprioceptions = tf.placeholder(tf.float32, (None, ARRAY_SIZE) )
    
    normalized_obs = tf.keras.layers.Lambda(lambda x : x/3.0)(observations)

    # Numerical arguments are filters, kernel_size, strides
    conv1 = tf.keras.layers.Conv2D(16, (1,1), (1,1), activation='relu', name='conv1')(normalized_obs)
    if debug:
        # Dump observations as fed into the network to stderr for viewing with show_observations.py.
        conv1 = tf.Print(conv1, [observations], message='\ndebug observations:',
                         summarize=2147483647)  # max no. of values to display; max int32
    
    conv2 = tf.keras.layers.Conv2D(16, (3,3), (1,1), activation='relu', name='conv2')(conv1)
    #conv3 = tf.layers.conv2d(conv2, 16, 3, 1, activation=tf.nn.relu, name='conv3')

    phidden = tf.keras.layers.Dense(30, activation='relu', name='phidden')(proprioceptions[:, 0:ARRAY_SIZE])
    phidden2 = tf.keras.layers.Dense(30, activation='relu', name='phidden2')(phidden)

    flattened = tf.keras.layers.Flatten()(conv2)

    expanded_features = tf.keras.layers.Concatenate()([flattened, phidden2])

    hidden = tf.keras.layers.Dense(256, activation='relu', name='hidden')(expanded_features)
    #hidden2 = tf.keras.layers.Lambda(lambda x: x * proprioceptions[:,9:10])(hidden)
    #action_logits = tf.keras.layers.Dense(n_actions, activation=None, name='action_logits')(hidden2)
    action_logits = tf.keras.layers.Dense(n_actions, activation=None, name='action_logits')(hidden)
    action_probs = tf.nn.softmax(action_logits)
    #values = tf.layers.Dense(1, activation=None, name='value')(hidden2)
    values = tf.layers.Dense(1, activation=None, name='value')(hidden)


    # Shape is currently (?, 1)
    # Convert to just (?)
    values = values[:, 0]

    layers = [conv1, conv2, phidden, phidden2, flattened, expanded_features, hidden]

    return (observations, proprioceptions), action_logits, action_probs, values, layers


def parse_args():
    parser = argparse.ArgumentParser()
    parser.add_argument("--run",
                        choices=['train', 'test'],
                        default='train')
    parser.add_argument('--path', default='.')
    parser.add_argument('--preprocessing', choices=['generic', 'user_defined'])
    return parser.parse_args()

def get_frame_from_fields(fields):
    imgdata = image_from_str(fields['frame'], 40, 40)
    return imgdata


class Agent:
    def __init__(self):
        self.hist = deque(maxlen=2)
        self.good_touchs = 0

    def __make_state__(hist, env_info):
        seq = np.array(hist)
        seq = np.moveaxis(seq, 0, -1)
        state = (seq, np.zeros(ARRAY_SIZE))
        state[1][0] = env_info['energy']/50.0
        state[1][1] = env_info['touchID']/3.0
        state[1][2] = env_info['tx']
        state[1][3] = env_info['ty']
        state[1][4] = env_info['tz']
        state[1][5] = env_info['fx']
        state[1][6] = env_info['fz']
        return state

    def reset(self, env):
        env_info = env.remoteenv.step('restart')
        frame = get_frame_from_fields(env_info)
        self.hist.append( frame )
        self.hist.append( frame )
        self.good_touchs = 0
        return Agent.__make_state__(self.hist, env_info)

    def act(self, env, action=None, info=None):
        sum_rewards = 0
        touchID = 0
        energy = 0
        for i in range(4):
            env_info = env.one_step(action)

            if env_info['done']:
                sum_rewards += env_info['reward']                
                if env_info['touchID'] == 2:
                    self.good_touchs += 1
                break
            
            env_info = env.remoteenv.step("get_result")
            sum_rewards += env_info['reward']
            if env_info['touchID'] == 2:
                self.good_touchs += 1

            if env_info['done']:
                break

        if env_info['done']:
            if sum_rewards == 0:
                sum_rewards = self.good_touchs
                if sum_rewards > 50:
                    sum_rewards = 50

        frame = get_frame_from_fields(env_info)
        self.hist.append(frame)

        return Agent.__make_state__(self.hist, env_info), sum_rewards, env_info['done'], env_info

def make_env_def():
        environment_definitions['state_shape'] = IMAGE_SHAPE
        environment_definitions['action_shape'] = (ACTION_SIZE,)
        environment_definitions['actions'] = [('walk', 1), ('run', 15), ('walk_in_circle', 1.0), ('walk_in_circle', -1.0), ('pickup', True), ('pickup', False), ('noop', -1), ('get_result', -1)]
        environment_definitions['agent'] = Agent
        environment_definitions['extra_inputs_shape'] = (ARRAY_SIZE,)
        environment_definitions['make_inference_network'] = make_inference_network

def train():
        args = ['--n_workers=8', '--steps_per_update=30', 'UnityRemote-v0']
        make_env_def()
        run_train(environment_definitions, args)

def test(path):
        args = ['UnityRemote-v0', path]
        make_env_def()
        run_test(environment_definitions, args)


if __name__ == '__main__':
   args = parse_args()
   if args.run == "train":
        train()
   elif args.run == "test":
        test(args.path)
