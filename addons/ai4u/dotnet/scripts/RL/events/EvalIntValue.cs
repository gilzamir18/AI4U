using System;
using System.Collections;
using System.Collections.Generic;
using Godot;
using ai4u;

namespace ai4u
{
    /// <summary>
    /// The 'EvalIntValue' represents an evaluation rewarded event.
    /// It contains fields as 'targetPath' and 'property'. The field 'targetPath'
    /// is the link to Node object to be evaluated. The 'property' field
    /// is the evaluated property from target object. The
    /// fields is 'minValue' and 'maxValue' the target range of the evaluated 'property'. If  'property' value is in range [minValue, maxValue], 
    /// than target evaluation is true; else, target evaluation is false.
    /// </summary>
    public partial class EvalIntValue : RewardFunc
    {
        /// <summary>
        /// Target object evaluated property.
        /// </summary>
        [Export]
        private string property;

        /// <summary>
        /// Minimum value of the reference range.
        /// </summary>
        [Export]
        private int minValue;

        /// <summary>
        /// Maximum value of the reference range.
        /// </summary>
        [Export]
        private int maxValue;

        /// <summary>
        /// Generated reward value if evaluation is true.
        /// </summary>
        [Export]
        private float reward = 1;

        /// <summary>
        /// Target object reference.
        /// </summary>
        [Export]
        private NodePath targetPath;

        private Node target;

        private float acmReward = 0;


        private RLAgent agent;

        public override void OnSetup(Agent agent)
        {
            this.agent = (RLAgent) agent;
            this.target = GetNode(targetPath);
            acmReward = 0;
        }

        public override void OnUpdate()
        {
            this.agent.AddReward(acmReward, this);
            acmReward = 0.0f;
        }

        public override bool Eval()
        {
            int v = int.Parse(target.GetType().GetProperty(property).GetValue(target).ToString(),
                System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            if (v >= minValue && v <= maxValue)
            {
                acmReward += reward;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void ResetEval()
        {

        }

        public override void OnReset(Agent agent)
        {
            acmReward = 0;
        }
    }
}
