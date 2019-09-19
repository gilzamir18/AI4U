using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using unityremote;

public class HumanController : Controller
{
    public override object[] GetAction()
    {
        Debug.Log("ACTION");
        float d = Input.GetAxis("Vertical");
        if (d > 0) {
            return GetFloatAction("Up", d);
        } else
        {
            return GetFloatAction("Down", d);
        }
    }

    public override void NewStateEvent()
    {

    }
}
