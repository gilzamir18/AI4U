using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{

    public GameObject action1;
    public GameObject action2;
    public GameObject action3;
    public GameObject action4;

    public GameObject gate1Pos1;
    public GameObject gate1Pos2;
    public GameObject gate2Pos1;
    public GameObject gate2Pos2;

    public GameObject agent;

    public GameObject respawn1, respawn2, respawn3;

    // Start is called before the first frame update
    void Awake()
    {
        int choosePosition = Random.Range(0, 4);
        int chooseRespawn = Random.Range(0, 3);
        int chooseOpenController = Random.Range(0, 2);

        if (chooseOpenController ==  0)
        {
            action1.GetComponent<GateController>().IsOpenController = true;
            action2.GetComponent<GateController>().IsOpenController = false;
            action3.GetComponent<GateController>().IsOpenController = false;
            action4.GetComponent<GateController>().IsOpenController = true;
        } else
        {
            action1.GetComponent<GateController>().IsOpenController = false;
            action2.GetComponent<GateController>().IsOpenController = true;
            action3.GetComponent<GateController>().IsOpenController = true;
            action4.GetComponent<GateController>().IsOpenController = false;
        }

        switch(chooseRespawn)
        {
            case 0:
                agent.transform.position = respawn1.transform.position;
                break;
            case 1:
                agent.transform.position = respawn2.transform.position;
                break;
            case 2:
                agent.transform.position = respawn3.transform.position;
                break;
        }

        switch (choosePosition)
        {
            case 0:
                action1.transform.position = gate1Pos1.transform.position;
                action2.transform.position = gate1Pos2.transform.position;
                action3.transform.position = gate2Pos1.transform.position;
                action4.transform.position = gate2Pos2.transform.position;
                break;
            case 1:
                action1.transform.position = gate1Pos2.transform.position;
                action2.transform.position = gate1Pos1.transform.position;
                action3.transform.position = gate2Pos1.transform.position;
                action4.transform.position = gate2Pos2.transform.position;
                break;
            case 2:
                action1.transform.position = gate1Pos1.transform.position;
                action2.transform.position = gate1Pos2.transform.position;
                action3.transform.position = gate2Pos2.transform.position;
                action4.transform.position = gate2Pos1.transform.position;
                break;
            case 3:
                action1.transform.position = gate1Pos2.transform.position;
                action2.transform.position = gate1Pos1.transform.position;
                action3.transform.position = gate2Pos2.transform.position;
                action4.transform.position = gate2Pos1.transform.position;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
