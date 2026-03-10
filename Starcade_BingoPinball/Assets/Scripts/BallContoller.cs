using UnityEngine;
using System.Collections;

public class BallContoller : MonoBehaviour
{
    public float debugKickForce;
    public float maxSpeed;
    public AudioSource appearSound;
    public AudioSource[] flightSounds;
    public AudioSource[] collisionSounds;
    public AudioSource[] flipperCollisionSounds;
    public AudioSource[] tracksCollisionSounds;
    public AudioSource[] microCollisionSounds;

    private Rigidbody rb;
    private DontGoThroughThings dngtt;
    private TrailRenderer trailRenderer;

    private bool pressed = false;
    private Vector3 defaultPosition;
    private float prevVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        dngtt = GetComponent<DontGoThroughThings>();
        defaultPosition = transform.position;
        trailRenderer = GetComponent<TrailRenderer>();
        prevVelocity = rb.velocity.magnitude;
    }

    void Update()
    {
        if (Game.build == Build.Release)
            return;

        if (Input.GetButtonDown("Ball Kick") && Game.State.CanPlay())
        {
            pressed = true;
        }
    }

    void FixedUpdate()
    {
        if (pressed)
        {
            rb.AddForce(Vector3.forward * debugKickForce, ForceMode.VelocityChange);
            pressed = false;
        }

        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }

        
        float speedDiff = rb.velocity.magnitude - prevVelocity;
        prevVelocity = rb.velocity.magnitude;
        for (int i = 0; i < flightSounds.Length; i++)
        {
            if (flightSounds[i].isPlaying)
            {
                return;
            }
        }
        if (speedDiff < 10)
        {
            return;
        }
        int index = (int)(speedDiff / 80) % 5;
        flightSounds[index].Play();
    }

    void OnCollisionEnter(Collision collision)
    {
        float impulse = collision.impulse.magnitude;
        const float low = 0.5f;
        const float high = 2f;
        if (impulse < low)
        {
            return;
        }
        else if (collision.collider.tag == "Tracks")
        {
            tracksCollisionSounds[Random.Range(0, tracksCollisionSounds.Length)].Play();
        }
        else if (collision.collider.tag == "Flipper")
        {
            flipperCollisionSounds[Random.Range(0, flipperCollisionSounds.Length)].Play();
        }
        if (impulse < high)
        {
            microCollisionSounds[Random.Range(0, microCollisionSounds.Length)].Play();
        }
        else if (impulse > high)
        {
            collisionSounds[Random.Range(0, collisionSounds.Length)].Play();
        }
    }

    public void Reset()
    {
        dngtt.enabled = false;
        transform.position = defaultPosition;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        dngtt.Start();
        dngtt.enabled = true;
        trailRenderer.enabled = true;

        appearSound.Play();
    }
}
