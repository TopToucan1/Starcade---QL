using UnityEngine;
using System.Collections;

public class Accelerator : MonoBehaviour
{
    public float force;
    public float accelerationDelay;

    private float nextAcceleration;

    void Start()
    {

    }
    
    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        if (Time.time < nextAcceleration)
        {
            return;
        }
        if (!other.CompareTag("Ball"))
        {
            return;
        }

        Rigidbody rb = other.GetComponent<Rigidbody>();
        Vector3 direction = Vector3.Normalize(rb.velocity);
        rb.AddForce(direction * force);
        nextAcceleration = Time.time + accelerationDelay;
    }
}
