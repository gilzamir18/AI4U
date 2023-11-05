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
    private HistoryStack<float> history;
    
    public override void OnSetup(Agent agent)
    {
        base.OnSetup(agent);
        if (grayScale)
        {
            shape = new int[] { width * height };
        }
        else
        {
            shape = new int[] { width, height };
        }
        type = SensorType.sfloatarray;
        history = new HistoryStack<float>(stackedObservations * width * height);
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
                        history.Push(0.3f * c.R + 0.59f * c.G + 0.11f * c.B);
                    }
                    else
                    {
                        var c = currentImg.GetPixel(i, j);
                        history.Push(c.R);
                        history.Push(c.G);
                        history.Push(c.B);
                    }
                }
            }
        }
        return history.Values;
    }
}
