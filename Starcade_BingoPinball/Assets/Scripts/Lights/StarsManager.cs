using UnityEngine;
using System.Collections;

public class StarsManager : MonoBehaviour
{
    public float minTime;
    public float maxTime;
    public float minSpeed;
    public float maxSpeed;

    private float nextTime;
    private Animator[] animators;

    void Start()
    {
        nextTime = 0;
        animators = GetComponentsInChildren<Animator>();
    }
    
    void Update()
    {
        if (Time.time > nextTime)
        {
            nextTime += Random.Range(minTime, maxTime);
            Animator anim = animators[Random.Range(0, animators.Length)];
            anim.speed = Random.Range(minSpeed, maxSpeed);
            anim.SetTrigger("Blink");
        }
    }
}
