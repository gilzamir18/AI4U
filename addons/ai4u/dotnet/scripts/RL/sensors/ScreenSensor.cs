using Godot;
using System;

namespace ai4u;

/// <summary>
/// This sensor get a screenshot from the viewport.
/// </summary>
public partial class ScreenSensor : Sensor
{

    /// <summary>
    /// The <code>grayScale</code> field defines the format of the generated image.If true, the image format is L8,
    /// containing only the luminance of the captured image from the viewport.
    /// Otherwise, the format is RGB8.Prefer grayscale images for performance reasons.
    /// </summary>
    [Export]
    private bool grayScale = true;

    /// <summary>
    /// The <code>width</code> field is the image width in pixels.
    /// </summary>
    [Export]
    private int width = 61;
    
    /// <summary>
    /// The <code>height</code> field is the tmage height in pixels.
    /// </summary>
    [Export]
    private int height = 61;

    private Image currentImg = null;
    private MaxSizeQueue<string> history;

    public override void OnSetup(Agent agent)
    {
        MakeData();
        this.agent = (RLAgent)agent;
        rangeMin = 0;
        rangeMax = 255;
    }

    private void MakeData()
    {
        type = SensorType.sstrings;
        history = new(stackedObservations);

        if (grayScale)
        {
            shape = new int[] { stackedObservations, width, height };
        }
        else
        {
            shape = new int[] { stackedObservations, 3, width, height};
        }
    }
    
    public override int GetIntValue()
    {
        return 0;
    }

    public override string[] GetStringValues()
    {
        currentImg = GetViewport().GetTexture().GetImage();
        
        if (grayScale)
        {
            currentImg.Convert(Image.Format.L8);
        }
        currentImg.Resize(width, height);
        var dt = currentImg.SavePngToBuffer();
        history.Enqueue(System.Convert.ToBase64String(dt));
        while (history.Count < stackedObservations)
        {
            history.Enqueue(System.Convert.ToBase64String(dt));
        }

        return history.Values;
    }

    public override SensorType GetDataType()
    {
        return SensorType.simage;
    }

    public override void OnReset(Agent agent)
    {
        MakeData();
    }
}
