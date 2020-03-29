using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopViewCamControl : MonoBehaviour
{

    public Camera[] cameras;
    private Camera camera;
    private GameObject[] canvas = null;

    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<Camera>();
        camera.enabled = true;
        foreach (Camera c in cameras){
            c.enabled = false;
        }

        canvas = GameObject.FindGameObjectsWithTag("canvas");
        SetAgentCamera(0);
    }

    private const float dist = 520;

    private void SetAgentCamera(int ID){
          for (int i = 0; i < canvas.Length; i++){
            canvas[i].SetActive(false);
          }
          GameObject obj = GameObject.Find("Game"+(ID+1));
          Vector3 pos = obj.transform.position;
          GameObject mycanvas = obj.transform.GetChild(21).gameObject;
          mycanvas.SetActive(true);
          transform.position = new Vector3(pos.x, pos.y + dist, pos.z);
          transform.LookAt(pos);
    }



    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)){
            SetAgentCamera(0);
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2)){
            SetAgentCamera(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3)){
            SetAgentCamera(2);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4)){
            SetAgentCamera(3);
        }

        if (Input.GetKeyDown(KeyCode.Alpha5)){
            SetAgentCamera(4);
        }

        if (Input.GetKeyDown(KeyCode.Alpha6)){
            SetAgentCamera(5);
        }

        if (Input.GetKeyDown(KeyCode.Alpha7)){
            SetAgentCamera(6);
        }

        if (Input.GetKeyDown(KeyCode.Alpha8)){
            SetAgentCamera(7);
        }
    }
}
