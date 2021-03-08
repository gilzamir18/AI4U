using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{



    public Camera headViewCam;
    public GameObject target;
    public Button actionerSwapCam;

    private Camera topViewCam;

    // Start is called before the first frame update
    void Start()
    {
        headViewCam.enabled = true;
        topViewCam = GetComponent<Camera>();
        topViewCam.enabled = false;
        if (actionerSwapCam != null) actionerSwapCam.onClick.AddListener(OnClick);

    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(target.transform.position);
    }

    void OnClick(){
        topViewCam.enabled = !topViewCam.enabled;
        headViewCam.enabled = !headViewCam.enabled;
    }
}
