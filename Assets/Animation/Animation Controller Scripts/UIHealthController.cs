using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHealthController : MonoBehaviour
{
    [SerializeField]
    private Animator[] healthCristals;
    private states[] cristalStates;
    private UniversalHealthController playerHealthController;

    private void Awake()
    {
        playerHealthController = GameObject.FindGameObjectWithTag("Player").GetComponent<UniversalHealthController>();
        cristalStates = new states[healthCristals.Length];
        for(int i = 0; i < healthCristals.Length; i++)
            cristalStates[i] = states.gain;
        
    }

    private string[] animations =
    {
        "health gain placeholder",
        "health loss placeholder"
    };

    private enum states
    {
        gain,
        loss
    }

    public void respawn()
    {
        cristalStates = new states[healthCristals.Length];
        for (int i = 0; i < healthCristals.Length; i++)
        {
            animate(healthCristals[i], returnStateAnimation(states.gain));
            cristalStates[i] = states.gain;
        }
    }

    private void animate(Animator animator, string state)
    {
        if (state == null) return;

        animator.Play(state);
    }

    private string returnStateAnimation(states state) { return animations[(int)state]; }


    private void Update()
    {
        for(int i = 0; i < healthCristals.Length; i++)
        {
            if (i < playerHealthController.currentLife - 1 && cristalStates[i] == states.loss)
            {
                animate(healthCristals[i], returnStateAnimation(states.gain));
                cristalStates[i] = states.gain;
            }
            else if (i > playerHealthController.currentLife - 1 && cristalStates[i] == states.gain)
            {
                animate(healthCristals[i], returnStateAnimation(states.loss));
                cristalStates[i] = states.loss;
            }
            
        }

    }

}
