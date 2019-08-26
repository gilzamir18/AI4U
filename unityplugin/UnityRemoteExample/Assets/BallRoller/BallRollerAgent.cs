using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using unityremote;

public class BallRollerAgent : Agent
{

    public GameObject target;

    private Rigidbody rBody;

    public float speed = 10;

    private bool done = false;
    private float reward = 0.0f;

    public void Start()
    {
        rBody = GetComponent<Rigidbody>();
        done = false;
    }

    private void ResetPlayer()
    {
        done = false;
        rBody.velocity = Vector3.zero;
        rBody.angularVelocity = Vector3.zero;
        target.transform.position = new Vector3(Random.value * 6 - 3, 0.5f, Random.value * 6 - 3);
        transform.position = new Vector3(0, 0.5f, 0);
        reward = 0;
    }

    public override void UpdatePhysics()
    {
        if (gameObject.transform.position.y < 0.0 && !done)
        {
            done = true;
        }
    }

    public override void ApplyAction()
    {
        float fx = 0.0f;
        float fz = 0.0f;
        switch (GetActionName())
        {
            case "fx":
                fx = GetActionArgAsFloat();
                break;
            case "fz":
                fz = GetActionArgAsFloat();
                break;
            case "restart":
                Debug.Log("Restarting");
                ResetPlayer();
                break;
        }
        if (rBody != null)
        {
            rBody.AddForce(fx * speed, 0, fz * speed);
        }
    }

    public override bool UpdateState()
    {
        SetStateAsBool(0, "done", done);
        SetStateAsFloat(1, "reward", reward);
        SetStateAsFloat(2, "tx", target.transform.position.x);
        SetStateAsFloat(3, "tz", target.transform.position.z);
        SetStateAsFloat(4, "vx", rBody.velocity.x);
        SetStateAsFloat(5, "vz", rBody.velocity.z);
        SetStateAsFloat(6, "x", transform.position.x);
        SetStateAsFloat(7, "y", transform.position.y);
        SetStateAsFloat(8, "z", transform.position.z);
        return true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name.Equals("Target") && !done)
        {
            reward = 1;
            done = true;
        }
    }
}
