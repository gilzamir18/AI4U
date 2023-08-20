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
        public bool automaticTagMapping = true;
        public int tagCodeDistance = 10;

        public bool debugMode = false;

        public bool flattened = false;

        private Dictionary<string, int> mapping;
        private Ray[,] raysMatrix = null;
        private Vector3 fw1 = new Vector3(), fw2 = new Vector3();
        private HistoryStack<float> stack;
    
        public override void OnSetup(Agent agent)
        {
            type = SensorType.sfloatarray;
            if (!flattened)
			{

				shape = new int[2]{hSize,  vSize};
			}
			else
			{
				shape = new int[1]{hSize * vSize};
            }
            CreateBuffer();
            agent.AddResetListener(this);
            
            mapping = new Dictionary<string, int>();
            

            if (automaticTagMapping)
            {
                #if UNITY_EDITOR
                for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)
                {
                        string tag = UnityEditorInternal.InternalEditorUtility.tags[i];
                        mapping[tag] = i * tagCodeDistance;
                }
                #endif
            }
            else
            {
                foreach(ObjectMapping obj in objectMapping)
                {
                    mapping[obj.tag] = obj.code;
                }
            }
            raysMatrix = new Ray[hSize, vSize];
        }
        
        public override void OnReset(Agent agent) 
        {
            if (!flattened)
            {
                stack = new HistoryStack<float>(stackedObservations * shape[0] * shape[1]);
            }
            else
            {
                stack = new HistoryStack<float>(stackedObservations * shape[0]);
            }
            mapping = new Dictionary<string, int>();
            foreach(ObjectMapping obj in objectMapping)
            {
                mapping[obj.tag] = obj.code;
            }
            raysMatrix = new Ray[hSize, vSize];
            GetFloatArrayValue();
        }

        public override float[] GetFloatArrayValue()
        {
            UpdateRaysMatrix(eye.transform.position, eye.transform.forward, eye.transform.up, eye.transform.right);
            return stack.Values;
        }
        
        void OnDrawGizmos()
        {
            if (shape == null || stack == null)
            {
                type = SensorType.sfloatarray;
                shape = new int[2]{hSize,  vSize};
                stack = new HistoryStack<float>(stackedObservations * hSize * vSize);
                mapping = new Dictionary<string, int>();
                foreach(ObjectMapping obj in objectMapping)
                {
                    mapping[obj.tag] = obj.code;
                }
                raysMatrix = new Ray[shape[0], shape[1]];
            }
            UpdateRaysMatrix(eye.transform.position, eye.transform.forward, eye.transform.up, eye.transform.right);
        }

        private void CreateBuffer()
        {

            if (flattened)
            {
                stack = new HistoryStack<float>(stackedObservations * shape[0]);
            }
            else
            {
                stack = new HistoryStack<float>(stackedObservations * shape[0] * shape[1]);
            }
        }

        private void UpdateRaysMatrix(Vector3 position, Vector3 forward, Vector3 up, Vector3 right)
        {
            int s0 = 1;
            int s1 = 1;
            if (hSize > 1)
            {
                s0 = hSize - 1;
            }
            if (vSize > 1)
            {
                s1 = vSize - 1;
            }

            float vangle = fieldOfView / s1;
            float hangle = fieldOfView / s0;

            if (hSize > 1 && vSize == 1)
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
            if (hSize == 1 && vSize > 1)
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
            } if (hSize > 1 && vSize > 1)
            {
                float ivangle = -fieldOfView/2;

                for (int i = 0; i < vSize; i++)
                {
                    float ihangle = -fieldOfView/2;
                    fw1 = (Quaternion.AngleAxis(ivangle + vangle * i, right) * forward).normalized;
                    fw2.Set(fw1.x, fw1.y, fw1.z);

                    for (int j = 0; j < hSize; j++)
                    {
                        raysMatrix[j, i].origin = position;
                        raysMatrix[j, i].direction = Quaternion.AngleAxis(-verticalShift, right) * Quaternion.AngleAxis(-horizontalShift, up) * (Quaternion.AngleAxis(ihangle + hangle * j, up) * fw2).normalized;
                        UpdateViewMatrix(j, i);
                    }
                }
            }
        }

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
                    if (returnDepthMatrix)
                    {
                        stack.Push(hitinfo.distance);
                    }
                    else
                    {
                        stack.Push(code);
                    }
                } 
                else 
                {
                    if (returnDepthMatrix)
                    {
                        stack.Push(-1);   
                    }
                    else
                    {
                        stack.Push(noObjectCode);
                    }
                }
            }
            else
            {
                if (debugMode)
                {
                    Debug.DrawRay(raysMatrix[i,j].origin, raysMatrix[i,j].direction * visionMaxDistance, Color.yellow);
                }
                if (returnDepthMatrix)
                {
                    stack.Push(-1);   
                }
                else
                {
                    stack.Push(noObjectCode);
                }
            }
        }
    }
}
