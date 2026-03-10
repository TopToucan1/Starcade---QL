using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour
{
    public GameObject ball;
    public DoorResetter topDoorResetter;
    public DoorResetter leftBottomDoorResetter;
    public DoorResetter rightBottomDoorResetter;
    public float tableSlopeAngle;
    public Animator capAnimator;

    private BallContoller ballController;

    void Start()
    {
        ballController = ball.GetComponent<BallContoller>();

        float g = 9.81f * 50;
        float y = -g * Mathf.Cos(Mathf.Deg2Rad * tableSlopeAngle);
        float z = -g * Mathf.Sin(Mathf.Deg2Rad * tableSlopeAngle);
        Physics.gravity = new Vector3(0, y, z);

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = -1;

        GameState.OnBallLoss += OnBallLoss;
        GameState.OnNewBall += OnNewBall;
    }

    void OnDestroy()
    {
        GameState.OnBallLoss -= OnBallLoss;
        GameState.OnNewBall -= OnNewBall;
    }

    void Update()
    {
        if (Game.build == Build.Release)
        {
            return;
        }
        
        if (Input.GetButtonDown("Promo In"))
        {
            Game.State.PromoIn(100);
        }
        else if (Input.GetButtonDown("Free In"))
        {
            Game.State.FreewaysIn(100);
        }
        else if (Input.GetButtonDown("Pause"))
        {
            if (Time.timeScale < 0.01)
            {
                Time.timeScale = 1;
            }
            else
            {
                Time.timeScale = 0;
            }
        }
        else if (Input.GetButtonDown("Progressive"))
        {
            Game.State.SetDebugProgressive();
        }
        else if (Input.GetButtonDown("JP Numbers"))
        {
            Game.State.SetJackpotNumbers();
        }
        else if (Input.GetButtonDown("Free Numbers"))
        {
            Game.State.SetFreeNumbers();
        }
    }

    private void OnNewBall()
    {
    }

    private void OnBallLoss()
    {
        Reset();
        ballController.Reset();
    }

    private void Reset()
    {
        Game.State.Reset();
        topDoorResetter.Reset();
        leftBottomDoorResetter.Reset();
        rightBottomDoorResetter.Reset();
        capAnimator.SetBool("IsOpened", false);
    }
}
