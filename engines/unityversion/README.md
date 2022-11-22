
# AI4U for Unity

This is the Unity version of the AI4U tool. The Unity version of the AI4U get advantage of the Unity architecture and facilitate agent specification by means of an agent abstraction. In Unity, AI4U provides a alternative approach to modeling Non-Player Characters (NPC). 

Agent abstraction defines an agent living in a environment and interacting with this environment by means of sensors and actuators. So, NPC specification is a kind of agent specification. Agent's components are: sensors, actuators, events, reward functions and brain. Sensors and actuators are the interface between agents and environments. A sensor provides data to an agent's brain, while actuators send actions from agent to environment. A brain is a script that proccessing sensors' data e made a decision (selects an action by time).

We map components of the Unity architecture to agents components. So, agents components are stored as prefabs. A prefab is made component of game objects. A Game Object is a visible or not visible element of the game. Furthermore, the visible elements are statics or dynamics. Shortly, we will publish a full article on this relationship between Unity and agent abstraction. 
