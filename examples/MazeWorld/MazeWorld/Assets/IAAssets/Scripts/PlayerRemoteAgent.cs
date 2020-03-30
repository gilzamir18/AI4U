using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityStandardAssets.Characters.ThirdPerson;
using UnityEngine.SceneManagement;
using unityremote;

namespace unityremote
{

    public class PlayerRemoteAgent : Agent
    {


        //BEGIN::Game controller variables
        private ThirdPersonCharacter character;
        private Transform m_CamTransform;
        private Vector3 m_CamForward;             // The current forward direction of the camera
        private Vector3 m_Move;
        //END::

        //BEGIN::motor controll variables
        private static float fx, fy;
        private float speed = 0.0f;
        private bool crouch;
        private bool jump;
        private float leftTurn = 0;
        private float rightTurn = 0;
        private float up = 0;
        private float down = 0;
        private bool pushing;
        private bool getpickup;
        private bool usewalkspeed = true;
        private float walkspeed = 0.5f;
 
        public int rayCastingWidth;
        public int rayCastingHeight;
        //END::

        public int GameID;


        public Text hud;

        private GameObject player;
        
        private PlayerRemoteSensor sensor;

        public Camera m_camera;

        private Rigidbody mRigidBody;

        public float initialEnergy = 30;

        private float energy;

        public float energyRatio = 5.0f/60.0f;

        private int touchID = 0;

        private bool done;

        public GameObject TopLeftCorner, BottonRightCorner;

        public GameObject[] respawnPositions;

        public GameObject[] fruits;
        public GameObject[] fires;

        private float reward = 0;

        public GameObject restartButton;

        public bool get_result = false;

        private float touchX = 0, touchY = 0, touchZ = 0;


        // Use this for initialization
        void Start()
        {
            
            fires = new GameObject[3];
            fruits = new GameObject[3];
            for (int i = 1; i <= 3; i++){
                fires[i-1] = GameObject.Find("Game" + GameID + "/FireBall" + i);
                fruits[i-1] = GameObject.Find("Game" + GameID + "/LifeBall" + i);
            }

            mRigidBody = GetComponent<Rigidbody>();
            Respawn(false);
            if (!gameObject.activeSelf)
            {
                return;
            }
            player = GameObject.FindGameObjectsWithTag("Player")[0];

            if (m_camera != null)
            {
                m_CamTransform = m_camera.transform;
            }
            else
            {
                Debug.LogWarning(
                    "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.", gameObject);
                // we use self-relative controls in this case, which probably isn't what the user wants, but hey, we warned them!
            }

            // get the third person character ( this should never be null due to require component )
            character = GetComponent<ThirdPersonCharacter>();
            sensor = new PlayerRemoteSensor();
            sensor.Start(m_camera, player, this.rayCastingHeight, this.rayCastingWidth);
        }
        
        private float deltaTime = 0;

        private void Respawn(bool respawn=true){
            reward = 0;
            if (respawn) {
                foreach(GameObject fruit in fruits){
                    fruit.GetComponent<RandomAgent>().Respawn();
                }

                foreach(GameObject fire  in fires){
                    fire.GetComponent<RandomAgent>().Respawn();
                }
            }
            getpickup = false;
            deltaTime = 0;
            energy = initialEnergy;
            touchID = 0;
            ResetState();
            done = false;
            int idx = Random.Range(0, respawnPositions.Length);
            //mRigidBody.position = respawnPositions[idx].transform.position;

            Vector3 pos = respawnPositions[idx].transform.position;
            mRigidBody.velocity = Vector3.zero;
            mRigidBody.MovePosition(pos);
        }

        private void ResetState()
        {
            reward = 0;
            touchID = 0;
            fx = 0;
            fy = 0;
            crouch = false;
            jump = false;
            pushing = false;
            leftTurn = 0;
            rightTurn = 0;
            up = 0;
            down = 0;
            get_result = false;
            touchX = 0;
            touchY = 0;
            touchZ = 0;
        }


        private void UpdateHUD(){
            if (hud != null) {
                hud.text = "Energy: " + System.Math.Round(energy,2) + "\tReward: " + reward + "\tDone: " + done + "\tGameID: " + GameID;
            }
        }

