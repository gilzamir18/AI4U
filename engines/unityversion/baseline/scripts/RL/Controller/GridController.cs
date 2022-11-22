using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u
{
    public class GridController : Controller
    {
        public string actuatorName = "grid";

        override public string GetAction()
        {
            string actionName = actuatorName;

            int actionValue = 5;
            

            if (Input.GetKey(KeyCode.W))
            {
                actionValue = 0;
            }

            if (Input.GetKey(KeyCode.S))
            {
                actionValue = 1;
            }

            if (Input.GetKey(KeyCode.L))
            {
                actionValue = 2;
            }

            if (Input.GetKey(KeyCode.A))
            {
                actionValue = 3;
            }

            if (Input.GetKey(KeyCode.D))
            {
                actionValue = 4;
            }

            if (Input.GetKey(KeyCode.R))
            {
                actionName = "__restart__";
            }

            if (actionName != "__restart__")
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