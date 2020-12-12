using System.Collections;
using System.Collections.Generic; 
using UnityEngine;
using ai4u;
using System.Text;

namespace ai4u.ext {

    public class SensorsExtractor 
    {
        public bool ComputingLinearInput(DPRLAgent agent, out string code, out int size)
        {
            StringBuilder list1 = new StringBuilder();
            StringBuilder list2 = new StringBuilder();
            list1.Append("[");
            int countL1 = 0;
            int countL2 = 0;
            int i = 0;
            size = 0;
            foreach(Sensor s in agent.sensors) 
            {
                if (s.isState)
                {
                    if (s.type == SensorType.sfloat)
                    {
                        size ++;
                        if (i > 0)
                        {
                            list1.Append(",");
                        }
                        list1.Append("fields['" + s.perceptionKey + "']");
                        countL1 ++;
                    } else if (s.type == SensorType.sfloatarray) {
                        size += s.shape[0];
                        if (i > 0) 
                        {
                            list2.Append("+");
                        }
                        list2.Append("fields['" + s.perceptionKey + "']");
                        countL2 ++;
                    }
                }
                i++;
            }
            list1.Append(']');
            string gen =  "";
            if (countL1 > 0) {
                gen += list1.ToString();
                if (countL2 > 0) {
                    gen += "+" + list2.ToString();
                }
                code = gen;
                return true;
            } else if (countL2 > 0) {
                gen = list2.ToString();           
                code = gen;
                return true;
            }
            code = string.Empty;
            size = 0;
            return false;
        }

        public bool ComputingImageInput(DPRLAgent agent, out string code, out int[] shape, out int numObjects)
        {
            Sensor sensor;
            if (agent.TryGetSensor("raycasting", out sensor) )
            {
                RaycastingSensor rsensor = (RaycastingSensor) sensor;
                code = "to_image(fields['" + rsensor.perceptionKey + "'])";
                shape = new int[]{rsensor.shape[0], rsensor.shape[1], rsensor.shape[2]};
                numObjects = rsensor.objectMapping.Length;
                return true;
            }
            code = "";
            shape = null;
            numObjects = 0;
            return false;
        }
    }

    public class BuiltInA3CTemplate: EnvironmentTemplate
    {

        private SensorsExtractor sensorsExtractor = new SensorsExtractor();

        public override string GenCode(EnvironmentGenerator gen, DPRLAgent agent)
        {
            string text = System.IO.File.ReadAllText("Assets/baseline/builtina3c.tpl");

            int dim = inputs.Count;

            if (dim == 1) {
                if (!inputs[0].isImage) {
                    string code;
                    int shape = 0;
                    if (sensorsExtractor.ComputingLinearInput(agent, out code, out shape)) 
                    {
                        text = text.Replace("#DISABLE51", "");
                        text = text.Replace("#TPL_RETURN_STATE", "return np.array(" + code + ")");
                        text = text.Replace("#TPL_INPUT_SHAPE", "(" + shape + ", )");
                        text = text.Replace("#IW", "0");
                        text = text.Replace("#IH", "0");
                    }
                } else {
                    string code;
                    int[] shape;
                    int numObjects = 0;
                    if (sensorsExtractor.ComputingImageInput(agent, out code, out shape, out numObjects))
                    { 
                        text = text.Replace("#IW", "" + shape[0]);
                        text = text.Replace("#IH", "" + shape[1]);
                        text = text.Replace("#TPL_RETURN_STATE", "return " +  code);
                        text = text.Replace("#TPL_INPUT_SHAPE", "(" + shape[0] + ", " + shape[1] + ", " + shape[2] + ")");
                        text = text.Replace("#DISABLE51", "");
                        text = text.Replace("#RAYCASTING1", "");
                        text = text.Replace("#RAYCASTING2", "");
                        text = text.Replace("#RAYCASTING3", "");
                        text = text.Replace("#RAYCASTING4", "");
                        text = text.Replace("#RAYCASTING5", "");
                        text = text.Replace("#HISTSIZE", "" + shape[2]);
                        text = text.Replace("#SHAPE1", "" + shape[0]);
                        text = text.Replace("#SHAPE2", "" + shape[1]);
                        text = text.Replace("#NETWORK", "");
                        text = text.Replace("#NUMOBJ", "" + numObjects);
                    }
                }
            } else if (dim == 2) {
                string code1;
                int shape1;
                string code2;
                int[] shape2;
                int numObjects;
                if (sensorsExtractor.ComputingLinearInput(agent, out code1, out shape1) && 
                        sensorsExtractor.ComputingImageInput(agent, out code2, out shape2, out numObjects) )
                {
                    text = text.Replace("#IW", "" + shape2[0]);
                    text = text.Replace("#IH", "" + shape2[1]);
                    text = text.Replace("#TPL_RETURN_STATE", "return [" + code1  + ", " + code2 + "]");
                    text = text.Replace("#ARRAY_SIZE", "" + shape1);
                    text = text.Replace("#TPL_INPUT_SHAPE", "(" + shape2[0] + ", " + shape2[1] + ", " + shape2[2] + ")");
                    text = text.Replace("#DISABLE51", "");
                    text = text.Replace("#DISABLE52", "");
                    text = text.Replace("#RAYCASTING1", "");
                    text = text.Replace("#RAYCASTING2", "");
                    text = text.Replace("#RAYCASTING3", "");
                    text = text.Replace("#RAYCASTING4", "");
                    text = text.Replace("#RAYCASTING5", "");
                    text = text.Replace("#HISTSIZE", "" + shape2[2]);
                    text = text.Replace("#SHAPE1", "" + shape2[0]);
                    text = text.Replace("#SHAPE2", "" + shape2[1]);
                    text = text.Replace("#NETWORK", "");
                    text = text.Replace("#NUMOBJ", "" + numObjects);
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
