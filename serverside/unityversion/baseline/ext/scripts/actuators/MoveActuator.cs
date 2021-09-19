using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ai4u.ext {

	[RequireComponent(typeof(Rigidbody))]
    public class MoveActuator : Actuator
    {

        //forces applied on the x, y and z axes.
        private float fx, fy, fz;
        public float speed = 1;

        public override void Act()
        {
            Rigidbody rBody = agent.GetComponent<Rigidbody>();
            if (agent.GetActionName()==actionName)
            {
                float[] f = agent.GetActionArgAsFloatArray();
                fx = f[0]; fy = f[1]; fz = f[2];
            }
            if (rBody != null)
            {
                rBody.AddForce(fx * speed, fy * speed, fz * speed);
            }
        }

        public override void Reset()
        {
            fx = 0;
            fz = 0;
            fy = 0;
        }
    }
}