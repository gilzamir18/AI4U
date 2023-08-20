using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ai4u {
    public class MoveActuatorRC : Actuator
    {
        //forces applied on the x, y and z axes.    
        private float move, turn, jump, jumpForward, brake;
        public float moveAmount = 1;
        public float turnAmount = 1;
        public float jumpPower = 1;
        public float jumpForwardPower = 1;
        public float brakePower = 10;
        public float groundCheckDistance = 1f;
        public LayerMask groundMask;

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
            if (agent != null && !agent.Done)
            {
                float[] action = agent.GetActionArgAsFloatArray();
                move = action[0];
                turn = action[1];
                //Debug.Log("move : " + move);
                //Debug.Log("turn : " + turn);
                if (action.Length >= 3)
                {
                    brake = action[2];
                    //Debug.Log("Break: " + brake);
                    if (action.Length >= 4)
                    {
                        jump = action[3];
                        if (action.Length >= 5)
                        {
                            jumpForward = action[4];
                        }
                    }
                }

                Rigidbody rBody = agent.GetComponent<Rigidbody>();
                Transform reference = agent.gameObject.transform;

                if (rBody != null)
                {
                    onGround = Physics.Raycast(reference.position, Vector3.down, groundCheckDistance, groundMask);
                
                    if (onGround)
                    {
                        if (Mathf.Abs(turn) < 0.01f)
                        {
                            turn = 0;
                        }

                        Vector3 brkForce = Vector3.zero;
                        if (rBody.velocity.magnitude > 0)
                        {
                            brkForce = -rBody.velocity.normalized * brake * brakePower;
                            if (brkForce.magnitude > rBody.velocity.magnitude)
                            {
                                brkForce = -rBody.velocity;
                            }
                            rBody.AddForce(brkForce, ForceMode.Acceleration);
                        }

                        
                        rBody.angularVelocity = Vector3.zero;
                        rBody.AddTorque(reference.up * turn * turnAmount);

                        rBody.AddForce(
                                        (jump * jumpPower * reference.up +
                                        move * moveAmount * reference.forward + 
                                        (
                                            jumpPower * jumpForward * reference.up + 
                                            jumpForward * jumpForwardPower * 
                                            reference.forward )   
                                        )

                                      );
                    }
                }
                move = 0;
                turn = 0;
                jump = 0;
                jumpForward = 0;
                brake = 0;
            }
        }

        public override void OnSetup(Agent agent)
        {
            shape = new int[1]{4};
            isContinuous = true;
            rangeMin = new float[]{0, -1, 0, 0};
			rangeMax = new float[]{1, 1, 1, 1};
            agent.AddResetListener(this);
        }

        public override void OnReset(Agent agent)
        {
            turn = 0;
            move = 0;
            jump = 0;
            jumpForward = 0;
            onGround = false;
        }
    }
}

