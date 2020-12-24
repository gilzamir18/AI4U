using System.Collections;
using System.Collections.Generic; 
using UnityEngine;
using ai4u;

namespace ai4u.ext {
    public class BaselinePPO2Template: EnvironmentTemplate
    {
        private SensorsExtractor sensorsExtractor = new SensorsExtractor();
        public float linearInputNorm = 100.0f;
        public string modelName = "ppo2model";
        public string outputLogName = "ppo2log";
        public int timeSteps = 500000;
        public int inputPort = 8080;
        public int outputPort = 7070;
        public bool useLSTM = false;

        public override string GenCode(EnvironmentGenerator gen, DPRLAgent agent)
        {
            string head = System.IO.File.ReadAllText("Assets/baseline/baselineppo2_head.tpl");
            string text = System.IO.File.ReadAllText("Assets/baseline/baselineppo2.tpl");

            int dim = inputs.Count;

            if (dim == 1) {
                if (!inputs[0].isImage) { //only linear input
                    string code;
                    int shape = 0;
                    if (sensorsExtractor.ComputingLinearInput(agent, out code, out shape)) 
                    {
                        text = head + text;
                        text = text.Replace("#DISABLE51", "");
                        text = text.Replace("#TPL_RETURN_STATE", "return[np.array(" + code + "), None]");
                        text = text.Replace("#TPL_INPUT_SHAPE", "(" + shape + ", )");
                        text = text.Replace("#IW", "0");
                        text = text.Replace("#IH", "0");
                        text = text.Replace("#ARRAY_SIZE", "" + shape);
                        text = text.Replace("#MODELNAME", modelName);
                        text = text.Replace("#OUTPUT_LOG", outputLogName);
                        text = text.Replace("#TIMESTEPS", ""+timeSteps);
                        text = text.Replace("#INPUT_PORT", ""+inputPort);
                        text = text.Replace("#OUTPUT_PORT", ""+outputPort);
                        if (useLSTM) {
                            text = text.Replace("#PPO2POLICY", "MlpLstmPolicy");
                        } else {
                            text = text.Replace("#PPO2POLICY", "MlpPolicy");
                        }
                    } else 
                    {
                        Debug.LogWarning("BaselinePPO2Template error: there is no enough information to generate agent program!!!");
                    }
                } else { //only image input
                    string code;
                    int[] shape;
                    int numObjects = 0;
                    if (sensorsExtractor.ComputingImageInput(agent, out code, out shape, out numObjects))
                    { 
                        string neuralCode = System.IO.File.ReadAllText("Assets/baseline/baselineppo2_imageInput.tpl");
                        text = head + neuralCode + text;                        
                        text = text.Replace("#IW", "" + shape[0]);
                        text = text.Replace("#IH", "" + shape[1]);
                        text = text.Replace("#TPL_RETURN_STATE", "return [None, " +  code + "]");
                        text = text.Replace("#TPL_INPUT_SHAPE", "(" + shape[0] + ", " + shape[1] + ", " + shape[2] + ")");
                        text = text.Replace("#NUMOBJ", "" + numObjects);
                        text = text.Replace("#LINEARINPUTNORM", "" + linearInputNorm);
                        text = text.Replace("#MODELNAME", modelName);
                        text = text.Replace("#OUTPUT_LOG", outputLogName);
                        text = text.Replace("#TIMESTEPS", ""+timeSteps);
                        text = text.Replace("#INPUT_PORT", ""+inputPort);
                        text = text.Replace("#OUTPUT_PORT", ""+outputPort);
                        if (useLSTM) {
                            text = text.Replace("#PPO2POLICY", "MlpLstmPolicy");
                        } else {
                            text = text.Replace("#PPO2POLICY", "MlpPolicy");
                        }
                    } else 
                    {
                        Debug.LogWarning("BaselinePPO2 error: there is no enough information to generate agent program!!!");
                    }
                }
            } else if (dim == 2) { //two inputs: image, and linear array.
                string code1;
                int shape1;
                string code2;
                int[] shape2;
                int numObjects;
                if (sensorsExtractor.ComputingLinearInput(agent, out code1, out shape1) && 
                        sensorsExtractor.ComputingImageInput(agent, out code2, out shape2, out numObjects) )
                {
                    Debug.LogWarning("BaselinePPO2 warning: BaselinePPO2 does not yet support multiple entries. You will have to adapt the generated code to support this functionality. See more at https://github.com/hill-a/stable-baselines/issues/133");
                
                    string neuralCode = System.IO.File.ReadAllText("Assets/baseline/baselineppo2_linearImageInput.tpl");
                    text = head + neuralCode + text;    
                    text = text.Replace("#IW", "" + shape2[0]);
                    text = text.Replace("#IH", "" + shape2[1]);
                    text = text.Replace("#TPL_RETURN_STATE", "return [" + code1  + ", " + code2 + "]");
                    text = text.Replace("#ARRAY_SIZE", "" + shape1);
                    text = text.Replace("#TPL_INPUT_SHAPE", "(" + shape2[0] + ", " + shape2[1] + ", " + shape2[2] + ")");
                    text = text.Replace("#DISABLE51", "");
                    text = text.Replace("#DISABLE52", "");
                    text = text.Replace("#RAYCASTING1", "");
                    text = text.Replace("#RAYCASTING2", "");
                    text = text.Replace("#HISTSIZE", "" + shape2[2]);
                    text = text.Replace("#SHAPE1", "" + shape2[0]);
                    text = text.Replace("#SHAPE2", "" + shape2[1]);
                    text = text.Replace("#NETWORK", "");
                    text = text.Replace("#NUMOBJ", "" + numObjects);
                    text = text.Replace("#LINEARINPUTNORM", "" + linearInputNorm);
                    text = text.Replace("#MODELNAME", modelName);
                    text = text.Replace("#OUTPUT_LOG", outputLogName);
                    text = text.Replace("#TIMESTEPS", ""+timeSteps);
                    text = text.Replace("#INPUT_PORT", ""+inputPort);
                    text = text.Replace("#OUTPUT_PORT", ""+outputPort);
                    if (useLSTM) {
                        text = text.Replace("#PPO2POLICY", "MlpLstmPolicy");
                    } else {
                        text = text.Replace("#PPO2POLICY", "MlpPolicy");
                    }
                } else 
                {
                    Debug.LogWarning("BaselinePPO2 error: there is no enough information to generate agent program!!!");
                }
            } else {
                text = text.Replace("#IW", "0");
                text = text.Replace("#IH", "0");
            }
            if (outputs.Count > 0){
                text = text.Replace("#TPL_OUTPUT_SHAPE", "(" + outputs[0].size + ", )");
            }
            if (gen.actions.Length > 0){
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("[");
                for (int i = 0; i < gen.actions.Length; i++) {
                    sb.Append("('").Append(gen.actions[i].name).Append("',").Append(gen.actions[i].value).Append(")");
                    if (i+1 < gen.actions.Length) {
                        sb.Append(',');
                    }
                }
                sb.Append("]");
                text = text.Replace("#TPL_ACTIONS", sb.ToString());
            } else {
                text = text.Replace("#TPL_ACTIONS", "[('Move', [1, 0, 0]), ('Move', [-1, 0, 0]), ('Move', [0, 1, 0]), ('Move', [0, -1, 0]), ('Move', [0, 0, 1]),  ('Move', [0, 0, -1])]");
                text = text.Replace("#TPL_OUTPUT_SHAPE", "(6, )");
            }
            text = text.Replace("#SKIP_FRAMES", "" + gen.skipFrames);
            for (int i = 0; i < replacement.Length; i++) {
                text = text.Replace(replacement[i].name, replacement[i].value);
            }
            text += "\n";
            return text;
        }
    }
}
