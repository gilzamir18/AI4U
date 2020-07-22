using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ai4u;

namespace ai4u.ext {
    public class EnvironmentGenerator : MonoBehaviour
    {

        public Agent[] agents;

        public int[] stateShape;
        public bool stateSpaceIsInteger = false;
        public int actionShape;
        public bool actionSpaceIsContinue = false;
        public bool useBuiltInA3CAsTrainer = true;
        public bool useStableBaselineAsTrainer = false;
    }

}