## UnityRemote with Gym

We can use unity remote as a gym environment, but there is a complex flow in this case. First, you import UnityRemoteGym and gym modules

    import UnityRemoteGym, gym

After this, you can make a environment instance:

    env = gym.make('UnityRemote-v0')
 
 Environment object `env`  performe comunication with server-side of the your unity application. It's necessary to define the environment configuration by means of a dictionary. For example:

	environment_definitions = {'host': '127.0.0.1', 'input_port': 8080, 'output_port': 7070, 'n_envs': 1,
                                "action_shape": (1, ), "state_shape": (1, ), 'min_value': -100.0, 
                                    'max_value': 100.0, 'state_type': np.float32,  'actions': [], 
                                        'action_meaning':[], 'state_wrapper': lambda s, e: s,
                                        'make_inference_network': make_inference_network}

Table 1 show the means of each key in dictionary `environment_definitions`:

| Key             |                                    Defintion                                          |
|-----------------|---------------------------------------------------------------------------------------|
| host            |  Host name or ip of the remote application.                                           |
| input_port      |  Client side input port. Also, it is server side output port.                         |
| output_port     |  Client side output port. Also, it is server side input port.                         |
| n_envs          |  Number of environments. We use n_envs > 1 if there is several agents in server side. |
| action_shape    |  Action shape is a number of actions.                                                 |
| state_shape     |  Shape of the environment's state.                                                    |
| min_value       |  Minimum value in the state content.                                                  |
| max_value       |  Maximum value in the state content.                                                  |
| state_type      |  Type of the state content.                                                           |
| actions         |  List of pairs (action_name, actions_parameters).                                     |
| actions_meaning |  Action meaning for each action in actions.                                           |
| state_wrapper   |  Method or callable object that transform dictionary returned by RemoteEnv in gym compatible data.                                                                                          |
| make_inference_network | Method used only by A3C algorithm.                                             |
|-----------------|---------------------------------------------------------------------------------------|


 When you performe `reset`, `env` sends a especial command named `restart` to your unity application and returns the first environment's state:

    initial_state = env.reset()


 

