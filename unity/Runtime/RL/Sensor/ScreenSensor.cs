using UnityEngine;
using System.Linq;
using ai4u;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine.Networking;
using System.Collections;

namespace ai4u
{
    public class ScreenSensor : Sensor
    {
        public int width = 50;
        public int height = 50;
        public bool grayScale = true;
        private byte[] lastFrame;
        private Texture2D destinationTexture;
        private Queue<byte[]> values;
        void CreateData()
        {
            values = new Queue<byte[]>(stackedObservations);
            var img = lastFrame;
            for (int i = 0; i < stackedObservations; i++)
                    values.Enqueue(img);
        }

        public override void OnSetup(Agent agent) 
        {
            if (grayScale)
            {
                lastFrame = new byte[width * height];
            }
            else
            {
                lastFrame = new byte[width * height * 3];
            }
            StartCoroutine(UpdateMyFrame());
            type = SensorType.sstring;
            shape = new int[2]{width,  height};
            rangeMin = 0;
            rangeMax = 255;
            CreateData();
            agent.AddResetListener(this);
            this.agent = (BasicAgent) agent;
        }

        public override string GetStringValue()
        {
            values.Enqueue(lastFrame);        
            if (values.Count > stackedObservations)
            {
                values.Dequeue();
            }
            byte[] sb = new byte[0];
            foreach (var f in values)
            {
                sb = sb.Concat(f).ToArray();
            }
            return System.Convert.ToBase64String(sb);
        }
        
        void Update()
        {
            StartCoroutine(UpdateMyFrame());
        }

        public override void OnReset(Agent agent)
        {
            if (grayScale)
            {
                lastFrame = new byte[width * height];
            }
            else
            {
                lastFrame = new byte[width * height * 3];
            }
            CreateData();
        }

        IEnumerator UpdateMyFrame()
        {
            yield return new WaitForEndOfFrame();
            // Create a texture the size of the screen, RGB24 format
            int width = Screen.width;
            int height = Screen.height;
            destinationTexture = new Texture2D(width, height,  TextureFormat.RGBA32, false);
            // Read screen contents into the texture
            destinationTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            destinationTexture.Apply();
            // Encode texture into PNG
            if (grayScale)
            {
                lastFrame = GrayScaleTexture(destinationTexture, this.width, this.height);
            }
            else
            {
                lastFrame = ScaleTexture(destinationTexture, this.width, this.height);
            }
            Destroy(destinationTexture);
        }

        private byte[] ScaleTexture(Texture2D source, int targetWidth, int targetHeight, int ch = 3) {
            float incX=(1.0f / (float)targetWidth);
            float incY=(1.0f / (float)targetHeight);
            byte[] bytes = new byte[targetWidth * targetHeight * ch];
            int k = 0;
            for (int i = 0; i < targetHeight; ++i) {
                for (int j = 0; j < targetWidth; ++j) {
                    Color newColor = source.GetPixelBilinear((float)j / (float)targetWidth, (float)i / (float)targetHeight);
                    bytes[k] = (byte)(255 * newColor.r);
                    bytes[k+1] = (byte)(255 * newColor.g);
                    bytes[k+2] = (byte)(255 * newColor.b);
                    k += ch;
                }
            }
            return bytes;
        }
    

        private byte[] GrayScaleTexture(Texture2D source, int targetWidth,int targetHeight) {
            float incX=(1.0f / (float)targetWidth);
            float incY=(1.0f / (float)targetHeight);
            byte[] bytes = new byte[targetWidth * targetHeight];
            int k = 0;
            for (int i = 0; i < targetHeight; ++i) {
                for (int j = 0; j < targetWidth; ++j) {
                    Color newColor = source.GetPixelBilinear((float)j / (float)targetWidth, (float)i / (float)targetHeight);
                    bytes[k++] = (byte)(255 * newColor.grayscale); 
                }
            }
            return bytes;
        }

    }
}
