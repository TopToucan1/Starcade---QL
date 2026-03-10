using UnityEngine;
using System.Collections;

public class BumperLighter : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        GameState.OnNewBall += OnNewBall;
        GameState.OnBallLoss += OnBallLoss;
        PlungerController.OnPlungeStart += OnPlungeStart;
        PlungerController.OnPlungeEnd += OnPlungeEnd;
    }

    void Update()
    {
        if (!Game.State.IsPlaying)
        {
            animator.SetBool("Enabled", true);
        }
    }
    
    private void OnNewBall()
    {
        animator.SetBool("Enabled", false);
        animator.SetBool("Plunge", false);
    }

    private void OnBallLoss()
    {
        animator.SetBool("Enabled", true);
    }

    private void OnPlungeStart()
    {
        if (Game.State.IsPlaying)
        {
            return;
        }
        animator.SetBool("Plunge", true);
    }

    private void OnPlungeEnd()
    {
        if (Game.State.IsPlaying)
        {
            return;
        }
        animator.SetBool("Plunge", false);
    }
}
