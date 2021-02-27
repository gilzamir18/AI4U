using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;

namespace ai4u.ext
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
        public float visionMaxDistance = 500f;

        private Dictionary<string, int> mapping;
        private Ray[,] raysMatrix = null;
        private int[,] viewMatrix = null;
        private float[,] depthMatrix = null;
        private Vector3 fw1 = new Vector3(), fw2 = new Vector3();


        void Start() 
        {
            agent.AddResetListener(this);
            OnReset(agent);
        }
        
        public override void OnReset(Agent agent) {
            mapping = new Dictionary<string, int>();
            foreach(ObjectMapping obj in objectMapping)
            {
                mapping[obj.tag] = obj.code;
            }
            raysMatrix = new Ray[shape[0], shape[1]];
            viewMatrix = new int[shape[0], shape[1]];
            depthMatrix = new float[shape[0], shape[1]];
        } 

        public override byte[] GetByteArrayValue()
        {
            string m = updateCurrentRayCastingFrame()[0];
            return Encoding.UTF8.GetBytes(m.ToCharArray());
        }

        public override string GetStringValue() {
            string[] m = updateCurrentRayCastingFrame();
            return m[0] + ":" + m[1];
        }

        public int[,] GetViewMatrix() {
            return this.viewMatrix;
        }

        private string[] updateCurrentRayCastingFrame()
        {
            UpdateRaysMatrix(eye.transform.position, eye.transform.forward, eye.transform.up, eye.transform.right);
            UpdateViewMatrix();
            StringBuilder sb = new StringBuilder();
            StringBuilder dp = new StringBuilder();
            for (int i = 0; i < shape[0]; i++)
            {
                for (int j = 0; j < shape[1]; j++)
                {
                    //Debug.DrawRay(raysMatrix[i, j].origin, raysMatrix[i, j].direction, Color.red);
                    sb.Append(viewMatrix[i, j]);
                    dp.Append( depthMatrix[i, j].ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat) );
                    if (j <= shape[1]-2) {
                        sb.Append(",");
                        dp.Append(",");
                    }

                    
                }
                if (i <= shape[0] - 2) {
                    sb.Append(";");
                    dp.Append(";");
                }
            }
            return new string[]{sb.ToString(), dp.ToString()};
            //return Encoding.UTF8.GetBytes(sb.ToString().ToCharArray());
        }

        private void UpdateRaysMatrix(Vector3 position, Vector3 forward, Vector3 up, Vector3 right, float fieldOfView = 90.0f)
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
                    //Debug.DrawRay(raysMatrix[i,j].origin, raysMatrix[i,j].direction * visionMaxDistance, Color.red);
                }
            }
        }

        public void UpdateViewMatrix(float maxDistance = 500.0f)
        {
            for (int i = 0; i < shape[0]; i++)
            {
                for (int j = 0; j < shape[1]; j++)
                {
                
                    RaycastHit hitinfo;
                    if (Physics.Raycast(raysMatrix[i, j], out hitinfo, visionMaxDistance))
                    {
                        //Debug.DrawRay(raysMatrix[i,j].origin, raysMatrix[i,j].direction * visionMaxDistance, Color.yellow);
    
                        GameObject gobj = hitinfo.collider.gameObject;

                        string objtag = gobj.tag;
                        if (mapping.ContainsKey(objtag)){
                            int code = mapping[objtag];
                            viewMatrix[i,j] = code;
                            depthMatrix[i, j] = hitinfo.distance;
                        } else {
                            viewMatrix[i, j] = 0;
                            depthMatrix[i, j] = -1.0f;
                        }
                    }
                    else
                    {
                        depthMatrix[i, j] = -1.0f;
                        viewMatrix[i, j] = 0;
                    }
                }
            }
        }
    }
}