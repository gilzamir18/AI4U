# UnityRemote with Gym

We can use unity remote as a gym environment, but there is a complex flow in this case. First, you import UnityRemoteGym and gym modules

    import UnityRemoteGym, gym

After this, you can make a environment instance:

    env = gym.make('UnityRemote-v0')
 
Environment object `env`  performed communication with the server-side of your unity application. It's necessary to define the environment configuration using a dictionary. For example:

	environment_definitions = {'host': '127.0.0.1', 'input_port': 8080, 'output_port': 7070, 'n_envs': 1,
                                "action_shape": (1, ), "state_shape": (1, ), 'min_value': -100.0, 
                                    'max_value': 100.0, 'state_type': np.float32,  'actions': [], 
                                        'action_meaning':[], 'make_inference_network': make_inference_network}

Table 1: show the means of each key in dictionary `environment_definitions`.

| Key             |                                    Defintion                                          |
|-----------------|---------------------------------------------------------------------------------------|
| host            |  Host name or ip of the remote application.                                           |
| input_port      |  Client side input port. Also, it is server side output port.                         |
| output_port     |  Client side output port. Also, it is server side input port.                         |
| n_envs          |  Number of environments. We use n_envs > 1 if there is several agents in server side. |
| action_shape    |  Action shape is a number of actions (used by A3C [1] implementation).                |                             |				  |																						  |
| state_shape     |  Shape of the environment's state (used by A3C implementation).                       |
| min_value       |  Minimum value in the state content (used by A3C implementation).                     |
| max_value       |  Maximum value in the state content (used by A3C implementation).                     |
| state_type      |  Type of the state content (used by A3C implementation).                              |
| actions         |  List of pairs (action_name, actions_parameters).                                     |
| agent           |  Agent is a agent of the class BasicAgent											  |
| actions_meaning |  Action meaning for each action in actions.                                           |
| make_inference_network | Method used only by A3C algorithm.                                             |


The next step is to set up the environment. To do this, you call the method 'configure' from the object `env` passing the dictionary `environment_definition` as an argument. If you don't want to create the dictionary from scratch, you can import a preconfigured dictionary from the `unityremote.utils` module. Most parameters do not need to be specified. For example, the following attributes are likely to have to be modified:

	from unityremote.utils import environment_definitions as env_def
	#set new values
	env_def['actions'] = [('tx', 10)]
	env_def['actions_meaning'] = ['horizontal movement']
	env_def['input_port'] = 8080
	env_def['output_port'] = 8081
	env.configure(env_def)

When you perform `reset`, the environment implementation (`env`) sends an especial command named `restart` to your Unity application and returns the first environment's state:

    initial_state = env.reset()

The reset command should restart the environment for a new simulated episode. Finally, to perform an action, simply execute the step command with the index of the action you want to send to the remote environment.

	next_state = env.one_step(0) #run action named 'tx'


#The Agent Class

If you don't specify an *agent* field, UnityRemoteGym assumes the following agent behavior as the default

	class BasicAgent:
	    def reset(self, env):
	        envinfo = env.remoteenv.step('restart')
	        if 'state' in envinfo:
	            return envinfo['state']
	        else:
	            return None

	    def act(self, env, action, info=None):
	        envinfo = env.one_step(action)
	        state = None
	        reward = 0
	        if 'state' in envinfo:
	            state = envinfo['state']

	        if 'reward' in envinfo:
	            reward = envinfo['reward']

	        done = True
	        if 'done' in envinfo:
	            done = envinfo['done']

	        return state, reward, done, {}

The method *reset* receives the environment reference *env* and returns the initial state of a new episode. Environment object has methods and objects that allow communicating the client-side to the server-side component of your application. Remember that the client-side is the component of your application written in Python, while the server-side is a component of your application based on Unity. For example, the *env.remoteenv* object implements a protocol to communicate with the server-side application. The method *step* of *env.remoteenv* sends a message directly to the server. The method *one_step* of *env* sends an action according to the specified list of actions in *environment_definitions*.  Awhile the method *step* receives a string and a list of arguments, the method  *one_step* receives an integer indicating an action from the list of actions specified in *environment_definitions*.

The method *act* receives an environment reference (named *env*), the action index (named *action*) and the extra information (named *env*). The environment reference provides communication with the remote environment. The action index is an action index in a list of actions specified in the environment definition. Extra information is a feedback channel from decision model decision, for example, when an A3C neural network returns a decision, it's returned the probabilities of each action form a set of actions.

If you use this basic agent class, your Unity application should have support for a command named *restart*, support the actions specified in *environment_definitions*, and return fields named *state*, *reward* and *done*.

The default behavior of the UnityRemoteGym perhaps is not sufficient for your necessities. You can modify this bahavior by inheriting the class BasicAgent from the module UnityRemoteGym. 

	from UnityRemoteGym import BasicAgent

Then, you need to override the methods *act* and *reset*. For example

	class Agent(BasicAgent):
		def __init__(self):
			self.memory = []
		
		def reset(self, env):
			env_info = env.remoteenv.step("restart") #restart episode and return its first state. 
			self.initial_state = env_info['state'] #set initial state
			return self.initial_state #this returns initial state
		
		def act(self, env, action=0, info=None):
			env_info = env.one_step(action) #run one step of simulation
			final_state = env_info['state'] #get resulting state
			self.memory.append( (self.initial_state, action, env_info['reward'], final_state) ) #register a trasition
			if not env_info['done']: #if episode not ends
				initial_state = final_state[:] #The initial state of the next traisition is the final state of the previous trasition
		  	return final_state, env_info['reward'], env_info['done'], env_info #It's necessary to follow the OpenAI Gym protocol.





# Final Considerations

The complete example used in this guide can be found at [here](/examples/CubeAgent/CubeExampleClient/).

[1] A3C: Asynchronous Advantage Actor-Critic.


