using UnityEngine;
using System.Collections;

public class StartArrows : MonoBehaviour
{
    public PlungerController plunger;
    public float idleWait;
    public float minWait;

    private Animator[] anims;
    private float nextBlink;
    private int index;
    private bool plunge;

    void Start()
    {
        anims = GetComponentsInChildren<Animator>();
        index = 0;
        nextBlink = Time.time + idleWait;
        PlungerController.OnPlungeStart += OnPlungerStart;
        PlungerController.OnPlungeEnd += OnPlungerEnd;
    }

    void Update()
    {
        if (Time.time >= nextBlink && !Game.State.IsPlaying)
        {
            float wait;
            if (plunge)
            {
                anims[index].SetTrigger("Start Bright");
                wait = idleWait * Mathf.Pow((1 - plunger.GetNormalizedForce()), 3);
                wait = Mathf.Max(wait, minWait);
            }
            else
            {
                anims[index].SetTrigger("Start");
                wait = idleWait;
            }
            nextBlink = Time.time + wait;
            index = (index + 1) % anims.Length;
        }
    }

    private void OnPlungerStart()
    {
        if (Game.State.IsPlaying)
        {
            return;
        }
        plunge = true;
    }

    private void OnPlungerEnd()
    {
        if (Game.State.IsPlaying)
        {
            return;
        }
        plunge = false;
    }
}
