using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Actions
{
    Enable,
    Disable
}

public class Highlighter : MonoBehaviour
{
    public Transform[] activeTriggers;
    public Actions plungeAction;

    private Animator animator;

    void Start()
    {
        BingoNumberTrigger.OnNumberHit += Highlight;

        animator = GetComponent<Animator>();

        PlungerController.OnPlungeStart += OnPlungeStart;
        PlungerController.OnPlungeEnd += OnPlungeEnd;
        GameState.OnNewBall += OnNewBall;
    }
    
    private void Highlight(Transform t)
    {
        foreach (var tr in activeTriggers)
        {
            if (tr == t)
            {
                animator.SetTrigger("Start");
                return;
            }
        }
    }

    private void OnPlungeStart()
    {
        if (Game.State.IsPlaying || !IsParameterExists("Enabled"))
        {
            return;
        }
        animator.SetBool("Enabled", plungeAction == Actions.Enable);
    }

    private void OnPlungeEnd()
    {
        if (Game.State.IsPlaying || !IsParameterExists("Enabled"))
        {
            return;
        }
        animator.SetBool("Enabled", plungeAction == Actions.Disable);
    }

    private void OnNewBall()
    {
        if (!IsParameterExists("Enabled"))
        {
            return;
        }
        animator.SetBool("Enabled", plungeAction == Actions.Disable);
    }

    private bool IsParameterExists(string param)
    {
        foreach (var p in animator.parameters)
        {
            if (p.name == param)
            {
                return true;
            }
        }
        return false;
    }
}
