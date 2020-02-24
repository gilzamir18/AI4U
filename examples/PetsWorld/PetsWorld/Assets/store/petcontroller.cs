using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using unityremote;

public class petcontroller : Controller
{
        override public object[] GetAction()
        {
            float  v = Input.GetAxis("Vertical");
            float  h = Input.GetAxis("Horizontal");
            float jump = Input.GetKey( KeyCode.Space  ) ? 1.0f : -1.0f;
            float push_block = Input.GetKey( KeyCode.Home ) ? 1.0f: -1.0f;
            float signal = Input.GetKey( KeyCode.End ) ? 1.0f : -1.0f;

            return GetFloatArrayAction("act", new float[]{v, h, jump, push_block, signal});
        }

        override public void NewStateEvent()
        {
        }
}
