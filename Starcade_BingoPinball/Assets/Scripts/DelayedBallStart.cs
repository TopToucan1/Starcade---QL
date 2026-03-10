using UnityEngine;
using System.Collections;

public class DelayedBallStart : MonoBehaviour
{
    public float force;
    public float waitTime;
    public Animator lightAnimator;

    private AudioSource sound;

    void Start()
    {
        sound = GetComponent<AudioSource>();
    }

    IEnumerator OnTriggerEnter(Collider other)
    {
        sound.Play();
        lightAnimator.SetBool("Blink", true);
        if (other.gameObject.CompareTag("Ball"))
        {
            yield return new WaitForSeconds(waitTime);
            lightAnimator.SetBool("Blink", false);
            other.gameObject.GetComponent<Rigidbody>().AddForce(Vector3.forward * force);
        }
    }
}
