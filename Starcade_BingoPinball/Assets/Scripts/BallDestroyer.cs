using UnityEngine;
using System.Collections;

public class BallDestroyer : MonoBehaviour
{
    public float ballResetTimeout;
    public TrailRenderer ballTrailRenderer;

    private bool started;
    private AudioSource lossSound;

    void Start()
    {
        started = false;
        lossSound = GetComponent<AudioSource>();
    }

    IEnumerator OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball") && !started)
        {
            lossSound.Play();
            if (Game.State.IsProgressiveWon && Game.State.IsJackpotHit())
            {
                RandomJackpot.Instance.EmitJp();
            }
            else
            {
                started = true;
                ballTrailRenderer.enabled = false;

                yield return new WaitForSeconds(ballResetTimeout);

                Game.State.BallLoss();
                started = false;
            }
        }
    }
}
