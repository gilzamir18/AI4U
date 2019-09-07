using UnityEngine;
using unityremote;

public class CubeAgent : Agent
{
    private Vector3 translation;
    void Start()
    {
        translation = new Vector3(0, 0, 0);
    }
    public override void ApplyAction()
    {
        string action = GetActionName();
        float tx = GetActionArgAsFloat();
        translation.Set(tx, 0, 0);
        if (action.Equals("tx"))
        {
            transform.position = transform.position + translation;
        }
    }
}
