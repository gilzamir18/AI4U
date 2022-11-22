using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ai4u;

namespace  ai4u
{
    public class WASDCharacterController : Controller
    {
        public float speedY = 10.0f;
        public float speedX = 5.0f;
        float[] actionValue = new float[10];
        
        private void Reset() {
            actionValue = new float[10];
        }

        override public string GetAction()
        {
            string actionName="";
            Reset();
            if (Input.GetKey(KeyCode.W))
            {
                actionName = "Character";
                actionValue[0] =  speedY;
            }

            if (Input.GetKey(KeyCode.S))
            {
                actionName = "Character";
                actionValue[0] =  -speedY;
            }

            if (Input.GetKey(KeyCode.U))
            {
                actionName = "Character";
                actionValue[6] = 1.0f;
            } else {
                actionValue[6] = 0.0f;
            }

            if (Input.GetKey(KeyCode.J))
            {
                actionName = "Character";
                actionValue[8] = 1.0f;
            } else {
                actionValue[8] = 0.0f;
            }

            if (Input.GetKey(KeyCode.A))
            {
                actionName = "Character";
                actionValue[1] = -speedX;
            }

            if (Input.GetKey(KeyCode.Space)) {
                actionName = "Character";
                actionValue[7] = 1;

            } else {
                actionValue[7] = 0;
            }

            if (Input.GetKey(KeyCode.D))
            {
                actionName = "Character";
                actionValue[1] = speedX;
            }

            if (Input.GetKey(KeyCode.R))
            {
                actionName = "restart";
                Reset();
            }

            if (actionName != "restart")
            {
                return actionName + ";" + actionValue;
            } else
            {
                return "restart";
            }
        }

        override public void NewStateEvent()
        {
            float r = GetStateAsFloat(1);
            if (r != 0) {
                Debug.Log("Reward " + r);
            }
        }
    }
}