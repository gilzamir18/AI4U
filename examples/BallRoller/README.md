# Environment BallRoller

BallRoller is a simple UnityRemote demonstration end to end with reinforcement learning. The agent's goal is to push the red ball forward to the green box.  Agent can run any actions of these actions: turn left, turn right, forward, and backward. The agent receives a pain equals to -1 when the ball falls from the plane and gains a reward equals to +1 when the ball touches the box. 

The best score in this environment is one (1). Using the vanilla A3C algorithm with four workers, we reach the best score of about twelve minutes. Figure 1 shows training evolution.

| ![BallRoller Agent](/doc/images/ballrollertraining.png) |
| :--: |
| Figure 1. *Training evolution.* |