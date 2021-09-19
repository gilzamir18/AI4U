using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u.ext {

public class CharacterLocalController : Controller
{
        float[] actionValue = new float[4];
        Vector3 dir = Vector3.zero;
        
        private void Reset() {
            dir = Vector3.zero;
        }

        override public object[] GetAction()
        {
            string actionName="";
            Reset();
            if (Input.GetKey(KeyCode.W))
            {
                actionName = "Character";
                dir[1] = 1;
            }

            if (Input.GetKey(KeyCode.S))
            {
                actionName = "Character";
                dir[1] = 0;
            }

            if (Input.GetKey(KeyCode.Space))
            {
                actionName = "Character";
                dir[2] = 1;
            }

            if (Input.GetKey(KeyCode.A))
            {
                actionName = "Character";
                dir[0] = 1;
            }

            if (Input.GetKey(KeyCode.D))
            {
                actionName = "Character";
                Reset();
                dir[0] = -1;
            }

            if (Input.GetKey(KeyCode.R))
            {
                actionName = "restart";
                Reset();
            }

            actionValue[0] = dir.x;
            actionValue[1] = dir.y;
            actionValue[2] = dir.z;

            if (actionName != "restart")
            {
                return GetFloatArrayAction(actionName, actionValue);
            } else
            {
                return GetFloatArrayAction("restart", actionValue);
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