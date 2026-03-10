using UnityEngine;
using System.Collections;

public class BottomDoorController : MonoBehaviour
{
    public Side side;
    public Animator animator;

    private AudioSource sound;

    void Awake()
    {
        GameState.OnBottomDoorChange += OnBottomDoorChange;
        sound = GetComponent<AudioSource>();
    }

    void OnDestroy()
    {
        GameState.OnBottomDoorChange -= OnBottomDoorChange;
    }

    private void OnBottomDoorChange()
    {
        var bd = Game.State.BottomDoorsOpened;
        if (bd[side])
        {
            animator.ResetTrigger("IsClosed");
        }
        else
        {
            animator.SetTrigger("IsClosed");
        }
    }

    public void PlaySound()
    {
        sound.Play();
    }
}
