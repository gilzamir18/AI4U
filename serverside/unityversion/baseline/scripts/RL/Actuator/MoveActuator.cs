using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ai4u {

	[RequireComponent(typeof(Rigidbody))]
    public class MoveActuator : Actuator
    {

        //forces applied on the x, y and z axes.    
        private float move, turn, jump;
        public float speed = 1;
        public float turnAmount = 1;
        public float jumpPower = 1;
        public float jumpForward = 0;
        
        private bool onGround = false;

        public override void Act()
        {
            if (!agent.Done)
            {
                float[] action = agent.GetActionArgAsFloatArray();

                move = action[0];
                turn = action[1];
                jump = action[2];

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

                    Quaternion deltaRotation = Quaternion.Euler(reference.up * turn * turnAmount * Time.fixedDeltaTime);
                    rBody.MoveRotation(rBody.rotation * deltaRotation);
                    if (onGround && move != 0)
                    {
                        rBody.AddForce(jump * jumpPower * reference.up + move * speed * reference.forward + jump * jumpForward * reference.forward);
                    }
                    else if (move == 0 && jump == 0)
                    {
                        rBody.velocity = Vector3.zero;
                    } else if (move == 0 && jump != 0)
                    {
                        float vy = rBody.velocity.y;
                        rBody.velocity = new Vector3(0, vy, 0);
                    }
                }
                move = 0;
                turn = 0;
                jump = 0;
            }
        }

        public override void Reset()
        {
            turn = 0;
            move = 0;
            jump = 0;
            onGround = false;
        }
    }
}