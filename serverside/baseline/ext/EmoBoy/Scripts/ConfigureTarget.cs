using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ai4u;
using ai4u.ext;

public class ConfigureTarget : MonoBehaviour, IAgentResetListener
{

    public GameObject reference;
    public float radio;
    public float minDistanceRate=0.5f;
    public float maxHeight = 35;

    // Start is called before the first frame update
    void Start()
    {
        reference.GetComponent<RLAgent>().AddResetListener(this);
        ResetConfig();
    }

    private void ResetConfig() {
        string[] args = System.Environment.GetCommandLineArgs ();

        int i = 0;
        while (i < args.Length){
            switch (args[i]) {
                case "--emoboy_radio":
                    radio = float.Parse(args[i+1], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    i += 2;
                    break;
                case "--emoboy_mindistancerate":
                    minDistanceRate = float.Parse(args[i+1], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    i += 2;
                    break;
                case "--emoboy_maxheight":
                    maxHeight = float.Parse(args[i+1], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    i += 2;
                    break;
                default:
                    i+=1;
                    break;
            }
        }
        RaycastHit hitinfo;
        DPRLAgent agent = reference.GetComponent<DPRLAgent>();
        Vector3 p = reference.transform.position + radio * Vector3.forward * Random.Range(minDistanceRate, 1.0f) + Vector3.up * Random.Range(1,maxHeight);
        if (Physics.Raycast(p, Vector3.up * -1,  out hitinfo, maxHeight)) {
            p = hitinfo.point;
            if (p.y <= 0) {
                p = new Vector3(p.x, 0.2f, p.z);
            }
        }
        transform.position = p;
        transform.RotateAround(reference.transform.position, Vector3.up, 360 * Random.Range(0.0f, 1.0f));
    }

    public void OnReset(Agent agent) {
        ResetConfig();
    }
}
