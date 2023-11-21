using Godot;
using System;

namespace ai4u;

public partial class ScreenSensor : Sensor
{

    [Export]
    private bool grayScale = true;
    [Export]
    private int width = 30;
    [Export]
    private int height = 30;

    private Image currentImg = null;
    private HistoryStack<int> ihistory;
    private HistoryStack<float> fhistory;

    public override void OnSetup(Agent agent)
    {
        MakeData();
    }

    private void MakeData()
    {
        if (grayScale)
        {
            shape = new int[] { width, height };
            if (Normalized)
            {
                fhistory = new(stackedObservations * width * height);
                type = SensorType.sfloatarray;
            }
            else
            {
                ihistory = new(stackedObservations * width * height);
                type = SensorType.sintarray;
            }
        }
        else
        {
            shape = new int[] { width, height, 3 };
            if (Normalized)
            {
                fhistory = new(stackedObservations * width * height * 3);
                type = SensorType.sfloatarray;
            }
            else
            {
                ihistory = new(stackedObservations * width * height);
                type = SensorType.sintarray;
            }
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (agent != null && !agent.Done)
        {
            currentImg = GetViewport().GetTexture().GetImage();
        }
    }

    public override float[] GetFloatArrayValue()
    {
        if (currentImg != null)
        {
            for (int i = 0; i < currentImg.GetWidth(); i++)
            {
                for (int j = 0; j < currentImg.GetHeight(); j++)
                {
                    if (grayScale)
                    {
                        var c = currentImg.GetPixel(i, j);
                        fhistory.Push(0.3f * c.R + 0.59f * c.G + 0.11f * c.B);
                    }
                    else
                    {
                        var c = currentImg.GetPixel(i, j);
                        
                        fhistory.Push(c.R);
                        fhistory.Push(c.G);
                        fhistory.Push(c.B);
                    }
                }
            }
        }

        return fhistory.Values;
    }

    public override int[] GetIntArrayValue()
    {
        if (currentImg != null)
        {
            for (int i = 0; i < currentImg.GetWidth(); i++)
            {
                for (int j = 0; j < currentImg.GetHeight(); j++)
                {
                    if (grayScale)
                    {
                        var c = currentImg.GetPixel(i, j);
                        ihistory.Push(Mathf.RoundToInt( 0.3f * c.R * 255 + 0.59f * c.G * 255 + 0.11f * c.B * 255) );
                    }
                    else
                    {
                        var c = currentImg.GetPixel(i, j);

                        ihistory.Push(Mathf.RoundToInt(c.R * 255));
                        ihistory.Push(Mathf.RoundToInt(c.G * 255));
                        ihistory.Push(Mathf.RoundToInt(c.B * 255));
                    }
                }
            }
        }

        return ihistory.Values;
    }

    public override void OnReset(Agent agent)
    {
        MakeData();
    }
}
