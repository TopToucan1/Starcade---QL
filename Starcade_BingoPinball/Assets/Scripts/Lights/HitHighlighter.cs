using UnityEngine;
using System.Collections;

public class HitHighlighter : MonoBehaviour
{
    public Animator animator;

    void Start()
    {
    }

    void OnCollisionEnter(Collision collision)
    {
        animator.SetTrigger("Start");
    }

    void OnCollisionStay(Collision collision)
    {
        animator.SetTrigger("Start");
    }
}
