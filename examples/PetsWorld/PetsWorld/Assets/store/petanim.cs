using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using unityremote;
using System.Text;

public class petanim : Agent
{
    Animator m_Animator;

    Rigidbody mRigidBody;

    public int ID = 0;


    public int maxEnergy=300;

    public int initialEnergy = 10;

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

    private int energy;

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


    public GameObject txtName;

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
        mRigidBody = GetComponent<Rigidbody>();
        raysMatrix = new Ray[verticalResolution, horizontalResolution];
        viewMatrix = new int[verticalResolution, horizontalResolution];
        if (txtName != null) {
            txtName.GetComponent<TextMesh>().text = "Agent" + ID;
        }
        ResetParams();
    }

    public void ResetParams() {
        touched_id = 0;
        energy = initialEnergy;
        onGround = false;
        touched_id = -1;
    }

    public int Energy {
        get {
            return energy;
        }
    }

    private bool IsDone() {
        return this.energy <= 0 || this.energy > maxEnergy;
    }

    private float v=0, h=0, jump=0, push=0, signal=0;

    override public void ApplyAction()
    {
        v = 0; h = 0; jump = 0; push = 0; signal = 0;
        string actionName = GetActionName();
        //Debug.Log(action);
        if (actionName == "act"){
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
            Debug.Log("checking status...");
        } else if (actionName == "restart") {
            if (IsDone()) {
                Manager.instance.Respawn(gameObject);
                mRigidBody.velocity = Vector3.zero;
                mRigidBody.angularVelocity = Vector3.zero;
                ResetParams();
            }
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
                            viewMatrix[i, j] = code + 10;
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
        if (IsDone()){
            if (onGround){
                mRigidBody.velocity = Vector3.zero;
            }
            return;
        }

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
            if (energy < 0) {
                energy = 0;
            }
            energy_delta_time = 0;
        } else {
            energy_delta_time += 1;
        }
        //END::UPDATE BIO
        v = 0; h = 0; jump = 0; push = 0; signal = 0;
    }

    /// <summary>
    /// OnCollisionEnter is called when this collider/rigidbody has begun
    /// touching another rigidbody/collider.
    /// </summary>
    /// <param name="other">The Collision data associated with this collision.</param>
    void OnCollisionStay(Collision other)
    {
        if (IsDone()) {
            return;
        }

        string objtag = other.gameObject.tag;
        string objname = other.gameObject.name;
        if (objtag == "eating")
        {
            touched_id = 2;
            energy += eggValue;
            Destroy(other.gameObject);
            manager.DestroyEgg();
        } else if (objname.StartsWith("block"))
        {
            touched_id = 3;
        } else if (objtag == "wall"){
            touched_id = 4;
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
        }
    }

    /// <summary>
    /// OnCollisionExit is called when this collider/rigidbody has
    /// stopped touching another rigidbody/collider.
    /// </summary>
    /// <param name="other">The Collision data associated with this collision.</param>
    void OnCollisionExit(Collision other)
    {
        if (IsDone()) {
            return;
        }

        if (other.gameObject.tag == "agent") {
            int code = int.Parse(other.gameObject.name.Split('_')[1]);
            petanim anim = other.gameObject.GetComponent<petanim>();
            touched_id = code + 10;
            this.energy += 10;
            anim.energy += 10;
        } else if (other.gameObject.tag == "ground"){
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
        SetStateAsBool(1, "done", IsDone());
        SetStateAsInt(2, "energy", energy);
        SetStateAsInt(3, "id", ID);
        SetStateAsInt(4, "touched", touched_id);
        SetStateAsFloat(5, "totalenergy", Manager.instance.SumOfEnergy);
        //END::UPDATE AGENT VISION
    }
}
