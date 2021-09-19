using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u.ext
{
	[RequireComponent(typeof(Animator))]
    public class AnimationGroundStatus : Sensor
    {
        public GameObject target;

        public override bool GetBoolValue()
        {
            Animator anim = target.GetComponent<Animator>();
            return anim.GetCurrentAnimatorStateInfo(0).IsName("Grounded");
        }

        public override float[] GetFloatArrayValue() {
            Animator anim = target.GetComponent<Animator>();
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Grounded")) {
                return new float[]{1.0f};
            } else {
                return new float[]{0.001f};
            }
        }
    }
}
