from unityremote.ml.a3c.train import run as run_train
from unityremote.ml.a3c.run_checkpoint import run as run_test
from unityremote.utils import environment_definitions
import UnityRemoteGym
import numpy as np
import argparse
from gym.core import Wrapper
from unityremote.utils import image_decode
from unityremote.ml.a3c.preprocessing import RandomStartWrapper, EndEpisodeOnLifeLossWrapper, ClipRewardsWrapper
from collections import deque


IMAGE_SHAPE = (20, 20, 4)
ARRAY_SIZE = 3
ACTION_SIZE = 7

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
        if not done:
            obs, _, done, info = self.env.step(-1)
        return obs, reward_sum, done, info


def make_inference_network(obs_shape, n_actions, debug=False, extra_inputs_shape=None):
    import tensorflow as tf
    from unityremote.ml.a3c.multi_scope_train_op import make_train_op 
    from unityremote.ml.a3c.utils_tensorflow import make_grad_histograms, make_histograms, make_rmsprop_histograms, \
        logit_entropy, make_copy_ops

    observations = tf.placeholder(tf.float32, [None] + list(obs_shape))
    proprioceptions = tf.placeholder(tf.float32, (None, ARRAY_SIZE) )

    normalized_obs = tf.keras.layers.Lambda(lambda x : x/20.0)(observations)

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

    hidden = tf.keras.layers.Dense(512, activation='relu', name='hidden')(expanded_features)
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



class AgentWrapper(Wrapper):
    """
    Start each episode with a random number of no-ops.
    """

    def __init__(self, env):
        Wrapper.__init__(self, env)

    def step(self, action):
        state, reward, done, info = self.env.step(action)
        return state, reward, done, info

    def reset(self):
        return self.env.reset()

def agent_preprocessing(env, max_n_noops, clip_rewards=True):
    env = RandomStartWrapper(env, max_n_noops)
    #env = FrameSkipWrapper(env)
    env = FrameSkipWrapper(env, 8)
    env = EndEpisodeOnLifeLossWrapper(env)
    if clip_rewards:
        env = ClipRewardsWrapper(env)
    env = AgentWrapper(env)
    return env


def parse_args():
    parser = argparse.ArgumentParser()
    parser.add_argument("--run",
                        choices=['train', 'test'],
                        default='train')
    parser.add_argument('--path', default='.')
    parser.add_argument('--preprocessing', choices=['generic, image_image', 'external'])
    return parser.parse_args()

def get_frame_from_fields(fields):
    imgdata = image_decode(fields['frame'], 20, 20)
    return imgdata


class state_wrapper:
    def __init__(self):
        self.energy = None
        self.buf = deque(maxlen=4)
        mem = np.zeros(shape=(20, 20))
        self.buf.append(mem.copy())
        self.buf.append(mem.copy())
        self.buf.append(mem.copy())
        self.buf.append(mem.copy())
        self.proprioceptions = np.zeros(ARRAY_SIZE)

    def __call__(self, fields, env):
        frame = get_frame_from_fields(fields)

        if self.energy is None:
            self.energy = fields['energy']

        done = fields['done']
        delta = fields['energy'] - self.energy

        '''if delta > 0 and fields['id']==0:
            print('-------------------------------------DELTA---------------------------------------')
            print(delta)
            print('---------------------------------------------------------------------------------')
        '''
        reward = 0
        if not done:
            if self.energy > 200:
                if delta > 0:
                    reward = -delta
            else:
                reward = delta
                if reward < 0:
                    reward = 0;
            info = fields
            self.energy = fields['energy']
        else:
            self.energy = None

        self.buf.append(frame)
        #frameseq = np.array(self.buf, dtype=np.float32).reshape(1, 4, 20, 20)
        frameseq = np.array(self.buf, dtype=np.float32)
        frameseq = np.moveaxis(frameseq, 0, -1)
        self.proprioceptions[0] = fields['touched']
        self.proprioceptions[1] = fields['energy']/300.0
        self.proprioceptions[2] = fields['signal']
        #proprioception = np.array(self.proprioceptions, np.float32).reshape(1, len(self.proprioceptions))
        proprioception = np.array(self.proprioceptions, dtype=np.float32)
        state = (frameseq, proprioception)
        return (state, reward, done, fields)

def make_env_def():
        environment_definitions['state_shape'] = IMAGE_SHAPE
        environment_definitions['action_shape'] = (ACTION_SIZE,)
        environment_definitions['actions'] = [('act',0), ('act', 1), ('act', 3), ('act', 4), ('act', 8), ('act', -1), ('act', 10)]
        environment_definitions['action_meaning'] = ['forward', 'right', 'backward', 'left', 'jump', 'NOOP', 'PickUp']
        environment_definitions['state_wrapper'] = state_wrapper()
        environment_definitions['preprocessing'] = agent_preprocessing
        environment_definitions['extra_inputs_shape'] = (ARRAY_SIZE,)
        environment_definitions['make_inference_network'] = make_inference_network

def train():
        args = ['--n_workers=4', '--preprocessing=external', 'UnityRemote-v0']
        make_env_def()
        run_train(environment_definitions, args)

def test(path):
        args = ['UnityRemote-v0', path, '--preprocessing=external' '--n_workers=4']
        make_env_def()
        run_test(environment_definitions, args)


if __name__ == '__main__':
   args = parse_args()
   if args.run == "train":
        train()
   elif args.run == "test":
        test(args.path)
