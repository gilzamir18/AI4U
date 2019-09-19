using unityremote;
using UnityEngine;

public class HumanController : Controller
{
    public override object[] GetAction()
    {
        return GetFloatArrayAction("move", new float[] { 0.1f, 0.1f });
    }

    public override void NewStateEvent()
    {
        //string field = GetStateName();
        //float[] pos = GetStateAsFloatArray();
        //Debug.Log(field);
        //Debug.Log(string.Join(", ", pos));
    }
}
