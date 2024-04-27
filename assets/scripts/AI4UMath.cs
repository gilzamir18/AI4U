namespace ai4u.math;

public sealed class AI4UMath
{
    private AI4UMath()
    {

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