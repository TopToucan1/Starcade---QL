using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour
{
    private Animator anim;
    private AudioSource sound;

    void Start()
    {
        anim = GetComponent<Animator>();
        sound = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            sound.Play();
            anim.SetBool("IsClosed", true);
        }
    }
}
