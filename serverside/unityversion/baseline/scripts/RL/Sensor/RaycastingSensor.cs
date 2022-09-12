using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;

namespace ai4u
{
    [Serializable]
    public struct ObjectMapping 
    {
        public string tag;
        public int code;

        public ObjectMapping(string tag, int code=0)
        {
            this.tag = tag;
            this.code = code;
        }
    }

    public class RaycastingSensor : Sensor
    {
        public  ObjectMapping[] objectMapping;
        public int noObjectCode;
        public GameObject eye;
        public float fieldOfView = 90.0f;
        public float visionMaxDistance = 500f;
        public bool returnDepthMatrix = false;
        public int vSize = 10;
        public int hSize = 10;

        private Dictionary<string, int> mapping;
        private Ray[,] raysMatrix = null;
        private Vector3 fw1 = new Vector3(), fw2 = new Vector3();
        private HistoryStack<float> stack;

        public override void OnSetup(Agent agent)
        {
            type = SensorType.sfloatarray;
            shape = new int[2]{hSize,  vSize};
            stack = new HistoryStack<float>(stackedObservations * shape[0] * shape[1]);
            agent.AddResetListener(this);
            mapping = new Dictionary<string, int>();
            foreach(ObjectMapping obj in objectMapping)
            {
                mapping[obj.tag] = obj.code;
            }
            raysMatrix = new Ray[shape[0], shape[1]];
        }
        
        public override void OnReset(Agent agent) 
        {
            stack = new HistoryStack<float>(stackedObservations * shape[0] * shape[1]);
            mapping = new Dictionary<string, int>();
            foreach(ObjectMapping obj in objectMapping)
            {
                mapping[obj.tag] = obj.code;
            }
            raysMatrix = new Ray[shape[0], shape[1]];
            GetFloatArrayValue();
        }

        public override float[] GetFloatArrayValue()
        {
            UpdateRaysMatrix(eye.transform.position, eye.transform.forward, eye.transform.up, eye.transform.right);
            return stack.Values;
        }

        private void UpdateRaysMatrix(Vector3 position, Vector3 forward, Vector3 up, Vector3 right)
        {
            float vangle = 2 * fieldOfView / shape[0];
            float hangle = 2 * fieldOfView / shape[1];

            float ivangle = -fieldOfView;

            for (int i = 0; i < shape[0]; i++)
            {
                float ihangle = -fieldOfView;
                fw1 = (Quaternion.AngleAxis(ivangle + vangle * i, right) * forward).normalized;
                fw2.Set(fw1.x, fw1.y, fw1.z);

                for (int j = 0; j < shape[1]; j++)
                {
                    raysMatrix[i, j].origin = position;
                    raysMatrix[i, j].direction = (Quaternion.AngleAxis(ihangle + hangle * j, up) * fw2).normalized;
                    UpdateViewMatrix(i, j);
                }
            }
        }

        public bool debugMode = false;
        public void UpdateViewMatrix(int i, int j)
        {                
            RaycastHit hitinfo;
            if (Physics.Raycast(raysMatrix[i, j], out hitinfo, visionMaxDistance))
            {
                if (debugMode)
                {
                    Debug.DrawRay(raysMatrix[i,j].origin, raysMatrix[i,j].direction * visionMaxDistance, Color.yellow);
                }

                GameObject gobj = hitinfo.collider.gameObject;

                string objtag = gobj.tag;
                if (mapping.ContainsKey(objtag)){
                    int code = mapping[objtag];
                    if (!returnDepthMatrix)
                    {
                        stack.Push(code);
                    }
                    else
                    {
                        stack.Push(hitinfo.distance);
                    }
                } 
                else 
                {
                    if (!returnDepthMatrix)
                    {
                        stack.Push(noObjectCode);
                    }
                    else
                    {
                        stack.Push(-1);
                    }
                }
            }
            else
            {
                if (!returnDepthMatrix)
                {
                    stack.Push(noObjectCode);
                }
                else
                {
                    stack.Push(-1);
                }
            }
        }
    }
}
