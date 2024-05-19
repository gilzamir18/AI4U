using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ai4u
{
    public partial class CollisionWithRewardFunc : RewardFunc
    {
        [Export]
        public float reward = -0.1f;
        [Export]
        public string group = "";
        private float acmReward = 0.0f;
        private BasicAgent agent;
        private bool configured = false;

        public bool stayInCollision = false;

        public override void OnSetup(Agent agent)
        {
            if (!configured)
            {
                configured = true;
                agent.AddResetListener(this);
                this.agent = (BasicAgent)agent;
                RigidBody3D body = this.agent.GetAvatarBody() as RigidBody3D;
                body.BodyEntered += OnEntered;
                body.BodyExited += OnExited;
            }
        }

        private void OnExited(Node body)
        {
            var groups = body.GetGroups();
            if (group == "" || groups.Contains(group))
            {
                stayInCollision = false;
            }
        }

        private void OnEntered(Node body)
        {
            var groups = body.GetGroups();
            if (group == "" || groups.Contains(group))
            {
                stayInCollision = true;
            }
        }

        public override void OnUpdate()
        {
            this.agent.AddReward(acmReward, this);
            acmReward = 0.0f;
        }

        public override void OnReset(Agent agent)
        {
            acmReward = 0.0f;
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);
            if (stayInCollision)
            {
                acmReward += this.reward;
            }
        }
    }
}
