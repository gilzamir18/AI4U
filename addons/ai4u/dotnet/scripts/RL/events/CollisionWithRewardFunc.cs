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
        private RLAgent agent;
        private bool configured = false;

        public bool stayInCollision = false;

        private bool isCharacterBody = false;

        private CharacterBody3D chBody;

        public override void OnSetup(Agent agent)
        {
            if (!configured)
            {
                configured = true;
                agent.AddResetListener(this);
                this.agent = (RLAgent)agent;

                var body = this.agent.GetAvatarBody();
                if (body.GetType() == typeof(RigidBody3D))
                {
                    ((RigidBody3D)body).BodyEntered += OnEntered;
                    ((RigidBody3D)body).BodyExited += OnExited;
                    isCharacterBody = false;
                }
                else if (body.GetType() == typeof(CharacterBody3D))
                {
                    isCharacterBody = true;
                    chBody = (CharacterBody3D) body;
                }
                else
                {
                    throw new Exception("The type of the agent avatar is invalid!");
                }
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
            if (isCharacterBody)
            {
                var nc = chBody.GetSlideCollisionCount();
                for (int i = 0; i < nc; i++)
                {
                    var kc = chBody.GetSlideCollision(i);
    
                    var n = (Node)kc.GetCollider();
                    if (n.IsInGroup(group))
                    {
                        acmReward += this.reward;
                    }
                }
            }
         
            if (stayInCollision)
            {
                acmReward += this.reward;
            }
        }
    }
}
