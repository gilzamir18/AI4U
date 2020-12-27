'''
This is an example of the SAC adapter for the reinforcament learning API stable-baselines.
The SAC adapter make a proxy between AI4U and SAC implementation of the stable-baselines API.
This adapter was made based on AI4U's automatic code generator feature for PPO2 algorithm.
After code generation, i made adaptations for SAC algoritms controll simple avatar in 3D environment.
Author: Gilzamir Gomes (gilzamir@gmail.com)
'''
import gym
from gym import spaces
from stable_baselines.sac.policies import MlpPolicy
from stable_baselines import SAC
from ai4u.utils import environment_definitions
import AI4UGym
from AI4UGym import BasicAgent
import numpy as np
from ai4u.utils import image_decode
import argparse
from stable_baselines.sac.policies import FeedForwardPolicy


'''
convert byte array image in a numpy matrix.
'''
def to_image(img):
    imgdata = image_decode(img, 20, 20)
    return imgdata


#BEGIN::GENERATED CODE :: DON'T CHANGE
'''
get_state_from_fields receive a dictionaries with current state information
and returns a concatenate array with all relevants features for agent decision making.
'''
def get_state_from_fields(fields):
    return np.concatenate( (fields['AgentForward'], fields['AgentPosition'], fields['targetDiscrepancy'])) * 1/100.0


'''
Agent class implements a BasicAgent performer that receives a decision marker action 
and convert in action format for specific implemented environment. Here, we can adapt 
features as skip frame, and stacking perceptions for composite an current state as a composite 
of the perceptions historic 
'''
class Agent(BasicAgent):
    def __init__(self):
        BasicAgent.__init__(self)

    '''
    It's run in the start of the episode returning the first agent's perception.
    '''
    def reset(self, env):
        env_info = env.remoteenv.step("restart")
        return get_state_from_fields(env_info)


    '''
    It's receives a decision marker action (named action) and convert it in action format for 
    specific implemented environment. The environment is an character controller that one receives
    an 10 length array of float-point numbers. Each array element specify an movement or animation of the
    character as walk, run, walk around, jump, and pickup itens. The action value specifie the movement
    intensity, except for jump, and pickup. Action value is converted for boolean value for actions jump,
    and pickup. 
    '''
    def act(self, env, action, info=None):
        reward_sum = 0
        #convert aciton format for the suitable character controller format.
        raction = np.zeros(10)
        raction[0] = action[0] #forward
        raction[1] = action[1] #walk around
        raction[7] = action[2] #jump
        raction[9] = action[3] #pickup
        for _ in range(8): #skip 8 frames with repeated actions application.
            envinfo = env.remoteenv.stepfv('Character', raction) #send action for character controller
            reward_sum += envinfo['reward'] #agent's reward by action execution.
            if envinfo['done']: #if episode ends.
                break #break the loop
        state = get_state_from_fields(envinfo) #get last state.
        return state, reward_sum, envinfo['done'], envinfo #return state, reward, done status, and extra environment informations

'''
make_env_def configure agent-environment properties.
'''
def make_env_def():
    #environment_definitions['state_shape'] = (20, 20, 4)
    #environment_definitions['extra_inputs_shape'] = (8,)
    action_low = np.array ([-100, -100, -0.1, -0.1], dtype=np.float32) #lower action's values
    action_high = np.array([ 100,  100, 0.1, 0.1], dtype=np.float32) #higher action's values
    environment_definitions['state_shape'] = (8,) #agent's perception shape
    environment_definitions['action_space'] = spaces.Box(action_low, action_high) #continuos action's space
    environment_definitions['action_shape'] = (4, ) #number of actions
    environment_definitions['agent'] = Agent #define agent's class 
    environment_definitions['input_port'] = 8080 #match with start output port of the object BrainManager in the server side.
    environment_definitions['output_port'] = 7070 #match with start input port of the object BrainManager in the server side.
    BasicAgent.environment_definitions = environment_definitions #It's define environment properties.

make_env_def()
#END::GENERATED CODE :: DON'T CHANGE

def train():
    import tensorflow as tf
    env = gym.make('AI4U-v0') #Make the environment
    policy_kwargs = dict(act_fun=tf.nn.tanh, layers=[128, 128])
    model = SAC(MlpPolicy, env, verbose=1, policy_kwargs=policy_kwargs, gamma=0.99, learning_rate=0.0003, buffer_size=1000000, learning_starts=10, train_freq=1, batch_size=64, tau=0.005, ent_coef='auto', target_update_interval=20, gradient_steps=1, target_entropy='auto', tensorboard_log="sac2log")
    model.learn(total_timesteps=500000, log_interval=10) #Training loop
    model.save('sacmodel') #Save trained model.
    del model # remove to demonstrate saving and loading

def test():
    env = gym.make('AI4U-v0') #Make the environment
    model = SAC.load('sacmodel')
    # Enjoy test loop with trained agent
    obs = env.reset()
    while True:
        action, _states = model.predict(obs)
        env.step(action)
        #env.render()

def parse_args():
    parser = argparse.ArgumentParser()
    parser.add_argument("--run",
                        choices=['train', 'test'],
                        default='train')
    return parser.parse_args()

if __name__ == '__main__':
   args = parse_args()
   if args.run == "train":
        train()
   elif args.run == "test":
        test()

