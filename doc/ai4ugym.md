# AI4U with Gym

We can use AI4U as a gym environment. First, you import AI4UEnv and gym modules

    import AI4UEnv, gym

After this, you can make a environment instance:

    env = gym.make('AI4UEnv-v0')
 
The object `env`  communicates with unity application thorugh a controller object. The default controller is an object of the class ai4u.controllers.BasicGymController. This is just one example for demonstration and only works with the 'SampleScene' scene of the 'AI4UTesting' project available in the 'examples' directory. You will probably have to create your own controller for a different environment.

When you perform `reset`, the environment implementation (`env`) sends an especial command named `restart` to your Unity application and returns the first environment's state:

    initial_state = env.reset()

The reset command should restart the environment for a new simulated episode. Finally, to perform an action, if your environment has discrete actions, you'll probably perform something like this:

```
	next_state = env.step(ID) #ID is an action
```

where ID is an integer identifier of the sent action. If your environment has continuous actions, say three, you'll probably have to perform something like this:

```
	next_state = env.step([a1, a2, a3])
```

where *a1*, *a2*, and *a3* are real numbers.

What will define the format of the action that the environment receives is the driver used. The default controller (of class ai4u.controllers.BasicGymController) accepts a Python list of real numbers frame indicating directional forces applied to an object (the NPC) in the scene.




# Final Considerations

The complete example used in this guide can be found at *examples/clientnewapi/scene_samplescene/appgym.py*.

