using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Hand : MonoBehaviour
{
    Animator animator;
    private float gripTarget;
    private float triggerTarget;
    private float gripCurrent;
    private float triggerCurrent;
    private string animatorGripParam = "Grip";
    private string animatorTriggerParam = "Trigger";
    
    public float speed;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        AnimateHand();
    }

    internal void SetGrip(float v)
    {
        gripTarget = v;
    }

    internal void SetTrigger(float v)
    {
        triggerTarget = v;
    }

    void AnimateHand()
    {
        if (gripTarget != gripCurrent)
        {
            gripCurrent = Mathf.Lerp(gripCurrent, gripTarget, Time.deltaTime * speed);
            //currentGrip = Mathf.MoveTowards(currentGrip, gripTarget, Time.deltaTime * speed);
            animator.SetFloat(animatorGripParam, gripCurrent);
        }
        if (triggerTarget != triggerCurrent)
        {
            triggerCurrent = Mathf.Lerp(triggerCurrent, triggerTarget, Time.deltaTime * speed);
            //currentTrigger = Mathf.MoveTowards(currentTrigger, triggerTarget, Time.deltaTime * speed);
            animator.SetFloat(animatorTriggerParam, triggerCurrent);
        }
    }
}
