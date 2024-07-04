# Don't Break the Game

The base object of any agent-environment interaction is defined by the `ai4u.RLAgent` class. Although this class implements the abstract class `ai4u.Agent`, if you want to create your own agent class, it is highly recommended that your new agent class inherits from `ai4u.RLAgent`. This is unless you really know what you are doing. We could be more radical and suggest that you never attempt to create your own agent class; all customization of the framework should be done in sensors, actuators, controllers, and reward functions. However, if you still want to take the risk, it is possible (though very difficult) to create your own agent class without breaking things.

We have developed several projects that demonstrate different ways to customize agents centered around the `RLAgent` class:

* [AI4U Basic Projects](https://github.com/gilzamir18/ai4u_demo_projects)
    * BoxChase
    * Platform2D
    * Jumper (here we have the Godot animation system integrated with the agent)

To support more customizations in AI4U, the `RLAgent` class provides a set of events that allow you to know exactly what is happening. See more details in our documentation on [events](events.md).