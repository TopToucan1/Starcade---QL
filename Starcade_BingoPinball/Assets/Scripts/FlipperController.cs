using UnityEngine;
using System.Collections;
using System;

public enum Side
{
    Left = 0,
    Right = 1
};

public class FlipperController : MonoBehaviour
{
    public Side side;
    public float maxAngle;
    public float minAngle;
    public float speed;
    public AudioSource upSound;
    public AudioSource downSound;

    private bool buttonPressed;
    private Rigidbody rb;
    private float angle;
    private Vector3 rotationAxis;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        angle = 0;
        rotationAxis = new Vector3(0, 0, side == Side.Left ? -1 : 1);
        animator = GetComponent<Animator>();
        GameState.OnNewBall += OnNewBall;
        GameState.OnBallLoss += OnBallLoss;
        PlungerController.OnPlungeStart += OnPlungerStart;
        PlungerController.OnPlungeEnd += OnPlungerEnd;
    }

    void Update()
    {
        var button = GetSideButton();

        if (InputBroker.GetButtonDown(button))
        {
            buttonPressed = true;
            upSound.Play();
        }
        else if (InputBroker.GetButtonUp(button))
        {
            buttonPressed = false;
            downSound.Play();
        }

        if (!Game.State.IsPlaying)
        {
            animator.SetBool("Enabled", true);
        }
    }

    void FixedUpdate()
    {
        float direction = buttonPressed ? 1 : -1;
        Quaternion deltaRotation = Quaternion.Euler(rotationAxis * speed * direction * Time.deltaTime);
        if ((!buttonPressed || angle < maxAngle) && (buttonPressed || angle > minAngle))
        {
            angle += Math.Abs(speed * Time.deltaTime) * direction;
            rb.MoveRotation(transform.rotation * deltaRotation);
        }
    }

    private string GetSideButton()
    {
        return side == Side.Left ? "Left Flipper" : "Right Flipper";
    }

    private void OnPlungerStart()
    {
        if (Game.State.IsPlaying)
        {
            return;
        }
        animator.SetBool("Plunge", true);
    }

    private void OnPlungerEnd()
    {
        if (Game.State.IsPlaying)
        {
            return;
        }
        animator.SetBool("Plunge", false);
    }

    private void OnNewBall()
    {
        animator.SetBool("Enabled", false);
        animator.SetBool("Plunge", false);
    }

    private void OnBallLoss()
    {
        animator.SetBool("Enabled", true);
    }
}
