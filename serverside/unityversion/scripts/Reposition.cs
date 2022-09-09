using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ai4u;

public class Reposition : MonoBehaviour, IAgentResetListener
{
    public float minX;
    public float maxX;
    public float minZ;
    public float maxZ;
    public float y;
    public BasicAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        agent.AddResetListener(this);
    }

    // Update is called once per frame
    public void OnReset(Agent agent)
    {
        Debug.Log("Restart");
        Vector3 position;
        position.x = Random.Range(minX, maxX);
        position.y = y;
        position.z = Random.Range(minZ, maxZ);
        transform.position = position;
    }
}
