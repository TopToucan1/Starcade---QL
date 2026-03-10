using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BumperCollider : MonoBehaviour
{
    public float explosionForce;
    public float explosionRadius;
    public float collisionDelay;
    public float dampingTime;
    public float maxEmission;
    public float maxLightIntensity;

    private float nextCollision = 0f;

    private AudioSource[] sounds;
    private Animator animator;

    void Start()
    {
        sounds = GetComponents<AudioSource>();
        animator = GetComponentInParent<Animator>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (Time.time < nextCollision)
        {
            return;
        }
        // Make position a bit random for excluding ball hangs
        float range = 0.01f;
        Vector3 modifier = new Vector3(Random.Range(-range, range), Random.Range(-range, range), Random.Range(-range, range));
        collision.rigidbody.AddExplosionForce(explosionForce, transform.position + modifier, explosionRadius);
        nextCollision = Time.time + collisionDelay;
        
        animator.SetTrigger("Start");

        int soundIndex = Random.Range(0, sounds.Length);
        sounds[soundIndex].Play();
    }
}