        public override void ApplyAction()
        {
            string action = GetActionName();
            if (action.Equals("get_result")){
                    get_result = true;
            } else if (action.Equals("restart")) {
                Respawn();
            } else if (!done) {
                switch (action)
                {
                    case "walk":
                        fx = 0;
                        fy = GetActionArgAsFloat();
                        increaseEnergy(-0.0001f);
                        break;
                    case "run":
                        fx = 0;
                        fy = GetActionArgAsFloat();
                        increaseEnergy(-0.0005f);
                        break;
                    case "walk_in_circle":
                        fx = GetActionArgAsFloat();;
                        fy = 0;
                        increaseEnergy(-0.0001f);
                        break;
                    case "right_turn":
                        rightTurn = GetActionArgAsFloat();
                        break;
                    case "left_turn":
                        leftTurn = GetActionArgAsFloat();
                        break;
                    case "up":
                        up = GetActionArgAsFloat();
                        break;
                    case "down":
                        down = GetActionArgAsFloat();
                        break;
                    case "push":
                        pushing = GetActionArgAsBool();
                        increaseEnergy(-0.01f);
                        break;
                    case "jump":
                        jump = GetActionArgAsBool();
                        increaseEnergy(-0.1f);
                        break;
                    case "crouch":
                        crouch = GetActionArgAsBool();
                        break;
                    case "pickup":
                        getpickup = GetActionArgAsBool();
                        break;
                }
            }
        }


        // Update is called once per frame
        public override void UpdatePhysics()
        {
            deltaTime += Time.deltaTime;
            if (deltaTime > 1.0){
                energy -= energyRatio;
                if (energy < 0){
                    energy = 0;
                }
                deltaTime = 0;
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
            character.Move(m_Move, crouch, jump, rightTurn - leftTurn, down - up, pushing, fx, fy, getpickup);
            //character.Move(m_Move, crouch, m_Jump, h, v, pushing);
            jump = false;
            sensor.UpdateViewMatrix();
            float x = transform.localPosition.x;
            float z = transform.localPosition.z;
            float tx = TopLeftCorner.transform.localPosition.x;
            float bx = BottonRightCorner.transform.localPosition.x;
            float tz = TopLeftCorner.transform.localPosition.z;
            float bz = BottonRightCorner.transform.localPosition.z;
            if (x < tx || x > bx || z > tz || z < bz) {
                done = true;
                reward += 10;
            }
        }

        private void increaseEnergy(float inc){
            energy += inc;
            if (energy>50) {
                energy = 50;
            }

            if (energy < 0) {
                energy = 0;
            }
        }

        private void updateTouchPosition(Collision other) {
            foreach(ContactPoint contact in other.contacts) {
                Vector3 lp = transform.InverseTransformPoint(contact.point);
                touchX = lp.x;
                touchY = lp.y;
                touchZ = lp.z;
            }
        }


        /// <summary>
        /// OnCollisionEnter is called when this collider/rigidbody has begun
        /// touching another rigidbody/collider.
        /// </summary>
        /// <param name="other">The Collision data associated with this collision.</param>
        void OnCollisionStay(Collision other)
        {
            if (!done) {
                //Debug.Log("COlission with " + other.gameObject.name);
                if (other.gameObject.tag.Equals("Fire")){
                    updateTouchPosition(other);
                    touchID = -2;
                    if (getpickup) {
                        increaseEnergy(-energy);
                    }
                } else if (other.gameObject.tag.Equals("Life")){
                    updateTouchPosition(other);
                    touchID = 2;
                    if (getpickup) {
                        increaseEnergy(10);
                    }
                } else if (other.gameObject.name.Equals("maze1")) {
                    updateTouchPosition(other);
                    touchID = -1;
                }
            }
        }

        public override void UpdateState()
        {
            if (energy <= 0) {
                energy = 0;
                if (!done){
                    reward -= 10;
                }
                done = true;
            }

            SetStateAsString(0, "frame", sensor.getCurrentRayCastingFrame());
            SetStateAsFloat(1, "reward", reward);
            SetStateAsFloat(2, "touchID", touchID);
            SetStateAsFloat(3, "energy", energy);
            SetStateAsBool(4, "done", done);
            SetStateAsFloat(5, "tx", touchX);
            SetStateAsFloat(6, "ty", touchY);
            SetStateAsFloat(7, "tz", touchZ);
            UpdateHUD();
            if(get_result) {
                ResetState();
            }
        }
    }

    public class PlayerRemoteSensor
    {
        private byte[] currentFrame;
        
        private Camera m_camera;

        private GameObject player;

        private int life, score;
        private float energy;


