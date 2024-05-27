using Godot;
using System;


namespace ai4u;

public partial class MoveActuator : Actuator
{

    public virtual bool OnGround
    {
        get
        {
            return true;
        }
    }
}
