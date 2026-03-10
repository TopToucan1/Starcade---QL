using UnityEngine;
using System.Collections;

public class CloseOnLeave : MonoBehaviour
{
    public Animator parentAnimator;
    public Side side;

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            parentAnimator.SetBool("IsClosed", true);
            Game.State.CloseBottomDoor(side);
        }
    }
}
