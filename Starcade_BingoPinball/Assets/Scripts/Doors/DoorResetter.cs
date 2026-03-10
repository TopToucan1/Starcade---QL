using UnityEngine;
using System.Collections;

public class DoorResetter : MonoBehaviour
{
    public Animator animator;
    
    public void Reset()
    {
        animator.SetBool("IsClosed", false);
    }
}
