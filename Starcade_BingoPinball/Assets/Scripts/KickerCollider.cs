using UnityEngine;
using System.Collections;
using System;

public class KickerCollider : MonoBehaviour
{
    public float force;
    public float collisionDelay;
    public Animator animator;

    private float nextCollision = 0f;
    private AudioSource sound;

    void Start()
    {
        sound = GetComponent<AudioSource>();
        GameState.OnNewBall += OnNewBall;
        GameState.OnBallLoss += OnBallLoss;
        PlungerController.OnPlungeStart += OnPlungerStart;
        PlungerController.OnPlungeEnd += OnPlungerEnd;
    }

    void Update()
    {
        if (!Game.State.IsPlaying && animator != null)
        {
            animator.SetBool("Enabled", true);
        }
    }

    IEnumerator OnCollisionEnter(Collision collision)
    {
        if (Time.time < nextCollision)
        {
            yield return null;
        }
        collision.rigidbody.AddForce(-collision.contacts[0].normal * force);
        nextCollision = Time.time + collisionDelay;
        if (sound != null)
        {
            sound.Play();
        }
        if (animator != null)
        {
            animator.SetTrigger("Start");
        }
    }

    private void OnPlungerStart()
    {
        if (Game.State.IsPlaying)
        {
            return;
        }
        if (animator != null)
        {
            animator.SetBool("Plunge", true);
        }
    }

    private void OnPlungerEnd()
    {
        if (Game.State.IsPlaying)
        {
            return;
        }
        if (animator != null)
        {
            animator.SetBool("Plunge", false);
        }
    }

    private void OnNewBall()
    {
        if (animator != null)
        {
            animator.SetBool("Enabled", false);
            animator.SetBool("Plunge", false);
        }
    }

    private void OnBallLoss()
    {
        if (animator != null)
        {
            animator.SetBool("Enabled", true);
        }
    }
}
