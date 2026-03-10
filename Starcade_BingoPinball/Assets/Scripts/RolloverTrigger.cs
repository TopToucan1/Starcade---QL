using UnityEngine;
using System.Collections;

public class RolloverTrigger : MonoBehaviour
{
    public int id;      // Number of rollover

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            Game.State.SetTopRollover(id);
        }
    }
}