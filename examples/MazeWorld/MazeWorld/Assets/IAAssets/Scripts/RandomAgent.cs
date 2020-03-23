using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAgent : MonoBehaviour
{

    private float fx;
    private float fy;

    private Rigidbody mRigidbody;

    private Vector3[] dir;

    public float speed = 10;

    // Start is called before the first frame update
    void Start()
    {
        fx = 0;
        fy = 0;
        dir = new Vector3[]{Vector3.forward, Vector3.forward * -1, Vector3.left, Vector3.right};
        mRigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        mRigidbody.AddForce(dir[Random.Range(0, 3)] * speed);
    }
}
