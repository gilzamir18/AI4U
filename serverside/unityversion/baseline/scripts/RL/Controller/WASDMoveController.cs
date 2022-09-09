using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ai4u;

namespace  ai4u
{
    public class WASDMoveController : Controller
    {
        public float speed = 10.0f;
        override public string GetAction()
        {
            float[] actionValue = new float[3];
            string actionName="move";

            if (Input.GetKey(KeyCode.W))
            {
                actionValue[0] = speed;
            }

            if (Input.GetKey(KeyCode.S))
            {
                actionValue[0] = -speed;
            }

            if (Input.GetKey(KeyCode.U))
            {
                actionValue[2] = speed;
            }

            if (Input.GetKey(KeyCode.J))
            {
                actionValue[2] = -speed;
            }

            if (Input.GetKey(KeyCode.A))
            {
                actionValue[1] = -speed;
            }

            if (Input.GetKey(KeyCode.D))
            {
                actionValue[1] = speed;
            }

            if (Input.GetKey(KeyCode.R))
            {
                actionName = "__restart__";
            }

            if (actionName != "restart")
            {
                //Debug.Log(ai4u.Utils.ParseAction(actionName, actionValue));
                return ai4u.Utils.ParseAction(actionName, actionValue);
            } else
            {
                return ai4u.Utils.ParseAction("__restart__");
            }
        }

        override public void NewStateEvent()
        {
            int n = GetStateSize();
            for (int i = 0; i < n; i++)
            {
                if (GetStateName(i) == "reward" || GetStateName(i) == "score")
                {
                    float r = GetStateAsFloat(i);
                    if (r != 0) {
                        Debug.Log("Reward/Score = " + r);
                    }
                }
            }
        }
    }
}
