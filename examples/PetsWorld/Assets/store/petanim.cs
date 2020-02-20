using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using unityremote;
using System.Text;

public class petanim : Agent
{
    Animator m_Animator;

    Rigidbody rigidBody;

    public int ID = 0;

    private const int MAX_AGENT_ENERGY = 10;

    //BEGIN::CONTROL VARIABLES
    public float speed = 1.0f;
    public float angularSpeed = 1.0f;

    public float jumpPower = 10000;

    public float verticalSpeed = 2.0f;

    public float visionMaxDistance = 500f;

    public GameObject eye;

    public int eggValue = 50;

    private int verticalResolution = 20;
    private int horizontalResolution = 20;
    private bool useRaycast = true;

    private int energy_delta_time = 0;
    public int energyUpdateInterval = 100;

    private int touched_id = -1;

    private int energy = MAX_AGENT_ENERGY;

    private Ray[,] raysMatrix = null;
    private int[,] viewMatrix = null;
    private Vector3 fw1 = new Vector3(), fw2 = new Vector3();

    public GameObject feet;

    public GameObject mouth;

    private Manager manager;

    private bool onGround;
    //END::CONTROL VARIABLES

    //BEGIN::::EMOTIONAL VARIABLES
    public float happiness = 0.0f;
    public float surprise = 0.0f;
    public float fear = 0.0f;
    public float disgust = 0.0f;
    public float sadness = 0.0f;
    public float neutral = 0.0f;
    public float anger = 0.0f;
    //END::::EMOTIONAL VARIABLES

    //BEGIN::::VARIAVEIS DE PERSONALIDADE
    public const int MOT_CURIOSITY = 0;
    public const int MOT_SOCIAL_CONTACT = 1;
    public const int MOT_EATING = 2;
    public const int MOT_SAVING = 3;
    public const int MOT_ACCEPTANCE = 4;
    public const int MOT_STATUS = 5;    
    public const int POWER = 6;

    public const int NB_PERSONALITY_VARS = 7;

    private float[] personality_profile = new float[NB_PERSONALITY_VARS];
    private float[] personlity_weights = new float[NB_PERSONALITY_VARS];
    //END::::PERSONALITY VARIABLES  

    // Start is called before the first frame update
    void Start()
    {
        manager = Manager.instance;
        m_Animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        raysMatrix = new Ray[verticalResolution, horizontalResolution];
        viewMatrix = new int[verticalResolution, horizontalResolution];
        ResetParams();
    }

    public void ResetParams() {
        touched_id = 0;
        energy = MAX_AGENT_ENERGY;
    }

    public int Energy {
        get {
            return energy;
        }
    }

    private float v, h, jump, push, signal;

    override public void ApplyAction()
    {
        string action = GetActionName();
        //Debug.Log(action);
        if (action == "act") {
            float[] args = GetActionArgAsFloatArray();
            v = args[0];
            h =  args[1];
            jump = args[2];
            push = args[3];
            signal = args[4];
            if (args.Length >= 12) {
                happiness = args[5];
                surprise = args[6];
                fear = args[7];
                disgust = args[8];
                sadness = args[9];
                neutral = args[10];
                anger = args[11];
            }
        } else if (action == "emotion"){
            float[] args = GetActionArgAsFloatArray();
            happiness = args[0];
            surprise = args[1];
            fear = args[2];
            disgust = args[3];
            sadness = args[4];
            neutral = args[5];
            anger = args[6];
        } else if (action == "get_status"){
            Debug.Log("checking status...");
        }
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
                sb.Append(viewMatrix[i, j]).Append(",");
            }
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
   
                    string objname = hitinfo.collider.gameObject.name;
                    
