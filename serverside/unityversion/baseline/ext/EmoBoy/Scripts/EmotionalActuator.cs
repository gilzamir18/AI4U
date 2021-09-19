using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ai4u.ext;

public class EmotionalActuator : Actuator
{
    private Animator animator;
    int facialExpressionAnimatorParameterHash;

    // Start is called before the first frame update
    void Start()
    {
        animator = agent.GetComponent<Animator>();
        facialExpressionAnimatorParameterHash = Animator.StringToHash("Facial Expression");     
    }

    public override void Act()
    {
        if (agent.GetActionName() == actionName) {
            float facialExpression = agent.GetActionArgAsFloatArray()[0];
             if (facialExpression > -1)
            {
                animator.SetFloat(facialExpressionAnimatorParameterHash, facialExpression);
            }
        }
    }
    

    public override void Reset()
    {

    }
}
