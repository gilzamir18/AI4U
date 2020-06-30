using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using unityremote;

public class petcontroller : Controller
{
    private int signal = 0;

    override public object[] GetAction()
    {

        int action = 10;

        if (Input.GetKey(KeyCode.Space))
        {
            signal = 1 - signal;
        }

        if (Input.GetKey(KeyCode.W))
        {
            action = 0;
        }

        if (Input.GetKey(KeyCode.S))
        {
            action = 3;
        }


        if (Input.GetKey(KeyCode.A))
        {
            action = 1;
        }

        if (Input.GetKey(KeyCode.D))
        {
            action = 4;
        }

        if (Input.GetKey(KeyCode.R))
        {
            action = -1;
        }

        if (signal > 0)
        {
            action = 11;
        }

        if (action >= 0)
        {
            return GetIntAction("act", action);
        } else
        {
            return GetIntAction("restart", action);
        }
    }

    override public void NewStateEvent()
    {
    }
}
