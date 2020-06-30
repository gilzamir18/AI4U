using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateController : MonoBehaviour
{
    public GameObject targetGate;
    public float speed = 1;
    public float maxHeight = 80;
    public bool reusable = true;


    private bool on = false;
    private GateInfo gateInfo;
    private bool isOpenController = true;

    // Start is called before the first frame update
    void Start()
    {
        gateInfo = targetGate.GetComponent<GateInfo>();
    }

    void OnCollisionEnter(Collision other)
    {
        if (targetGate != null && other.gameObject.tag == "agent" && !on)
        {
            on = true;
        }
    }

    public bool IsOpenController
    {
         set
        {
            isOpenController = value;
        }

        get
        {
            return isOpenController;
        }
    }

    private void Open()
    {
        if (on)
        {
            Vector3 pos = targetGate.transform.position;
            if (gateInfo.RelativeHeight < maxHeight)
            {
                gateInfo.RelativeHeight += speed;
                targetGate.transform.Translate(0, speed, 0);
            } else
            {
                if (reusable)
                {
                    on = false;
                }
            }
        }
    }

    private void Close()
    {
        if (on)
        {
            if (gateInfo.RelativeHeight > 0)
            {
                Vector3 pos = targetGate.transform.position;
                gateInfo.RelativeHeight -= speed;
                targetGate.transform.Translate(0, -speed, 0);
            } else
            {
                if (reusable)
                {
                    on = false;
                }
            }
        }
    }



    // Update is called once per frame
    void Update()
    {
        if (IsOpenController)
        {
            Open(); 
        } else
        {
            Close();
        }
    }
}
