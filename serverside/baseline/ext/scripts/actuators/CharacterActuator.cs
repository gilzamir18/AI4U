using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

namespace ai4u.ext {
    public class CharacterActuator : Actuator
    {

        //BEGIN::Game controller variables
        private ThirdPersonCharacter character;
        private Transform m_CamTransform;
        private Vector3 m_CamForward;             // The current forward direction of the camera
        private Vector3 m_Move;
        //END::
        

        
        //BEGIN::motor controll variables
        private float fx, fy;
        private bool crouch;
        private bool jump;
        private float leftTurn = 0;
        private float rightTurn = 0;
        private float up = 0;
        private float down = 0;
        private bool pushing;
        private bool getpickup;
        private bool usewalkspeed = false;
        private float walkspeed = 0.5f;
        public int nMoveUpdates = 4;
        //END::motor controll variables

        public Camera mainCamera;

        void Awake()
        {
            // get the third person character ( this should never be null due to require component )
            character = agent.GetComponent<ThirdPersonCharacter>();

            if (mainCamera != null)
            {
                m_CamTransform = mainCamera.transform;
            }
            else
            {
                Debug.LogWarning(
                    "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.", gameObject);
                // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
            }
        }


        void FixedUpdate() {

        }


        private void UpdateActuator(){
            if (agent.Done) {
                return;
            }

            // read inputs
            float h = fx;
            float v = fy;

            // calculate move direction to pass to character
            if (m_CamTransform != null)
            {

                // calculate camera relative direction to move:
                m_CamForward = Vector3.Scale(m_CamTransform.forward, new Vector3(1, 0, 1)).normalized;
                m_Move = v * m_CamForward + h * m_CamTransform.right;

            }
            else
            {
                // we use world-relative directions in the case of no main camera
                m_Move = v * Vector3.forward + h * Vector3.right;
            }

            // walk speed multiplier
            if (usewalkspeed) {
                m_Move *= walkspeed;
            } 

            // pass all parameters to the character control script
            for (int m = 0; m < nMoveUpdates; m++) {
                character.Move(m_Move, crouch, jump, rightTurn - leftTurn, down - up, pushing, fx, fy, getpickup);
            }
            //character.Move(m_Move, crouch, m_Jump, h, v, pushing);
            jump = false;
        }

        public override void Act()
        {
            Reset();
            if (agent.GetActionName()==actionName)
            {
                //walk and run = 0
                float[] args = agent.GetActionArgAsFloatArray(); 
                fy = args[0]; 
                //walk_around = 1
                fx = args[1];
                
                //right_turn = 2
                rightTurn = args[2];

                //leftTurn = 3    
                leftTurn = args[3];
                    
                //up = 4
                up = args[4];
                
                //down = 5
                down = args[5];

                //push = 6
                pushing = args[6] > 0;
                
                //jump = 7
                jump = args[7] > 0;
                    
                //crouch = 8
                crouch = args[8] > 0;

                //pickup = 9
                getpickup = args[9] > 0;
            }
            UpdateActuator();
        }
        

        public override void Reset()
        {
            fx = 0;
            fy = 0;
            crouch = false;
            jump = false;
            pushing = false;
            leftTurn = 0;
            rightTurn = 0;
            up = 0;
            down = 0;
        }
    }
}