using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using unityremote;
using System.Text;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class petanim : Agent
{

    //BEGIN::GUI
    public Text gameStatus;
    //END::GUI

    //BEGIN::Physical Body
    Rigidbody mRigidBody;
    public GameObject feet;
    public GameObject mouth;
    public GameObject eye;
    private GameObject body;
    //END::Physical Body

    //BEGIN::Management
    public GameObject gameManager;
    private bool hitButton = false;
    private bool hitRightButton = false;
    private bool rewardByHitButtonUsed = false;
    //END::Management

    //BEGIN::Environment Limits
    public GameObject left, right, top, bottom;
    //END::Environment Limits

    //BEGIN:Internal
    public int initialEnergy = 500;
    private int energy;
    private int energy_delta_time = 0;
    public int energyUpdateInterval = 100;
    private bool energyGainLocker = false;
    private bool done = false;
    public GameObject brain;
    //END::Internal

    //BEGIN::Move Variables
    Animator m_Animator;
    public float speed = 1.0f;
    public float angularSpeed = 1.0f;
    public float jumpPower = 10000;
    public float verticalSpeed = 2.0f;
    private bool onGround;
    //END::Move Variables

    //BEGIN::Artificial Vision Variables
    private int verticalResolution = 20;
    private int horizontalResolution = 20;
    public float visionMaxDistance = 500f;
    private Ray[,] raysMatrix = null;
    private int[,] viewMatrix = null;
    private Vector3 fw1 = new Vector3(), fw2 = new Vector3();
    //END::Artificial Vision Variables

    //BEGIN::Touch Sense
    private int touched_id = -1;
    private bool lockTouch = false;
    //END::Touch Sense

    //BEGIN::::EMOTIONAL VARIABLES
    public float happiness = 0.0f;
    public float surprise = 0.0f;
    public float fear = 0.0f;
    public float disgust = 0.0f;
    public float sadness = 0.0f;
    public float neutral = 0.0f;
    public float anger = 0.0f;
    //END::::EMOTIONAL VARIABLES

    //BEGIN::System
    private static string[] cmdInfo = null;
    private static int inputPort = 8081;
    private static int outputPort = 8080;
    private static string host = "127.0.0.1";
    //END::System
    
    void Awake()
    {
        //Time.timeScale = 10;
        if (cmdInfo == null)
        {
            try
            {
                cmdInfo = System.Environment.GetCommandLineArgs();
                if (cmdInfo.Length > 3)
                {
                    inputPort = int.Parse(cmdInfo[1]);
                    outputPort = int.Parse(cmdInfo[2]);
                    host = cmdInfo[3];
                }
            }
            catch (System.Exception ex)
            {
                inputPort = 8081;
                outputPort = 8080;
                host = "127.0.0.1";
            }
        }

        if (brain.GetComponent<RemoteBrain>().enabled)
        {
            gameStatus.text = host;
            brain.GetComponent<RemoteBrain>().port = inputPort;
            brain.GetComponent<RemoteBrain>().remotePort = outputPort;
            brain.GetComponent<RemoteBrain>().remoteIP = host;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        body = transform.GetChild(2).gameObject;
        m_Animator = GetComponent<Animator>();
        mRigidBody = GetComponent<Rigidbody>();
        raysMatrix = new Ray[verticalResolution, horizontalResolution];
        viewMatrix = new int[verticalResolution, horizontalResolution];
        ResetParams();
    }

    public void ResetParams() {
        touched_id = 0;
        energy = initialEnergy;
        onGround = false;
        touched_id = -1;
        signal = 0;
        energyGainLocker = false;
        energy_delta_time = 0;
        hitRightButton = false;
        hitButton = false;
        lockTouch = false;
        rewardByHitButtonUsed = false;
    }


    public float Signal {
        get {
            return this.signal;
        }

        set {
            this.signal = value;
        }
    }

    private bool CheckAgentGoal()
    {
        float lx = left.transform.position.x;
        float rx = right.transform.position.x;
        float by = bottom.transform.position.z;
        float ty = top.transform.position.z;
        float x = transform.position.x;
        float z = transform.position.z;

        return x < rx && x > lx && z > by && z < ty;
    }

    public int Energy {
        get {
            return energy;
        }
    }

    private bool IsDone() {
        return done;
    }

    private float v=0, h=0, jump=0, push=0, signal=0;
    override public void ApplyAction()
    {
        v = 0; h = 0; jump = 0; push = 0; //signal = 0;
        string actionName = GetActionName();
        if (actionName == "act"){
            if (done)
            {
                return;
            }
            int action = GetActionArgAsInt();
            switch(action){
                case 0:
                    v = 1;
                    break;
                case 1:
                    h = 1;
                    break;
                case 2:
                    v = 1;
                    h = 1;
                    break;
                case 3:
                    v = -1;
                    break;
                case 4:
                    h = -1;
                    break;
                case 5:
                    v = -1;
                    h = -1;
                    break;
                case 6:
                    v = 1;
                    h = -1;
                    break;
                case 7:
                    v = -1;
                    h = 1;
                    break;
                case 8:
                    jump =  1;
                    break;
                case 9:
                    push = 1;
                    break;
                case 10:
                    //Debug.Log("signal");
                    signal = 0;
                    break;
                case 11:
                    signal = 1;
                    break;
            }
        } else if (actionName == "emotion"){
            float[] args = GetActionArgAsFloatArray();
            happiness = args[0];
            surprise = args[1];
            fear = args[2];
            disgust = args[3];
            sadness = args[4];
            neutral = args[5];
            anger = args[6];
        } else if (actionName == "get_status"){
            //Debug.Log("checking status...");
        } else if (actionName == "restart") {
            mRigidBody.velocity = Vector3.zero;
            mRigidBody.angularVelocity = Vector3.zero;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public int[,] GetViewMatrix() {
        return this.viewMatrix;
    }

    private byte[] updateCurrentRayCastingFrame()
    {
        UpdateRaysMatrix(eye.transform.position, eye.transform.forward, eye.transform.up, eye.transform.right);
        UpdateViewMatrix();
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < verticalResolution; i++)
        {
            for (int j = 0; j < horizontalResolution; j++)
            {
                //Debug.DrawRay(raysMatrix[i, j].origin, raysMatrix[i, j].direction, Color.red);
                sb.Append(viewMatrix[i, j]);
                if (j <= horizontalResolution-2)
                    sb.Append(",");
            }
            if (i <= verticalResolution - 2)
                sb.Append(";");

        }
        return Encoding.UTF8.GetBytes(sb.ToString().ToCharArray());
    }

    private void UpdateRaysMatrix(Vector3 position, Vector3 forward, Vector3 up, Vector3 right, float fieldOfView = 45.0f)
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
                //Debug.DrawRay(raysMatrix[i,j].origin, raysMatrix[i,j].direction * visionMaxDistance, Color.red);
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
                if (Physics.Raycast(raysMatrix[i, j], out hitinfo, visionMaxDistance))
                {
                    //Debug.DrawRay(raysMatrix[i,j].origin, raysMatrix[i,j].direction * visionMaxDistance, Color.yellow);
   
                    GameObject gobj = hitinfo.collider.gameObject;
                    petanim other = gobj.GetComponent<petanim>();

                    string objname = gobj.name;
                    string objtag = gobj.tag;
                    if (objtag == "gate") {
                        viewMatrix[i, j] = 6;
                    } else if (objtag == "wall") {
                        viewMatrix[i, j] = 11;
                    } else if (objtag == "action")
                    {
                        if (objname == "Action1")
                        {
                            viewMatrix[i, j] = 16;
                        }
                        else if (objname == "Action2")
                        {
                            viewMatrix[i, j] = 21;
                        }
                        else if (objname == "Action3")
                        {
                            viewMatrix[i, j] = 21;
                        }
                        else
                        {
                            viewMatrix[i, j] = 16;
                        }
                    } else if (objtag == "ground")
                    {
                        viewMatrix[i, j] = 1;
                    }
                }
                else
                {
                    viewMatrix[i, j] = 0;
                }
            }
        }
    }

    private Color petOriginalColor = new Color(1.0f, 0.0f, 0.7451f, 1.0f);
    override public void UpdatePhysics()
    {
        if (onGround){
            mRigidBody.velocity = Vector3.zero;
        }

        anger = signal;
        happiness = signal;
        if (signal > 0)
            body.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        else
            body.GetComponent<Renderer>().material.SetColor("_Color", petOriginalColor);
        
        //BEGIN::UPDATE EMOTIONAL STATE
        m_Animator.SetFloat("happiness", happiness);
        m_Animator.SetFloat("surprise", surprise);
        m_Animator.SetFloat("fear", fear);
        m_Animator.SetFloat("sadness", sadness);
        m_Animator.SetFloat("anger", anger);
        m_Animator.SetFloat("neutral", neutral);
        m_Animator.SetFloat("disgust",disgust);
        //END::UPDATE EMOTIONAL STATE

        //BEGIN::UPDATE PHYSICS STATE
        Vector3 fwd = gameObject.transform.forward;

        mRigidBody.angularVelocity = new Vector3(0, h * angularSpeed, 0);
        mRigidBody.velocity = v * speed * fwd;
        
        
        if (jump < 0 || !onGround) {
            mRigidBody.velocity = new Vector3(mRigidBody.velocity.x, - verticalSpeed, mRigidBody.velocity.z);
        } else if (onGround && jump > 0){
            mRigidBody.AddForce(Vector3.up * jumpPower);
        }
        //END::UPDATE PHYSICS STATE


        //BEGEIN::UPDATE BIO
    
        if (energy_delta_time >= energyUpdateInterval) {
            energy -= 1;
            energy_delta_time = 0;
        } else {
            energy_delta_time += 1;
        }
        //END::UPDATE BIO
        v = 0; h = 0; jump = 0; push = 0; //signal = 0;
    }

    
    /// <summary>
    /// OnCollisionEnter is called when this collider/rigidbody has begun
    /// touching another rigidbody/collider.
    /// </summary>
    /// <param name="other">The Collision data associated with this collision.</param>
    void OnCollisionStay(Collision other)
    {
        string objtag = other.gameObject.tag;
        if (objtag == "ground")
        {
            foreach (ContactPoint contact in other.contacts)
            {
                if (Vector3.Distance(contact.point, feet.transform.position) < 2.0f)
                {
                    onGround = true;
                    break;
                }
                //print(contact.thisCollider.name + " hit " + contact.otherCollider.name);
                // Visualize the contact point
                //Debug.DrawRay(contact.point, contact.normal, Color.white);
            }
        }

        if (done || lockTouch)
        {
            return;
        }

        string objname = other.gameObject.name;
        if (objtag == "gate")
        {
            touched_id = 6;
        } else if (objtag == "wall"){
            touched_id = 11;
        } else if (objtag == "action")
        {
            if (objname == "Action1")
            {
                touched_id = 16;
            } else if (objname == "Action2")
            {
                touched_id = 21;
            } else if (objname == "Action3")
            {
                touched_id = 21;
                hitButton = true;
                lockTouch = true;
                if (other.gameObject.GetComponent<GateController>().IsOpenController)
                {
                    hitRightButton = true;
                } else
                {
                    hitRightButton = false;
                }
            }
            else
            {
                touched_id = 16;
                hitButton = true;
                lockTouch = true;
                if (other.gameObject.GetComponent<GateController>().IsOpenController)
                {
                    hitRightButton = true;
                }
                else
                {
                    hitRightButton = false;
                }
            }
        }
        else if (objtag == "ground")
        {
            touched_id = 1;
        }
        else 
        {
            touched_id = 0;
        }
    }

    /// <summary>
    /// OnCollisionExit is called when this collider/rigidbody has
    /// stopped touching another rigidbody/collider.
    /// </summary>
    /// <param name="other">The Collision data associated with this collision.</param>
    void OnCollisionExit(Collision other)
    {
        energyGainLocker = false;

        if (other.gameObject.tag == "ground"){
            onGround = false;
        }
        touched_id = 0;
    }

    override public void UpdateState()
    {
        float reward = 0;
        
        if (!done && CheckAgentGoal())
        {
            gameStatus.text = "You Win!!!";
            done = true;
            reward = 1;
        } else if (!done && energy <= 0)
        {
            gameStatus.text = "Game Over!!!";
            done = true;
        } else if (hitButton && !done && !rewardByHitButtonUsed)
        {
            rewardByHitButtonUsed = true;
            if (hitRightButton)
            {
                gameStatus.text = "Reward: 1";
                reward = 1;
            }
            else
            {
                gameStatus.text = "Reward: -1";
                reward = -1;
            }
        }
        
        //BEGIN::UPDATE AGENT VISION
        UpdateRaysMatrix(eye.transform.position + 1.5f * gameObject.transform.forward, gameObject.transform.forward, gameObject.transform.up, gameObject.transform.right, 45);
        UpdateViewMatrix(visionMaxDistance);
        byte[] frame = updateCurrentRayCastingFrame();
        SetStateAsByteArray(0, "frame", frame);
        SetStateAsBool(1, "done", done);
        SetStateAsInt(2, "energy", energy);
        SetStateAsInt(3, "touched", touched_id);
        SetStateAsFloat(4, "signal", signal);
        SetStateAsFloat(5, "reward", reward);
        SetStateAsFloat(6, "x", transform.position.x);
        SetStateAsFloat(7, "y", transform.position.y);
        SetStateAsFloat(8, "z", transform.position.z);
        //END::UPDATE AGENT VISION
    }
}
