namespace ai4u.math;

public static class AI4UMath
{

    public static int GetArgMax(float[] v)
    {
        float max = float.NegativeInfinity;
        int idx = -1;
        for (int i = 0; i < v.Length; i++)
        {
            if (v[i] > max)
            {
                max = v[i];
                idx = i;
            }
        }
        return idx;
    }

    public static float Clip(float v, float start, float end)
    {
        if (v < start)
        {
            return start;
        }
        else if (v > end)
        {
            return end;
        }
        else
        {
            return v;
        }
    }
}