using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u
{
    public class RigidbodyRespawnActuator : Actuator
    {
    
        public Transform respawnMarker;
        public Vector3 startPoint = Vector3.zero;
        public bool ignoreStartPoint = true;
        
        public override void OnSetup(Agent agent)
        {
            this.agent = (BasicAgent) agent;
            this.agent.beginOfEpisodeEvent += HandleBeginOfEpisode;
            this.agent.endOfEpisodeEvent += HandleEndOfEpisode;
        }

        private void HandleBeginOfEpisode(BasicAgent agent)
        {
            Configure(agent);
        }

        private void HandleEndOfEpisode(BasicAgent agent)
        {
            var rBody = agent.GetComponent<Rigidbody>();
            rBody.velocity = Vector3.zero;
            rBody.angularVelocity = Vector3.zero;
        }

        public void Configure(BasicAgent agent)
        {
            var transform = agent.gameObject.transform;
            var rBody = agent.GetComponent<Rigidbody>();
            
            transform.rotation = Quaternion.identity;

            if (rBody != null) {
                rBody.velocity = Vector3.zero;
                rBody.angularVelocity = Vector3.zero;
            }

            if (respawnMarker != null)
            {
                if (respawnMarker.childCount > 0)
                {
                    int idx = (int)Random.Range(0, respawnMarker.childCount - 1 + 0.5f );
                    Transform c = respawnMarker.GetChild(idx);
                    transform.localPosition = c.localPosition;
                    transform.localRotation = c.localRotation;                    
                }
                else
                {
                    transform.localPosition =  respawnMarker.localPosition;
                    transform.localRotation = respawnMarker.localRotation;
                }
            } else {
                if (!ignoreStartPoint)
                {
                    transform.position = startPoint;
                }
            }
        }
    }
}
