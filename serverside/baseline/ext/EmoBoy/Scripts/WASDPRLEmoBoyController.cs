using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ai4u;

namespace  ai4u.ext
{
    public class WASDPRLEmoBoyController : Controller
    {
        public float speedY = 10.0f;
        public float speedX = 5.0f;
        public float emotion = 0.0f;
        
        float[] actionValue = new float[10];
        
        private void Reset() {
            actionValue = new float[10];
        }

        override public object[] GetAction()
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

            if (Input.GetKey(KeyCode.Alpha0)){
                actionName = "Emotion";
                emotion = 0;
            }

            if (Input.GetKey(KeyCode.Alpha1)){
                actionName = "Emotion";
                emotion = 1;
            }

            if (Input.GetKey(KeyCode.Alpha2)){
                actionName = "Emotion";
                emotion = 2;
            }

            if (Input.GetKey(KeyCode.Alpha3)){
                actionName = "Emotion";
                emotion = 3;
            }

            if (Input.GetKey(KeyCode.Alpha4)){
                actionName = "Emotion";
                emotion = 4;
            }

            if (Input.GetKey(KeyCode.Alpha5)){
                actionName = "Emotion";
                emotion = 5;
            }

            if (Input.GetKey(KeyCode.Alpha6)){
                actionName = "Emotion";
                emotion = 6;
            }

            if (Input.GetKey(KeyCode.Alpha7)){
                actionName = "Emotion";
                emotion = 7;
            }

            if (Input.GetKey(KeyCode.Alpha8)){
                actionName = "Emotion";
                emotion = 8;
            }

            if (Input.GetKey(KeyCode.Alpha9)){
                actionName = "Emotion";
                emotion = 9;
            }

            if (Input.GetKey(KeyCode.R))
            {
                actionName = "restart";
                Reset();
            }

            if (actionName == "Character" )
            {
                return GetFloatArrayAction(actionName, actionValue);
            } else if (actionName == "Emotion") {
                return GetFloatArrayAction("Emotion", new float[]{emotion});
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