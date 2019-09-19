using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using unityremote;

public class MoveBall : Agent
{

    private Rigidbody rb;
    private Vector3 force;
    private float[] m_position = new float[] { 0, 0};
    private Vector2Int hv = new Vector2Int();
    private bool NoOp = false; 

    // Start is called before the first frame update
    void Start()
    {   
        rb = GetComponent<Rigidbody>();
        force = Vector3.zero;
    }

    public override void ApplyAction()
    {
        string action = GetActionName();
        if (action.Equals("move"))
        {
            float[] args = GetActionArgAsFloatArray();
            force.Set(args[0], args[1], 0);
            NoOp = false;
        } else if (action.Equals("NoOp"))
        {
            force.Set(0, 0, 0);
            NoOp = true;
        }
    }

    public override void UpdatePhysics()
    {
        rb.AddForce(force);
        force.Set(0, 0, 0);
    }

    private const float err = 0.01f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.contactCount > 0)
        {
            ContactPoint point = collision.GetContact(0);

            
            float dx = point.point.x - transform.position.x;
            float dy = point.point.y - transform.position.y;

            int horizontal = 0;
            int vertical = 0;
            if (Mathf.Abs(dx) > err)
            {
                if (dx > 0)
                {
                    horizontal = 1;
                } else
                {
                    horizontal = -1;
                }
            }

            if (Mathf.Abs(dy) > err)
            {
                if (dy > 0)
                {
                    vertical = 1;
                } else
                {
                    vertical = -1;
                }
            }
            
            hv.Set(horizontal, vertical);
            //Debug.Log(hv.x + " " + hv.y);
        }
    }

    public override void UpdateState()
    {
        m_position[0] = transform.position.x;
        m_position[1] = transform.position.y;
        SetStateAsFloatArray(0, "position", m_position);
        SetStateAsInt(1, "ht", hv.x);
        SetStateAsInt(2, "vt", hv.y);
        if (NoOp)
        {
            hv.Set(0, 0);
            NoOp = false;
        }
    }
}
