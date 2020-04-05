using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetFrame : MonoBehaviour
{

    public int frameWidth;
    public int frameHeight;
    private Texture2D image = null;

    public static byte[] currentFrame = new byte[] { };

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        RenderTexture.active = source;
        if (image == null)
            image = new Texture2D(source.width, source.height);
        image.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
        //TextureScale.Bilinear(image, frameWidth, frameHeight);
        currentFrame = image.EncodeToPNG();
        Graphics.Blit(source, destination);
        RenderTexture.active = destination;
    }
}
