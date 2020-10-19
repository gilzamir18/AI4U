using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ai4u.ext;
using ai4u;

namespace ai4u.ext
{

    /// <summary>DPRLAgent - Dimentional Physical Reinforcement Learning Agent
    /// This class models an agent with physical rigidbody control in a tridimentional world. </summary> 
    public class DPRLAgent : RLAgent
    {

        /// <summary>Targets are game objects that the agent must reach.</summary> 
        public GameObject[] targets; 

        /// <summary> Ramdom positions contains all positions where the agent can be placed in the environment. 
        /// All positions are equally likely.</summary>
        public Vector3[] randomPositions;

        ///<summary> <code>doneAtNegativeReward</code> ends the simulation whenever the agent receives a negative reward.</summary>
        public bool doneAtNegativeReward = true;
        ///<summary> <code>doneAtPositiveReward</code> ends the simulation whenever the agent receives a positive reward.</summary>
        public bool doneAtPositiveReward = false;
        
        ///<summary>The maximum number of steps per episode.</summary>
        public int MaxStepsPerEpisode = 0;

        //Agent's ridid body
        private Rigidbody rBody;

        ///<summary>Agent's speed by simulation step.</summary>
        public float speed = 10;
        
        //This flag indicates the end of the episode.
        private bool done = false;

        //Current episode's number of steps.
        private int nSteps = 0;

        //forces applied on the x, y and z axes.
        private float fx, fy, fz;

        private Vector3 initialLocalPosition;

        void Awake() {
            this.numberOfFields = 3;
            rBody = GetComponent<Rigidbody>();
        }

        ///<summary>This property indicates the end of the episode.</summary>
        public bool Done {
            set {
                done = value;
            }

            get {
                return done;
            }
        }

        public override void AddReward(float v, RewardFunc from = null){
            if (doneAtNegativeReward && v < 0) {
                done = true;
            }

            if (doneAtPositiveReward && v > 0) {
                done = true;
            }
            reward += v;
        }

        public override void ApplyAction()
        {
            fx = 0.0f;
            fy = 0.0f;
            fz = 0.0f;
            Debug.Log(GetActionName());
            switch (GetActionName())
            {
                case "x":
                    if (Done) return;
                    fx = GetActionArgAsFloat();
                    break;
                case "z":
                    if (Done) return;
                    fz = GetActionArgAsFloat();
                    break;
                case "y":
                    if (Done) return;
                    fy = GetActionArgAsFloat();
                    break;
                case "xyz":
                    if (Done) return;
                    float[] f = GetActionArgAsFloatArray();
                    fx = f[0];
                    fy = f[1];
                    fz = f[2];
                    break;
                case "restart":
                    ResetPlayer();
                    break;
            }
        }

        public override void UpdatePhysics()
        {
            if (Done) {
                return;
            }
            if (rBody != null)
            {
                rBody.AddForce(fx * speed, fy * speed, fz * speed);
            }
        }

        private void ResetPlayer()
        {
            nSteps = 0;
            if (initialLocalPosition == null) {
                initialLocalPosition = transform.localPosition;
            }
            done = false;
            rBody.velocity = Vector3.zero;
            rBody.angularVelocity = Vector3.zero;
            if (randomPositions.Length > 0) {
                int idx = (int)Random.Range(0, randomPositions.Length-1 + 0.5f);
                transform.localPosition = randomPositions[idx];
            } else {
                transform.localPosition = initialLocalPosition;
            }

            fx = 0;
            fz = 0;
            fy = 0;
            ResetReward();
        }

        public override void UpdateState()
        {
            nSteps ++;

            if (MaxStepsPerEpisode > 0 && nSteps >= MaxStepsPerEpisode) {
                done = true;
            }

            SetStateAsBool(0, "done", done);
            SetStateAsFloat(1, "reward", Reward);
            int framesize = 6 + 6 * targets.Length;
            float[] frame = new float[framesize]; 
            frame[0] = rBody.velocity.x;
            frame[1] = rBody.velocity.y;
            frame[2] = rBody.velocity.z;
            frame[3] = transform.localPosition.x;
            frame[4] = transform.localPosition.y;
            frame[5] = transform.localPosition.z;
            for (int i = 0; i < targets.Length; i += 6) {
                Rigidbody rBody = targets[i].GetComponent<Rigidbody>();
                frame[i+6] = rBody.velocity.x;
                frame[i+7] = rBody.velocity.y;
                frame[i+8] = rBody.velocity.z;
                frame[i+9] = targets[i].transform.localPosition.x;
                frame[i+10] = targets[i].transform.localPosition.y;
                frame[i+11] = targets[i].transform.localPosition.z;
            }
            SetStateAsFloatArray(2, "state", frame);
            ResetReward();
            Debug.Log("State Updated");
        }
    }
}