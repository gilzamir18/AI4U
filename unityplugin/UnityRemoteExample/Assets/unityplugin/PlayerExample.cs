using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using unityremote;

public class PlayerExample : MonoBehaviour
{

    private RemoteBrain remote;
    private string[] desc = new string[] { "done" };
    private byte[] type = new byte[] { 2 };

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void GetState()
    {
        if (this.remote == null)
        {
            Debug.Log("Remote is not defined...");
            return;
        }

        if (gameObject.transform.position.x < 10)
        {
            this.remote.SendMessage(desc, type, new string[] { "0" });
        }
        else
        {
            this.remote.SendMessage(desc, type, new string[] { "1" });
        }
    }

    void SetRemote(RemoteBrain remote)
    {
        this.remote = remote;
    }

    void Left(string[] args)
    {
        float d = float.Parse(args[0], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        Vector3 pos = gameObject.transform.position;
        gameObject.transform.Translate(new Vector3(-d, 0, 0));
    }

    void Right(string[] args)
    {
        Debug.Log("RIGHT");
        float d = float.Parse(args[0], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        Vector3 pos = gameObject.transform.position;
        gameObject.transform.Translate(new Vector3(d, 0, 0));
    }

    void Up(string[] args)
    {
        float d = float.Parse(args[0], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        Vector3 pos = gameObject.transform.position;
        gameObject.transform.Translate(new Vector3(0, d, 0));
    }

    void Down(string[] args)
    {
        float d = float.Parse(args[0], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        Vector3 pos = gameObject.transform.position;
        gameObject.transform.Translate(new Vector3(0, -d, 0));
    }
}
