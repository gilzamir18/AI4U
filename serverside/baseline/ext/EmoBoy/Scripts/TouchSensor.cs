using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u.ext
{
    public class TouchSensor : Sensor
    {
     
        private float[] output = new float[3];
        void Start() {
            agent.AddResetListener(this);
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Terrain") {
                output[0] = 1.0f;
            }

            if (collision.gameObject.tag == "Wall") {
                output[1] = 1.0f;
            }

            if (collision.gameObject.tag == "Target") {
                output[2] = 1.0f;
            }
        }

        void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.tag == "Terrain") {
                output[0] = 0.0f;
            }

            if (collision.gameObject.tag == "Wall") {
                output[1] = 0.0f;
            }

            if (collision.gameObject.tag == "Target") {
                output[2] = 0.0f;
            }
        }

        void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.tag == "Terrain") {
                output[0] = 1.0f;
            }

            if (collider.gameObject.tag == "Wall") {
                output[1] = 1.0f;
            }

            if (collider.gameObject.tag == "Target") {
                output[2] = 1.0f;
            }
        }

        void OnTriggerExit(Collider collider)
        {
            if (collider.gameObject.tag == "Terrain") {
                output[0] = 0.0f;
            }

            if (collider.gameObject.tag == "Wall") {
                output[1] = 0.0f;
            }

            if (collider.gameObject.tag == "Target") {
                output[2] = 0.0f;
            }
        }

        public override float[] GetFloatArrayValue()
        {
            return output;
        }

        public override void OnReset(Agent agent) {
            output[0] = 0.0f;
            output[1] = 0.0f;
            output[2] = 0.0f;
        } 
    }
}