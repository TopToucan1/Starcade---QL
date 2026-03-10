using UnityEngine;
using System.Collections;

public class GravityPoint : MonoBehaviour
{
    public float force;
    public float innerRadius;

    private GameObject ball;
    private Rigidbody ballRigidbody;
    private float radius;
    private bool isEnabled;

    void Start()
    {
        ball = GameObject.FindGameObjectWithTag("Ball");
        ballRigidbody = ball.GetComponent<Rigidbody>();
        radius = GetComponent<SphereCollider>().radius * transform.lossyScale.x + ball.GetComponent<SphereCollider>().radius * ball.transform.lossyScale.x;
    }

    void OnTriggerEnter(Collider other)
    {
        isEnabled = true;
    }

    void OnTriggerStay(Collider other)
    {
        if (Game.State.IsProgressiveWon && other.gameObject.CompareTag("Ball") && isEnabled)
        {
            float distance = Vector3.Distance(transform.position, ball.transform.position);
            if (distance < innerRadius)
            {
                ballRigidbody.constraints = RigidbodyConstraints.None;
                ballRigidbody.velocity = Vector3.zero;
                isEnabled = false;
                RandomJackpot.Instance.EmitJp();
            }
            else
            {
                Vector3 direction = transform.position - ball.transform.position;
                ballRigidbody.AddForce(direction.normalized * force * (1 - distance * distance / (radius * radius)));
            }
        }
    }
}
