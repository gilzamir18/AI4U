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


IMAGE_SHAPE = (20, 20, 4)
ARRAY_SIZE = 2
ACTION_SIZE = 14

def make_inference_network(obs_shape, n_actions, debug=False, extra_inputs_shape=None):
    import tensorflow as tf
    from unityremote.ml.a3c.multi_scope_train_op import make_train_op 
    from unityremote.ml.a3c.utils_tensorflow import make_grad_histograms, make_histograms, make_rmsprop_histograms, \
        logit_entropy, make_copy_ops

    observations = tf.placeholder(tf.float32, [None] + list(obs_shape))
    proprioceptions = tf.placeholder(tf.float32, (None, ARRAY_SIZE) )
    
    normalized_obs = tf.keras.layers.Lambda(lambda x : x/6.0)(observations)

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
        self.episode_steps = 0

    def step(self, action, info=None):
        state, reward, done, env_info = self.env.step(action)

        self.episode_steps += 1

        return state, reward, done, env_info

    def reset(self):
        self.episode_steps = 0
        return self.env.reset()

def agent_preprocessing(env, max_n_noops, clip_rewards=True):
    env = FrameSkipWrapper(env, 8)
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
    imgdata = image_from_str(fields['frame'], 20, 20)
    return imgdata


class state_wrapper:
    def __init__(self):
        self.hist = deque(maxlen=4)
        self.hist.append( np.zeros( (20, 20) ) )
        self.hist.append( np.zeros( (20, 20) ) )
        self.hist.append( np.zeros( (20, 20) ) )
        self.hist.append( np.zeros( (20, 20) ) )

    def __call__(self, fields, env, action=None, info=None):
        frame = get_frame_from_fields(fields)
        self.hist.append(frame)
        
        done = fields['done']
        reward = fields['reward'];

        info = fields

        reward = np.clip(reward, -1, +1)

        proprioceptions = np.zeros(ARRAY_SIZE)
        
        proprioceptions[0] = fields['energy']/100.0
        proprioceptions[1] = fields['touchID']/10.0

        proprioception = np.array(proprioceptions, dtype=np.float32)
 
        seq = np.array(self.hist)
        seq = np.moveaxis(seq, 0, -1)
        if done:
            self.hist.append( np.zeros( (20, 20) ) )
            self.hist.append( np.zeros( (20, 20) ) )
            self.hist.append( np.zeros( (20, 20) ) )
            self.hist.append( np.zeros( (20, 20) ) )
        state = (seq, proprioception)
        return (state, reward, done, fields)

def make_env_def():
        speed = 100
        angular_speed = 50
        environment_definitions['state_shape'] = IMAGE_SHAPE
        environment_definitions['action_shape'] = (ACTION_SIZE,)
        environment_definitions['actions'] = [('fx', speed), ('fx', -speed), ('fy', speed), ('fy', -speed), ('left_turn', angular_speed), ('left_turn', -angular_speed), ('right_turn', angular_speed), ('right_turn', -angular_speed), ('up',speed),
                ('down', speed), ('jump', True), ('crouch', True), ('crouch', False), ('noop', -1)]
        environment_definitions['action_meaning'] = ['fx', 'fx', 'fy', 'fy', 'left_turn', 'left_turn', 'right_turn', 'right_turn', 'up', 'down', 'jump', 'crouch', 'crouch', 'NOOP']
        environment_definitions['state_wrapper'] = state_wrapper
        environment_definitions['preprocessing'] = agent_preprocessing
        environment_definitions['extra_inputs_shape'] = (ARRAY_SIZE,)
        environment_definitions['make_inference_network'] = make_inference_network

def train():
        args = ['--n_workers=4', '--preprocessing=external', '--steps_per_update=30', 'UnityRemote-v0']
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
