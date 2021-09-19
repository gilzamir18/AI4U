using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ai4u;
using ai4u.ext;

public class ConfigureTarget : MonoBehaviour, IAgentResetListener
{

    public GameObject reference;
    public float radio;
    public float inBuildingProb=0.0f;
    public int inBuildingLevel = 0;
    public float minDistanceRate=0.5f;
    public float maxHeight = 35;

    // Start is called before the first frame update
    void Start()
    {
        reference.GetComponent<RLAgent>().AddResetListener(this);
        ResetConfig();
    }

    private Transform FindWithTag(GameObject parent, string tag) {
        foreach (Transform child in parent.transform)
        {
            if (child.CompareTag(tag)) {
                return child;
            }
        }
        return null;
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
                case "--emoboy_inbuildingprob":
                    inBuildingProb = float.Parse(args[i+1], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    i += 2;
                    break;
                case "--emoboy_inbuildinglevel":
                    inBuildingLevel = int.Parse(args[i+1]);
                    i += 2;
                    break;
                default:
                    i+=1;
                    break;
            }
        }
        string buildingLevel = "TargetPos";
        if (inBuildingLevel == 2) {
            buildingLevel = "TargetPos2";
        } else if (inBuildingLevel == 3) {
            buildingLevel = "TargetPos3";
        }

        float prob = Random.Range(0.0f, 1.0f);
        if (prob < inBuildingProb) {
            ArrayList list = new ArrayList();
            if (inBuildingLevel == 1 || inBuildingLevel == -1) {
                GameObject[] objs = GameObject.FindGameObjectsWithTag("TargetPos");
                foreach(GameObject obj in objs) {
                    list.Add(obj);
                }
            }
            if (inBuildingLevel == 2 || inBuildingLevel == -1) {
                GameObject[] objs = GameObject.FindGameObjectsWithTag("TargetPos2");
                foreach(GameObject obj in objs) {
                    list.Add(obj);
                }
            }
            if (inBuildingLevel == 3 || inBuildingLevel == -1) {
                GameObject[] objs = GameObject.FindGameObjectsWithTag("TargetPos3");
                foreach(GameObject obj in objs) {
                    list.Add(obj);
                }
            }
            int idx = Random.Range(0, list.Count);
            GameObject pos = (GameObject) list[idx];
            transform.position = pos.transform.position;            
        } else {
            bool isToRatate = true;
            DPRLAgent agent = reference.GetComponent<DPRLAgent>();
            RaycastHit hitinfo;
            Vector3 p = reference.transform.position + radio * Vector3.forward * Random.Range(minDistanceRate, 1.0f) + Vector3.up * maxHeight;
            if (Physics.Raycast(p, Vector3.up * -1,  out hitinfo, maxHeight+10)) {
                p = hitinfo.point;
                string __tag = hitinfo.collider.gameObject.tag;
                if ( __tag == "Terrain") {
                    p = new Vector3(p.x, p.y + 0.2f, p.z);
                } else if (__tag=="Wall") {
                    Transform r = FindWithTag(hitinfo.collider.gameObject, buildingLevel);
                    p = r.position;
                    isToRatate = false;
                } else {
                    p = new Vector3(p.x, 0.2f, p.z);
                }
            }

            if (isToRatate){
                transform.position = p;
                transform.RotateAround(reference.transform.position, Vector3.up, 360 * Random.Range(0.0f, 1.0f));
                p = transform.position;
            }

            GameObject[] walls  = GameObject.FindGameObjectsWithTag("Wall");
            foreach(GameObject wall in walls) {
                MeshCollider collider = wall.GetComponent<MeshCollider>();
                if (collider.bounds.Contains(p)) {
                    Transform r = FindWithTag(wall, buildingLevel);
                    transform.position = r.position;
                    break;
                }
            }
        }
    }

    public void OnReset(Agent agent) {
        ResetConfig();
    }
}
