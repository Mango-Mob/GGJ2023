using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetParamFromAnim : StateMachineBehaviour
{
    public enum ParamType { INT, FLOAT, BOOL, TRIG}
    public ParamType m_type;
    public string paramName;
    public float paramVal;

    public bool onExit = false;
    public bool onEnter = false;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(onEnter)
        {
            switch (m_type)
            {
                case ParamType.INT:
                    animator.SetInteger(paramName, Mathf.FloorToInt(paramVal));
                    break;
                case ParamType.FLOAT:
                    animator.SetFloat(paramName, paramVal);
                    break;
                case ParamType.BOOL:
                    animator.SetBool(paramName, Mathf.FloorToInt(paramVal) == 0);
                    break;
                case ParamType.TRIG:
                    animator.SetTrigger(paramName);
                    break;
                default:
                    break;
            }
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(onExit)
        {
            switch (m_type)
            {
                case ParamType.INT:
                    animator.SetInteger(paramName, Mathf.FloorToInt(paramVal));
                    break;
                case ParamType.FLOAT:
                    animator.SetFloat(paramName, paramVal);
                    break;
                case ParamType.BOOL:
                    animator.SetBool(paramName, Mathf.FloorToInt(paramVal) == 0);
                    break;
                case ParamType.TRIG:
                    animator.SetTrigger(paramName);
                    break;
                default:
                    break;
            }
        }
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
