using UnityEngine;
using System.Collections;

public class PlungerController : MonoBehaviour
{
    public delegate void Action();
    public static event Action OnPlungeStart;
    public static event Action OnPlungeEnd;

    public float maxForce;
    public float chargeTime;
    public float returnSpeed;
    public AudioSource plungerSound;
    public AudioSource[] hitSounds;
    public Animator glassAnimator;

    private bool strike;
    private bool springReturn;
    private float startTime;
    private float defaultSpringOffset;

    private GameObject spring;

    void Start()
    {
        spring = transform.FindChild("Spring").gameObject;
        defaultSpringOffset = spring.transform.localPosition.y;
    }

    void Update()
    {
        strike = false;

        if (!Game.State.CanPlay())
        {
            return;
        }

        if (springReturn)
        {
            spring.transform.localPosition += new Vector3(0, Time.deltaTime * returnSpeed, 0);
            if (spring.transform.localPosition.y >= defaultSpringOffset)
            {
                spring.transform.localPosition = new Vector3(0, defaultSpringOffset, 0);
                strike = true;
                springReturn = false;
            }
        }
        else if (InputBroker.GetButtonDown("Plunge"))
        {
            startTime = Time.time;
            plungerSound.Play();
            glassAnimator.SetBool("Plunge", true);
            EmitPlungeStart();
            return;
        }
        else if (InputBroker.GetButtonUp("Plunge"))
        {
            springReturn = true;
            plungerSound.Stop();
            float force = GetForce() / maxForce;
            if (force < 0.1)
            {
                hitSounds[0].Play();
            }
            else if (force < 0.35)
            {
                hitSounds[1].Play();
            }
            else if (force < 0.5)
            {
                hitSounds[2].Play();
            }
            else
            {
                hitSounds[3].Play();
            }
            print("Hit Force: " + force);
            glassAnimator.SetBool("Plunge", false);
            EmitPlungeEnd();
            return;
        }

        if (InputBroker.GetButton("Plunge") && Time.time - startTime < chargeTime)
        {
            float offset = defaultSpringOffset - 0.1519f * (Time.time - startTime) / chargeTime;
            spring.transform.localPosition = new Vector3(0, offset, 0);
            return;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (strike && other.gameObject.CompareTag("Ball"))
        {
            float force = GetForce();
            other.gameObject.GetComponent<Rigidbody>().AddForce(Vector3.forward * force);
            strike = false;
        }
    }

    public float GetNormalizedForce()
    {
        return GetForce() / maxForce;
    }

    private float GetForce()
    {
        return maxForce * (Time.time - startTime) / chargeTime;
    }

    private void EmitPlungeStart()
    {
        if (OnPlungeStart != null)
        {
            OnPlungeStart();
        }
    }

    private void EmitPlungeEnd()
    {
        if (OnPlungeEnd != null)
        {
            OnPlungeEnd();
        }
    }
}
