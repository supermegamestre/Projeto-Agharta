using System;
using System.Collections.Generic;
using UnityEngine;
namespace SmallUtilities
{
    public class AnimatorController
    {
        private List<string> animations = new List<string>();
        private Animator animator;
        private string dummy;

        private int state = 0;

        public AnimatorController(Animator animator, IEnumerable<string> animations, string dummy = null)
        {
            this.animator = animator;
            this.animations.AddRange(animations);
            this.dummy = dummy;
        }

        public AnimatorController(Animator animator, string animation, string dummy = null)
        {
            this.animator = animator;
            animations.Add(animation);
            this.dummy = dummy;
        }

        public AnimatorController(Animator animator) => this.animator = animator;

        public void animate(int state, bool ignoreStateCheck = false)
        {
            if(animations.Count ==  0) throw new ArgumentNullException("you didn't feed this controller an animation list, you idiot");

            if (this.state == state && !ignoreStateCheck) return;
            this.state = state;
            animator.Play(animations[state]);
        }
        public void animate(string animation) => animator.Play(animation);
        public void animate()
        {
            if (animations.Count == 0) throw new ArgumentNullException("you didn't feed this controller ANY animation, you idiot");
            
            if (animations.Count == 1)
                animator.Play(animations[0]);
            else
                throw new ArgumentException("which animation? you gave this controller a list, not a single animation you idiot");
        }

        public List<string> getAnimations() => animations.Count > 0 ? animations : throw new ArgumentNullException("this controler has no animation list");
        
        public string getLastPlayedAnimation() => animations.Count > 0 ? animations[state] : throw new ArgumentNullException("this controler has no animation list, it is not possible to retreive the last played animation");
        
        public string getCurrentAnimation() => animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        
        public int getCurrentState() => animations.Count > 0 ? state : throw new ArgumentNullException("this controler has no animation list, there is no current state");

        public bool isOnTheDummy() => dummy != null ? animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == dummy : throw new ArgumentNullException("this controller has no dummy");
    }

}