from unityremote.ml.a3c.train import run as run_train
from unityremote.ml.a3c.run_checkpoint import run as run_test
from unityremote.utils import environment_definitions
import UnityRemoteGym
from UnityRemoteGym import BasicAgent
import numpy as np
import argparse
from gym.core import Wrapper
from unityremote.utils import image_decode
from collections import deque


IMAGE_SHAPE = (20, 20, 4)
ARRAY_SIZE = 3
ACTION_SIZE = 8


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


def parse_args():
    parser = argparse.ArgumentParser()
    parser.add_argument("--run",
                        choices=['train', 'test'],
                        default='train')
    parser.add_argument("--id", default='0')
    parser.add_argument('--path', default='.')
    parser.add_argument('--preprocessing', choices=['generic', 'user_defined'])
    return parser.parse_args()

def get_frame_from_fields(fields):
    imgdata = image_decode(fields['frame'], 20, 20)
    return imgdata

class Agent(BasicAgent):
    def __init__(self):
        super().__init__()
        self.energy = 0
        self.buf = deque(maxlen=4)
        mem = np.zeros(shape=(20, 20))
        self.buf.append(mem.copy())
        self.buf.append(mem.copy())
        self.buf.append(mem.copy())
        self.buf.append(mem.copy())
        self.entropy_hist = deque(maxlen=30)

    def __make_state__(env_info, imageseq):
        proprioceptions = np.zeros(ARRAY_SIZE)
        frameseq = np.array(imageseq, dtype=np.float32)
        frameseq = np.moveaxis(frameseq, 0, -1)
        proprioceptions[0] = env_info['touched']
        proprioceptions[1] = env_info['energy']/300.0
        proprioceptions[2] = env_info['signal']
        proprioception = np.array(proprioceptions, dtype=np.float32)
        return (frameseq, proprioception)

    def reset(self, env):
        env_info = env.remoteenv.step("restart")
        frame = get_frame_from_fields(env_info)
        self.buf.append(frame)
        self.buf.append(frame)
        self.buf.append(frame)
        self.buf.append(frame)
        self.energy = env_info['energy']
        self.avg_entropy = 0.0;
        return Agent.__make_state__(env_info, self.buf)



    def calc_reward(self, env_info):
        reward = 0
        delta = env_info['energy'] - self.energy
        if self.energy > 200:
            if delta > 0:
                reward = -delta
        else:
            reward = delta
            if reward < 0:
                reward = 0;
        return reward

    def act(self, env, action=0, info=None):
        reward_sum = 0
        touched = -1
        for _ in range(8):
            env.one_step(action)
            env_info = env.remoteenv.step("get_status")
            reward = self.calc_reward(env_info)
            if reward != 0 or env_info['done']:
                #print(reward)
                #print(env_info['touched'])
                break

        reward_sum += reward

        self.energy = env_info['energy']

        if reward_sum == 0:
            reward_sum = np.random.choice([0.0, 0.01])

        action_probs, value_estimate = info
            
        if reward_sum == 0 and len(self.entropy_hist)>0:
            entropy_list = np.array(self.entropy_hist)
            em = np.mean(entropy_list)
            st = np.std(entropy_list)
            e = -np.sum(action_probs * np.log2(action_probs)/np.log2(len(action_probs)) )
            if (e-em > st):
                reward_sum = 0.01
        Shannon2 = -np.sum(action_probs * np.log2(action_probs)/np.log2(len(action_probs)) )
        self.entropy_hist.append(Shannon2)

        frame = get_frame_from_fields(env_info)
        self.buf.append(frame)

        state = Agent.__make_state__(env_info, self.buf)

        return (state, reward_sum, env_info['done'], env_info)

def make_env_def(id=0):
        environment_definitions['state_shape'] = IMAGE_SHAPE
        environment_definitions['action_shape'] = (ACTION_SIZE,)
        environment_definitions['actions'] = [('act',0), ('act', 1), ('act', 3), ('act', 4), ('act', 8), ('act', -1), ('act', 10), ('act', 11)]
        environment_definitions['agent'] = Agent
        environment_definitions['extra_inputs_shape'] = (ARRAY_SIZE,)
        environment_definitions['make_inference_network'] = make_inference_network
        environment_definitions['input_port'] = 8080 + id
        environment_definitions['output_port'] = 7070 + id

def train():
        args = ['--n_workers=8', '--steps_per_update=30', 'UnityRemote-v0']
        make_env_def()
        run_train(environment_definitions, args)

def test(path, id=0):
        args = ['UnityRemote-v0', path]
        make_env_def(id)
        run_test(environment_definitions, args)


if __name__ == '__main__':
   args = parse_args()
   if args.run == "train":
        train()
   elif args.run == "test":
        test(args.path, int(args.id))
