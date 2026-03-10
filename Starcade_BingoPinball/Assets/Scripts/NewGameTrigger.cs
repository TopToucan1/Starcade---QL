using UnityEngine;
using System.Collections;

public class NewGameTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            Game.State.StartGame();
        }
    }
}