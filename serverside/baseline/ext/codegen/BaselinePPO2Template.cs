using System.Collections;
using System.Collections.Generic; 
using UnityEngine;
using ai4u;

namespace ai4u.ext {
    public class BaselinePPO2Template: EnvironmentTemplate
    {
        public override string GenCode(EnvironmentGenerator gen, Agent agent)
        {
            string text = System.IO.File.ReadAllText("Assets/baseline/baselineppo2.tpl");

            int dim = inputs.Count;


            if (dim == 1) {
                if (inputs[0].shape.Length == 1) {
                    text = text.Replace("#TPL_RETURN_STATE", "return np.array(fields['" + inputs[0].name + "'])");
                    text = text.Replace("#TPL_INPUT_SHAPE", "(" + inputs[0].shape[0] + ", )");
                    text = text.Replace("#IW", "0");
                    text = text.Replace("#IH", "0");
                } else {
                    text = text.Replace("#IW", "" + inputs[0].shape[0]);
                    text = text.Replace("#IH", "" + inputs[0].shape[1]);
                    text = text.Replace("#TPL_RETURN_STATE", "return to_image(fields[" + inputs[0].name + "])");
                    text = text.Replace("#TPL_INPUT_SHAPE", "(" + inputs[0].shape[0] + ", " + inputs[0].shape[1] + ", " + inputs[0].shape[2] + ")");
                    text = text.Replace("#DISABLE52", "");
                }
            } else if (dim == 2) {

                ModelInput arrayInput, imageInput;
                if (inputs[0].shape.Length == 1) {
                    arrayInput = inputs[0];
                    imageInput = inputs[1];
                } else {
                    arrayInput = inputs[0];
                    imageInput = inputs[1];
                }

                text = text.Replace("#IW", "" + inputs[0].shape[0]);
                text = text.Replace("#IH", "" + inputs[0].shape[1]);
                text = text.Replace("#TPL_RETURN_STATE", "return [" + "np.array(fields['" + arrayInput.name + "'])"  + "," + "to_image(fields[" + imageInput.name + "])]");
                text = text.Replace("#ARRAY_SIZE", "" + arrayInput.shape[0]);
                text = text.Replace("#TPL_INPUT_SHAPE", "(" + imageInput.shape[0] + ", " + imageInput.shape[1] + ", " + imageInput.shape[2] + ")");
                text = text.Replace("#DISABLE52", "");
            } else {
                text = text.Replace("#IW", "0");
                text = text.Replace("#IH", "0");
            }
            if (outputs.Count > 0){
                text = text.Replace("#TPL_OUTPUT_SHAPE", "(" + outputs[0].size + ", )");
            }
            if (gen.actionName.Length >= gen.actionValue.Length && gen.actionValue.Length > 0){
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("[");
                for (int i = 0; i < gen.actionValue.Length; i++) {
                    sb.Append("('").Append(gen.actionName[i]).Append("',").Append(gen.actionValue[i]).Append(")");
                    if (i+1 < gen.actionValue.Length) {
                        sb.Append(',');
                    }
                }
                sb.Append("]");
                text = text.Replace("#TPL_ACTIONS", sb.ToString());
            } else {
                text = text.Replace("#TPL_ACTIONS", "[('x', 1), ('x', -1), ('y', 1), ('y', -1), ('z', 1),  ('z', -1)]");
                text = text.Replace("#TPL_OUTPUT_SHAPE", "(6, )");
            }
            text = text.Replace("#SKIP_FRAMES", "" + gen.skipFrames);
            for (int i = 0; i < replacesNames.Length; i++) {
                text = text.Replace(replacesNames[i], replacesValues[i]);
            }
            text += "\n";
            return text;
        }
    }
}
