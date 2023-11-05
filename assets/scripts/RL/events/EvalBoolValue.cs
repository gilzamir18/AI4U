using System;
using System.Collections;
using System.Collections.Generic;
using Godot;
using ai4u;

namespace ai4u
{
    /// <summary>
    /// The 'EvalBoolValue' represents an evaluation rewarded event.
    /// It contains fields as 'targetPath' and 'property'. The field 'targetPath'
    /// is the link to Node object to be evaluated. The 'property' field
    /// is the evaluated property from target object. Another important
    /// field is 'value'.  If 'value' and 'property' has the same value, 
    /// than target evaluation is true; else target evaluation is false.
    /// </summary>
    public partial class EvalBoolValue : RewardFunc
    {
        /// <summary>
        /// Target object evaluated property.
        /// </summary>
        [Export]
        private string property;

        /// <summary>
        /// Property reference value.
        /// </summary>
        [Export]
        private bool value;

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

        
        private BasicAgent agent;

        public override void OnSetup(Agent agent)
        {
            this.agent = (BasicAgent) agent;
            target = GetNode(targetPath);
            acmReward = 0;
        }

        public override void OnUpdate()
        {
            this.agent.AddReward(acmReward, this);
            acmReward = 0.0f;
        }

        public override bool Eval()
        {
            bool v = bool.Parse(target.GetType().GetProperty(property).GetValue(target).ToString());
            if (v == value)
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
