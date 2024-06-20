using Godot;
using System;

namespace ai4u;

/// <summary>
/// This sensor get a screenshot from a Viewport object. 
/// Rendered image can be projected on a Subviewport if set <code>viewport</code> field 
/// with a valid Subviewport object. 
/// </summary>
public partial class Camera3DSensor : Sensor
{

    ///<summary
    /// Source camera witch the image will be captured.
    ///</summary>
    [Export]
    private Camera3D camera;


    /// <summary>
    /// The <code>grayScale</code> field defines the format of the generated image.If true, the image format is L8,
    /// containing only the luminance of the captured image from the viewport.
    /// Otherwise, the format is RGB8. Prefer grayscale images for performance reasons.
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
    
    /// <summary>
    /// The viewport to rendering. If null, it use default viewport.
    /// </summary>
    [Export]
    private SubViewport viewport;

    private Image currentImg = null;

    private MaxSizeQueue<string> history;

    public override void OnSetup(Agent agent)
    {
        this.agent = (BasicAgent)agent;
        if (viewport != null)
        {
            this.viewport.Size = new Vector2I(width, height);
            this.viewport.World3D = GetViewport().World3D;
            RenderingServer.ViewportAttachCamera(viewport.GetViewportRid(), camera.GetCameraRid());
        }

        MakeData();
        normalized = false;
    }

    private void MakeData()
    {
        type = SensorType.sstrings;
        history = new(stackedObservations);

        if (grayScale)
        {
            shape = new int[] {stackedObservations, width, height };
        }
        else
        {
            shape = new int[] {stackedObservations, 3, width, height};
        }

        normalized = false;
        rangeMin = 0;
        rangeMax = 255;
    }
    
    public override int GetIntValue()
    {
        return 0;
    }

    public override string[] GetStringValues()
    {

        if (viewport != null)
        {
            currentImg = viewport.GetTexture().GetImage();
        }
        else
        {
            currentImg = camera.GetViewport().GetTexture().GetImage();
        }

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
