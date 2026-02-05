using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAnimationController : MonoBehaviour
{
    [SerializeField]
    private Animator[] animators;

    private enum animatorIDs
    {
        ScreenTransition
    }

    private enum states
    {
        Idle,
        TransitionIn,
        TransitionOut
    }

    private string[] animations =
    {
        "black screen",
        "screen transition in placeholder",
        "screen transition out placeholder"
    };

    private void animate(Animator animator, string state)
    {
        if (state == null) return;
        
        animator.Play(state);
    }

    private string returnStateAnimation(states state) { return animations[(int)state]; }
    private Animator returnAnimatorID(animatorIDs ID) { return animators[(int)ID]; }

    public void TransitionIn() { animate(returnAnimatorID(animatorIDs.ScreenTransition), returnStateAnimation(states.TransitionIn)); }
    public void TransitionOut() { animate(returnAnimatorID(animatorIDs.ScreenTransition), returnStateAnimation(states.TransitionOut)); }
    public void BlackScreen() { animate(returnAnimatorID(animatorIDs.ScreenTransition), returnStateAnimation(states.Idle)); }
}
