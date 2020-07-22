using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ai4u;

namespace  ai4u.ext
{
    public class WASDPRLController : Controller
    {
        public float speed = 10.0f;
        override public object[] GetAction()
        {

            float actionValue=0;
            string actionName="";

            if (Input.GetKey(KeyCode.W))
            {
                actionName = "z";
                actionValue = speed;
            }

            if (Input.GetKey(KeyCode.S))
            {
                actionName = "z";
                actionValue = -speed;
            }

            if (Input.GetKey(KeyCode.U))
            {
                actionName = "y";
                actionValue = speed;
            }

            if (Input.GetKey(KeyCode.J))
            {
                actionName = "y";
                actionValue = -speed;
            }

            if (Input.GetKey(KeyCode.A))
            {
                actionName = "x";
                actionValue = -speed;
            }

            if (Input.GetKey(KeyCode.D))
            {
                actionName = "x";
                actionValue = speed;
            }

            if (Input.GetKey(KeyCode.R))
            {
                actionName = "restart";
                actionValue = -1;
            }

            if (actionName != "restart")
            {
                return GetFloatAction(actionName, actionValue);
            } else
            {
                return GetFloatAction("restart", actionValue);
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