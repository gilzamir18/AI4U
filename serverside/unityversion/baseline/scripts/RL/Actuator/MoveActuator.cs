using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ai4u {

	[RequireComponent(typeof(Rigidbody))]
    public class MoveActuator : Actuator
    {
        //forces applied on the x, y and z axes.    
        private float move, turn, jump, jumpForward;
        public float speed = 1;
        public float turnAmount = 1;
        public float jumpPower = 1;
        public float jumpForwardPower = 1;

        private bool onGround = false;

        public bool OnGround
        {
            get
            {
                return onGround;
            }
        }

        public override void Act()
        {
            if (!agent.Done)
            {
                float[] action = agent.GetActionArgAsFloatArray();

                move = action[0];
                turn = action[1];
                jump = action[2];
                jumpForward = action[3];

                Rigidbody rBody = agent.GetComponent<Rigidbody>();
                Transform reference = agent.gameObject.transform;
                if (rBody != null)
                {
                    if (Mathf.Abs(rBody.velocity.y) > 0.01)
                    {
                        onGround = false;
                    }
                    else
                    {
                        onGround = true;
                    }

                    Quaternion deltaRotation = Quaternion.Euler(reference.up * turn * turnAmount);
                    rBody.MoveRotation(rBody.rotation * deltaRotation);
                    if (onGround)
                    {
                        rBody.AddForce(jump * jumpPower * reference.up + move * speed * reference.forward + (jumpPower * jumpForward * reference.up + jumpForward * jumpForwardPower * reference.forward)   );
                    }
                }
                move = 0;
                turn = 0;
                jump = 0;
                jumpForward = 0;
            }
        }

        public override void Reset()
        {
            turn = 0;
            move = 0;
            jump = 0;
            jumpForward = 0;
            onGround = false;
        }
    }
}