        private int verticalResolution = 20;
        private int horizontalResolution = 20;
        private bool useRaycast = true;

        private Ray[,] raysMatrix = null;
        private int[,] viewMatrix = null;
        private Vector3 fw1 = new Vector3(), fw2 = new Vector3(), fw3 = new Vector3();

        
        public void SetCurrentFrame(byte[] cf)
        {
            this.currentFrame = cf;
        }

        // Use this for initialization
        public void Start(Camera cam, GameObject player, int rayCastingHRes, int rayCastingVRes)
        {
            this.verticalResolution = rayCastingVRes;
            this.horizontalResolution = rayCastingHRes;
            life = 0;
            score = 0;
            energy = 0;
            useRaycast = true;
            currentFrame = null;

            m_camera = cam;
            this.player = player;
            fw3 = m_camera.transform.forward;


            if (useRaycast)
            {
                if (raysMatrix == null)
                {
                    raysMatrix = new Ray[verticalResolution, horizontalResolution];
                }
                if (viewMatrix == null)
                {
                    viewMatrix = new int[verticalResolution, horizontalResolution];

                }
                for (int i = 0; i < verticalResolution; i++)
                {
                    for (int j = 0; j < horizontalResolution; j++)
                    {
                        raysMatrix[i, j] = new Ray();
                    }
                }
                currentFrame = updateCurrentRayCastingFrame();
            }    
        }



        public byte[] updateCurrentRayCastingFrame()
        {
            UpdateRaysMatrix(m_camera.transform.localPosition, m_camera.transform.forward, m_camera.transform.up, m_camera.transform.right);
            UpdateViewMatrix();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < verticalResolution; i++)
            {
                for (int j = 0; j < horizontalResolution; j++)
                {
                    sb.Append(viewMatrix[i, j]).Append(",");
                }
                sb.Append(";");
            }
            return Encoding.UTF8.GetBytes(sb.ToString().ToCharArray());
        }


        public string getCurrentRayCastingFrame()
        {
            UpdateRaysMatrix(m_camera.transform.position, m_camera.transform.forward, m_camera.transform.up, m_camera.transform.right);
            UpdateViewMatrix();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < verticalResolution; i++)
            {
                for (int j = 0; j < horizontalResolution; j++)
                {
                    sb.Append(viewMatrix[i, j]);
                    if (j < horizontalResolution-1){
                        sb.Append(",");
                    }
                }
                if (i < verticalResolution-1){
                    sb.Append(";");
                }
            }
            return sb.ToString();
        }

        private void UpdateRaysMatrix(Vector3 position, Vector3 forward, Vector3 up, Vector3 right, float fieldOfView = 90.0f)
        {


            float vangle = 2 * fieldOfView / verticalResolution;
            float hangle = 2 * fieldOfView / horizontalResolution;

            float ivangle = -fieldOfView;

            for (int i = 0; i < verticalResolution; i++)
            {
                float ihangle = -fieldOfView;
                fw1 = (Quaternion.AngleAxis(ivangle + vangle * i, right) * forward).normalized;
                fw2.Set(fw1.x, fw1.y, fw1.z);

                for (int j = 0; j < horizontalResolution; j++)
                {
                    raysMatrix[i, j].origin = position;
                    raysMatrix[i, j].direction = (Quaternion.AngleAxis(ihangle + hangle * j, up) * fw2).normalized;
                }
            }
        }

        public void UpdateViewMatrix(float maxDistance = 500.0f)
        {
            for (int i = 0; i < verticalResolution; i++)
            {
                for (int j = 0; j < horizontalResolution; j++)
                {
                    RaycastHit hitinfo;
                    if (Physics.Raycast(raysMatrix[i, j], out hitinfo, maxDistance))
                    {
                        string objname = hitinfo.collider.gameObject.name;
                        if (objname.Equals("Terrain")){
                                viewMatrix[i, j] = 1;
                        } else if (objname.StartsWith("maze")){
                                viewMatrix[i, j] = 2;
                        } else {
                                objname = hitinfo.collider.gameObject.tag;
                                if (objname.Equals("Fire"))
                                {
                                    viewMatrix[i, j] = -3;
                                }
                                else if (objname.Equals("Life"))
                                {
                                    viewMatrix[i, j] = 3;
                                }
                                else
                                {
                                    viewMatrix[i, j] = 0;
                                }
                        }
                    }
                    else
                    {
                        viewMatrix[i, j] = 0;
                    }
                }
            }
        }
    }
}