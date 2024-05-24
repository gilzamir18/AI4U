using Godot;
using System;


namespace ai4u;

public abstract partial class MoveActuator : Actuator
{

    public abstract bool OnGround
    {
        get;
    }

}
