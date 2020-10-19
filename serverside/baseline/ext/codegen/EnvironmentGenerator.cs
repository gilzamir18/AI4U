using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ai4u;

namespace ai4u.ext {


    public class EnvironmentGenerator : MonoBehaviour
    {
        public string outputPath = "";
        public Agent[] agents;
        public string arrayInputName;
        public int inputArraySize;
        public string imageInputName;
        public int imageInputWidth;
        public int imageInputHeight;
        public int imageInputChannels;
        public int actionShape;
        public bool actionSpaceIsContinue = false;
        public EnvironmentTemplate template;
        public bool runAgent = false;
        public string[] actionName;
        public string[] actionValue;
        public int skipFrames = 1;

        public bool render = true;

        void Update()
        {
            if (render) {
                render = false;
                foreach (Agent agent in agents)
                {
                    string filename =  agent.name + ".py";
                    int[] inputShape = new int[1];
                    inputShape[0] = inputArraySize;
                    if (arrayInputName != string.Empty) {
                        int arrayStateIdx = agent.GetStateIndex(arrayInputName);
                        byte arrayStateType = agent.GetStateType(arrayStateIdx);
                        ModelInput arrayInput = new ModelInput(arrayInputName, inputShape);
                        arrayInput.isImage = false;
                        if (arrayStateType == Brain.FLOAT_ARRAY) {
                            arrayInput.type = Dtype.float32;
                        }  else {
                            throw new System.NotSupportedException("Input array type is not supported: " + arrayStateType);
                        }
                        template.AddInput(arrayInput);
                    }
                    
                    if (imageInputName != string.Empty) {
                        int[] ishape = new int[]{imageInputWidth, imageInputHeight, imageInputChannels};
                        ModelInput imageInput = new ModelInput(imageInputName, ishape);
                        int imageStateIdx = agent.GetStateIndex(imageInputName);
                        byte imageStateType = agent.GetStateType(imageStateIdx);
                        if (imageStateType == Brain.STR) {
                            imageInput.type = Dtype.str;
                        } else if (imageStateType == Brain.OTHER) {
                            imageInput.type = Dtype.int8;
                        } else {
                            throw new System.NotSupportedException("Input image type is not supported: " + imageStateType);
                        }
                        template.AddInput(imageInput);
                    }

                    ModelOutput output = new ModelOutput(actionShape);
                    output.type = Dtype.float32;
                    if (actionSpaceIsContinue) {
                        output.categorical = false;
                    } else {
                        output.categorical = true;
                    }

                    string code = template.GenCode(this, agent);
                    string fpath = System.IO.Path.Combine(outputPath, filename);
                    System.IO.File.WriteAllText(fpath, code);
                    if (runAgent) {
                        throw new System.NotSupportedException("Operation yet supported!!!");
                        //run_cmd("python", fpath, "");
                    }
                }
            }
        }

        private void run_cmd(string cmd="python", string filename="", string args="")
        {
            System.Diagnostics.ProcessStartInfo start = new System.Diagnostics.ProcessStartInfo();
            start.FileName = filename;
            start.Arguments = string.Format("{0} {1}", cmd, args);
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            using(System.Diagnostics.Process process = System.Diagnostics.Process.Start(start))
            {
                using(System.IO.StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    System.Console.Write(result);
                }
            }
        }
    }
}