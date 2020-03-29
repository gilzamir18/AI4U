using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using unityremote;

public class RandomAgent : MonoBehaviour
{

    private float fx;
    private float fy;

    private Rigidbody mRigidbody;

    private Vector3[] dir;
    private Vector3 TopLeftCorner;
    private Vector3 LowerRightCorner;

    public GameObject TopLeftCornerObject;
    public GameObject LowerRightCornerObject;


    public float speed = 10;
    private Vector3 mPos;

    // Start is called before the first frame update
    void Start()
    {
        TopLeftCorner = TopLeftCornerObject.transform.position;
        LowerRightCorner = LowerRightCornerObject.transform.position;
        
        dir = new Vector3[]{Vector3.forward, Vector3.forward * -1, Vector3.left, Vector3.right};
        mRigidbody = GetComponent<Rigidbody>();
        mPos = transform.localPosition;
    }

    public void Respawn(){
        mRigidbody.velocity = Vector3.zero;
        transform.localPosition = mPos;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float x = transform.position.x;
        float z = transform.position.z;

        float tx = TopLeftCorner.x;
        float tz = TopLeftCorner.z;

        float lx = LowerRightCorner.x;
        float lz = LowerRightCorner.z;
        bool inRoom = true;
        if (x < tx) {
            inRoom = false;
            mRigidbody.AddForce(Vector3.right * speed);
        } else if (x > lx) {
            inRoom = false;
            mRigidbody.AddForce(Vector3.left * speed);
        } 
        
        if (z < lz) {
            inRoom = false;
            mRigidbody.AddForce(Vector3.forward * speed);
        } else if (z > tz) {
            inRoom = false;
            mRigidbody.AddForce(Vector3.forward * (-speed));
        }

        if (inRoom){
            mRigidbody.AddForce(dir[Random.Range(0, 3)] * speed);
        }
    }
}
