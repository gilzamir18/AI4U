## UnityRemote with Gym

We can use unity remote as a gym environment, but there is a complex flow in this case. First, you import UnityRemoteGym and gym modules

    import UnityRemoteGym, gym

After this, you can make a environment instance:

    env = gym.make('UnityRemote-v0')
 
Environment object `env`  performed communication with the server-side of your unity application. It's necessary to define the environment configuration using a dictionary. For example:

	environment_definitions = {'host': '127.0.0.1', 'input_port': 8080, 'output_port': 7070, 'n_envs': 1,
                                "action_shape": (1, ), "state_shape": (1, ), 'min_value': -100.0, 
                                    'max_value': 100.0, 'state_type': np.float32,  'actions': [], 
                                        'action_meaning':[], 'state_wrapper': lambda s, e: s,
                                        'make_inference_network': make_inference_network}

Table 1: show the means of each key in dictionary `environment_definitions`.

| Key             |                                    Defintion                                          |
|-----------------|---------------------------------------------------------------------------------------|
| host            |  Host name or ip of the remote application.                                           |
| input_port      |  Client side input port. Also, it is server side output port.                         |
| output_port     |  Client side output port. Also, it is server side input port.                         |
| n_envs          |  Number of environments. We use n_envs > 1 if there is several agents in server side. |
| action_shape    |  Action shape is a number of actions (used by A3C [1] implementation).                                              |
| state_shape     |  Shape of the environment's state (used by A3C implementation).                                                    |
| min_value       |  Minimum value in the state content (used by A3C implementation).                                                  |
| max_value       |  Maximum value in the state content (used by A3C implementation).                                                  |
| state_type      |  Type of the state content (used by A3C implementation).                                                          |
| actions         |  List of pairs (action_name, actions_parameters).                                     |
| actions_meaning |  Action meaning for each action in actions.                                           |
| state_wrapper   |  Method or callable object that transform dictionary returned by RemoteEnv in gym compatible data.                                                                                          |
| make_inference_network | Method used only by A3C algorithm.                                             |


The next step is to set up the environment. To do this, you call the method 'configure' from the object `env` passing the dictionary `environment_definition` as an argument. If you don't want to create the dictionary from scratch, you can import a preconfigured dictionary from the `unityremote.utils` module. Most parameters do not need to be specified. For example, the following attributes are likely to have to be modified:

	from unityremote.utils import environment_definitions as env_def
	#set new values
	env_def['actions'] = [('tx', 10)]
	env_def['actions_meaning'] = ['horizontal movement']
	env_def['input_port'] = 8080
	env_def['output_port'] = 8081
	env.configure(env_def)

When you perform `reset`, the environment implementation (`env`) sends an especial command named `restart` to your unity application and returns the first environment's state:

    initial_state = env.reset()

The reset command should restart the environment for a new simulation episode. Finally, to perform an action, simply execute the step command with the index of the action you want to send to the remote environment.

	next_state = env.step(0)

The complete example used in this guide can be found at `UnityRemote/examples/CubeAgent/CubeExampleClient/cubeagentwithgym.py`. Server-side implementation can be found at `UnityRemote/examples/CubeAgent/CubeExampleUnity`.

[1] A3C: Asynchronous Advantage Actor-Critic.


