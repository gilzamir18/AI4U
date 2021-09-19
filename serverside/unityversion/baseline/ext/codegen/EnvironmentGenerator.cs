using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ai4u;
using System;

namespace ai4u.ext {


    [Serializable]
    public struct StringPair
    {
        public string name;
        public string value;
        public StringPair(string n, string v)
        {
            this.name = n;
            this.value = v;
        }
    }

    [Serializable]
    public struct ActionDescription 
    {
        public string name;
        public string value;
        public string description;


        public ActionDescription(string name, string value)
        {
            this.name = name;
            this.value = value;
            this.description = "";
        }
    }


    public class EnvironmentGenerator : MonoBehaviour
    {
        public string outputPath = "";
        public DPRLAgent[] agents;
        public bool hasLinearInput;
        public bool hasImageInput;
        public int actionShape;
        public bool actionSpaceIsContinue = false;
        public EnvironmentTemplate template;
        public bool runAgent = false;
        public ActionDescription[] actions;
        public int skipFrames = 1;

        public bool render = true;

        void Update()
        {
            if (render) {
                render = false;
                foreach (DPRLAgent agent in agents)
                {
                    string filename =  agent.name + ".py";
                    if (hasLinearInput) {
                        ModelInput linearInput = new ModelInput(false);
                        template.AddInput(linearInput);
                    }
                    
                    if (hasImageInput) {
                        ModelInput imageInput = new ModelInput(true);
                        template.AddInput(imageInput);
                    }

                    ModelOutput output = new ModelOutput(actionShape);
                    output.type = Dtype.float32;
                    if (actionSpaceIsContinue) {
                        output.categorical = false;
                    } else {
                        output.categorical = true;
                    }

                    template.AddOutput(output);

                    string code = template.GenCode(this, agent);
                    string fpath = System.IO.Path.Combine(outputPath, filename);
                    System.IO.File.WriteAllText(fpath, code);
                    if (runAgent) {
                        throw new System.NotSupportedException("Operation yet supported!!!");
                        //run_cmd("python", fpath, "");
                    }
                }
                Debug.Log("Python render done.");
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