                    if (objname == "Terrain") {
                        viewMatrix[i, j] = 1;
                    } else {
                        string objtag = hitinfo.collider.gameObject.tag;
                        if (objtag == "eating")
                        {
                            viewMatrix[i, j] = 2;
                        } else if (objname.StartsWith("block"))
                        {
                            viewMatrix[i, j] = 3;
                        } else if (objtag == "wall"){
                            viewMatrix[i, j] = 4;
                        } else if (objtag == "agent") {
                            int code = int.Parse(objname.Split('_')[1]);
                            viewMatrix[i, j] = code;
                        } else {
                            viewMatrix[i, j] = -1;
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

    override public void UpdatePhysics()
    {
        if (energy <= 0){
            rigidBody.velocity = Vector3.zero;
            return;
        }

        if (Manager.stopped) {
            return;
        }




        //BEGIN::UPDATE EMOTIONAL STATE
        m_Animator.SetFloat("happiness", happiness);
        m_Animator.SetFloat("surprise", surprise);
        m_Animator.SetFloat("fear", fear);
        m_Animator.SetFloat("sadness", sadness);
        m_Animator.SetFloat("anger", anger);
        m_Animator.SetFloat("neutral", neutral);
        m_Animator.SetFloat("disgust", disgust);
        //END::UPDATE EMOTIONAL STATE

        //BEGIN::UPDATE PHYSICS STATE
        Vector3 fwd = gameObject.transform.forward;

        rigidBody.angularVelocity = new Vector3(0, h * angularSpeed, 0);
        rigidBody.velocity = v * speed * fwd;
        
        
        if (jump < 0 || !onGround) {
            rigidBody.velocity = new Vector3(rigidBody.velocity.x, - verticalSpeed, rigidBody.velocity.z);
        } else if (onGround){
            rigidBody.AddForce(Vector3.up * jumpPower);
        }
        //END::UPDATE PHYSICS STATE


        //BEGEIN::UPDATE BIO
    
        if (energy_delta_time >= energyUpdateInterval) {
            energy -= 1;
            if (energy < 0) {
                energy = 0;
            }
            energy_delta_time = 0;
        } else {
            energy_delta_time += 1;
        }
        //END::UPDATE BIO
    }

    /// <summary>
    /// OnCollisionEnter is called when this collider/rigidbody has begun
    /// touching another rigidbody/collider.
    /// </summary>
    /// <param name="other">The Collision data associated with this collision.</param>
    void OnCollisionStay(Collision other)
    {
        if (Manager.stopped) {
            return;
        }

        string objtag = other.gameObject.tag;
        string objname = other.gameObject.name;
        if (objtag == "eating")
        {
            touched_id = 2;
        } else if (objname.StartsWith("block"))
        {
            touched_id = 3;
        } else if (objtag == "wall"){
            touched_id = 4;
        } else if (objtag == "agent") {
            int code = int.Parse(objname.Split('_')[1]);
            petanim anim = other.gameObject.GetComponent<petanim>();
            if (this.energy < anim.energy) {
                this.energy = anim.energy;
            }
            touched_id = code;
        } else if (objname == "Terrain") {
            touched_id = 1;
        } else {
            touched_id = 0;
        }

        if (other.gameObject.tag == "ground"){
            foreach (ContactPoint contact in other.contacts)
            {
                if (Vector3.Distance(contact.point, feet.transform.position) < 2.0f){
                    onGround = true;
                    break;
                }
                //print(contact.thisCollider.name + " hit " + contact.otherCollider.name);
                // Visualize the contact point
                //Debug.DrawRay(contact.point, contact.normal, Color.white);
            }
        } else if (other.gameObject.tag == "eating") {
            foreach (ContactPoint contact in other.contacts)
            {
                if (Vector3.Distance(contact.point, mouth.transform.position) < 2.0f){
                    energy += eggValue;

                    Destroy(other.gameObject);
                    manager.DestroyEgg();
                    break;
                }
            }
        }    
    }

    /// <summary>
    /// OnCollisionExit is called when this collider/rigidbody has
    /// stopped touching another rigidbody/collider.
    /// </summary>
    /// <param name="other">The Collision data associated with this collision.</param>
    void OnCollisionExit(Collision other)
    {
        if (Manager.stopped) {
            return;
        }

        if (other.gameObject.tag == "ground"){
            onGround = false;
        }
        touched_id = 0;
    }

    override public void UpdateState()
    {
        //BEGIN::UPDATE AGENT VISION
        UpdateRaysMatrix(eye.transform.position + 1.5f * gameObject.transform.forward, gameObject.transform.forward, gameObject.transform.up, gameObject.transform.right, 45);
        UpdateViewMatrix(visionMaxDistance);
        byte[] frame = updateCurrentRayCastingFrame();
        SetStateAsByteArray(0, "frame", frame);
        SetStateAsBool(1, "done", energy <= 0);
        SetStateAsInt(2, "energy", energy);
        SetStateAsInt(3, "id", ID);
        SetStateAsInt(4, "touched", touched_id);
        SetStateAsBool(5, "status", Manager.stopped);
        //END::UPDATE AGENT VISION
    }
}
