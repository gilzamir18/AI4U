# Event Handler

An `ai4u.BasicAgent` object provides a set of events that allow you to intercept relevant events occurring during the agent's lifecycle. By intercepting an event, you can, for example, change a time indicator on the graphical user interface or even make convenient changes to the environment.

If the *PhysicsMode* option of the *ControlRequestor* node is enabled, every event occurs in the agent's physics update loop (corresponding to executing code in the *_PhysicsProcess* method). Otherwise, the update occurs in the rendering loop (equivalent to executing code in the *_Process* method). Any code or behavior you want to insert into the agent or environment that interferes with the simulation of the agent and environment should be inserted through an event controller. For instance, suppose you want to check if a collision has occurred with the agent, and you have created the method *HandleCollision(BasicAgent)*. Then, you would execute the following code to indicate that this method should be executed at the end of a time step:

```CSharp
agent.OnStepEnd += HandleCollision; 
```

where *agent* is the `BasicAgent` object. In this case, avoid performing the collision check directly in the *_PhysicsProcess* method.

The events supported by a `BasicAgent` object are:

* OnResetStart: executed at the beginning of an episode, before any OnReset method is executed.
* OnEpisodeEnd: executed at the end of an episode.
* OnEpisodeStart: executed at the beginning of an episode.
* OnStepEnd: executed at the end of a time step.
* OnStepStart: executed at the beginning of a time step.
* OnStateUpdateStart: executed at the beginning of a state update.
* OnStateUpdateEnd: executed at the end of a state update.
* OnActionStart: executed before all actions of the current time step are executed.
* OnActionEnd: executed after all actions of the current time step are executed.
* OnAgentStart: executed during the initialization of the agent.

All controllers of these events are of type AgentEpisodeHandler:

```CSharp
/// <summary>
/// Delegate for handling agent episode events.
/// </summary>
/// <param name="agent">The agent that triggered the event.</param>
public delegate void AgentEpisodeHandler(BasicAgent agent);
```
