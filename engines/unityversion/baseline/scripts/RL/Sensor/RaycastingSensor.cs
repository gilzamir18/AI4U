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
        public float verticalShift = 0;
        public float horizontalShift = 0;
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
        
        void OnDrawGizmos()
        {
            type = SensorType.sfloatarray;
            shape = new int[2]{hSize,  vSize};
            stack = new HistoryStack<float>(stackedObservations * shape[0] * shape[1]);
            mapping = new Dictionary<string, int>();
            foreach(ObjectMapping obj in objectMapping)
            {
                mapping[obj.tag] = obj.code;
            }
            raysMatrix = new Ray[shape[0], shape[1]];
            UpdateRaysMatrix(eye.transform.position, eye.transform.forward, eye.transform.up, eye.transform.right);
        }

        private void UpdateRaysMatrix(Vector3 position, Vector3 forward, Vector3 up, Vector3 right)
        {
            int s0 = 1;
            int s1 = 1;
            if (shape[0] > 1)
            {
                s0 = shape[0] - 1;
            }
            if (shape[1] > 1)
            {
                s1 = shape[1] - 1;
            }

            float vangle = fieldOfView / s1;
            float hangle = fieldOfView / s0;

            if (shape[0] > 1 && shape[1] == 1)
            {
                float ihangle = -fieldOfView/2;
                fw2.Set(forward.x, forward.y, forward.z);
                for (int j = 0; j < shape[0]; j++)
                {
                    raysMatrix[j, 0].origin = position;
                    raysMatrix[j, 0].direction = (Quaternion.AngleAxis(-verticalShift, right) * Quaternion.AngleAxis(-horizontalShift, up) * Quaternion.AngleAxis(ihangle + hangle * j, up) * fw2).normalized;
                    UpdateViewMatrix(j, 0);
                }
            }
            if (shape[0] == 1 && shape[1] > 1)
            {
                float ivangle = -fieldOfView/2;
                for (int i = 0; i < shape[1]; i++)
                {
                    float ihangle = -fieldOfView;
                    fw1 = (Quaternion.AngleAxis(-verticalShift, right) * Quaternion.AngleAxis(-horizontalShift, up) * Quaternion.AngleAxis(ivangle + vangle * i, right) * forward).normalized;
                    fw2.Set(fw1.x, fw1.y, fw1.z);
                    raysMatrix[0, i].origin = position;
                    raysMatrix[0, i].direction = fw2.normalized;
                    UpdateViewMatrix(0, i);
                }
            } if (shape[0] > 1 && shape[1] > 1)
            {
                float ivangle = -fieldOfView/2;

                for (int i = 0; i < shape[1]; i++)
                {
                    float ihangle = -fieldOfView/2;
                    fw1 = (Quaternion.AngleAxis(ivangle + vangle * i, right) * forward).normalized;
                    fw2.Set(fw1.x, fw1.y, fw1.z);

                    for (int j = 0; j < shape[0]; j++)
                    {
                        raysMatrix[j, i].origin = position;
                        raysMatrix[j, i].direction = Quaternion.AngleAxis(-verticalShift, right) * Quaternion.AngleAxis(-horizontalShift, up) * (Quaternion.AngleAxis(ihangle + hangle * j, up) * fw2).normalized;
                        UpdateViewMatrix(j, i);
                    }
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

                    Debug.DrawRay(raysMatrix[i,j].origin, raysMatrix[i,j].direction * hitinfo.distance, Color.red);
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
                if (debugMode)
                {
                    Debug.DrawRay(raysMatrix[i,j].origin, raysMatrix[i,j].direction * visionMaxDistance, Color.yellow);
                }
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
