using Godot;

namespace ai4u;

public class VectorUtils
{

    private VectorUtils()
    {

    }

    public static Vector3 Lerp(Vector3 a, Vector3 b, float weight)
    {
        Vector3 r = new Vector3();
        r.X = Mathf.Lerp(a.X, b.X, weight);
        r.Y = Mathf.Lerp(a.Y, b.Y, weight);
        r.Z = Mathf.Lerp(a.Z, b.Z, weight);

        return r;
    }
}
