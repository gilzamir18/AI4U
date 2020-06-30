using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using UnityEngine;

public class GateInfo : MonoBehaviour
{


    private float relativeHeight;

    // Start is called before the first frame update
    void Start()
    {
        relativeHeight = 0;
    }

    public float RelativeHeight {
        set
        {
            this.relativeHeight = value;
        }

        get
        {
            return this.relativeHeight;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
