using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using unityremote;

public class PlayerController : Controller
{
        public override object[] GetAction()
        {
            if (Input.GetKeyUp(KeyCode.A)) {
                return GetIntAction("move", (int)ACTION.left);
            }

            if (Input.GetKeyUp(KeyCode.D)) {
                return GetIntAction("move", (int)ACTION.right);
            }
            
            if (Input.GetKeyUp(KeyCode.W)) {
                return GetIntAction("move", (int)ACTION.up);
            }
            
            if (Input.GetKeyUp(KeyCode.S)) {
                return GetIntAction("move", (int)ACTION.down);
            }
            
            return GetStringAction("get_status", "get_status");
        }

}
