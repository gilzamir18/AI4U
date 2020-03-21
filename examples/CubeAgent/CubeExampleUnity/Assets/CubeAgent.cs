using UnityEngine;
using unityremote;

public class CubeAgent : Agent
{
    private Vector3 translation;
    void Start()
    {
        translation = Vector3.zero;
    }
    public override void ApplyAction()
    {
        string action = GetActionName();
        if (action.Equals("restart")){
            transform.position = Vector3.zero;
            translation = Vector3.zero;
        }
        if (action.Equals("tx"))
        {
            float tx = GetActionArgAsFloat();
            translation.Set(tx, 0, 0);
            transform.position = transform.position + translation;
        }
    }
}
