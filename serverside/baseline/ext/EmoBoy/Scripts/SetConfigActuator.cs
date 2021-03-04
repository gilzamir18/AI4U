using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ai4u.ext;

public class SetConfigActuator : Actuator
{


    public ConfigureTarget config;

    // Start is called before the first frame update
    void Start()
    {

    }


    public override void Act()
    {
        if (agent.GetActionName() == actionName) {
            float[] values = agent.GetActionArgAsFloatArray();
            config.radio = values[0];
            config.inBuildingProb = values[1];
            config.inBuildingLevel = (int) values[2];
            config.minDistanceRate= values[3];
            config.maxHeight = (int) values[4];
            config.OnReset(null);
        }
    }
    

    public override void Reset()
    {

    }
}
