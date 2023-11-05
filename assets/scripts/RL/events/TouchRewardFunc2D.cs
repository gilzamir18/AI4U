using Godot;
using System;
using ai4u;
namespace ai4u
{
    public partial class TouchRewardFunc2D : RewardFunc
    {
        [Export]
        public float reward = 0.0f;
        [Export]
        public NodePath targetPath;
        private Node target;
        private float acmReward = 0.0f;
        private BasicAgent agent;
        private bool configured = false;
        private bool eval = false;

        public override void OnSetup(Agent agent)
        {
            if (!configured)
            {
                configured = true;
                agent.AddResetListener(this);
                this.agent = (BasicAgent)agent;
                target = GetNode(targetPath);
                RigidBody2D body = this.agent.GetAvatarBody() as RigidBody2D;
                body.BodyEntered += OnEntered;
            }
        }

        public void OnEntered(Node body)
        {
            if (body == target)
            {
                eval = true;
                acmReward += this.reward;
            }
        }

        public override bool Eval()
        {
            return eval;
        }

        public override void OnUpdate()
        {
            this.agent.AddReward(acmReward, this);
            acmReward = 0.0f;
        }

        public override void ResetEval()
        {
            eval = false;
        }

        public override void OnReset(Agent agent)
        {
            eval = false;
            acmReward = 0.0f;
        }
    }
}

