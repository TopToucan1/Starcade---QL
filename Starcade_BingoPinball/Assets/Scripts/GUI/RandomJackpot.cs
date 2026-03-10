using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class RandomJackpot : MonoBehaviour
{
    public List<Sprite> icons;
    public Image image1;
    public Image image2;
    public Text JpBalance;
    public AudioSource ambient;

    private int count = 19;

    private bool canTake = false;
    private float takeDelay = 0.15f;

    private float balance = 0;

    private bool jpOpened = false;
    private bool soundPlayed;

    private AudioSource sound;

    private static RandomJackpot instance;

    public static RandomJackpot Instance
    {
        get
        {
            return instance;
        }
    }

    void Awake()
    {
        instance = this;
        sound = GetComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        if (!canTake)
        {
            return;
        }

        if (takeDelay < 0f)
        {
            // Magic for fixed take time
            float magicNumber = 100;
            float magicFactor = Game.State.ProgressiveWin / magicNumber;
            balance += 50f * Time.fixedDeltaTime * Mathf.Pow(magicNumber / 500f, 0.33f) * magicFactor;
            if (balance >= Game.State.ProgressiveWin)
            {
                balance = Game.State.ProgressiveWin;
                canTake = false;
                Game.State.TakeProgressive();
                GetComponent<Animator>().SetTrigger("Close");
            }

            JpBalance.text = Mathf.FloorToInt(balance).ToString();
        }
        else
        {
            takeDelay -= Time.fixedDeltaTime;
        }
    }

    public void EmitJp()
    {
        if (jpOpened)
        {
            return;
        }
        GetComponent<Animator>().SetTrigger("Open");
        GetComponent<Animator>().SetBool("Stop", false);
        JpBalance.text = "";
        balance = 0;
        takeDelay = 0.3f;
        count = 19;
        jpOpened = true;
        soundPlayed = false;
        ambient.Pause();
    }

    void ChangeFirstIcon()
    {
        if (canTake)
        {
            image1.sprite = icons[Game.State.GetJackpot()];
        }
        else
        {
            image1.sprite = icons[Random.Range(0, icons.Count)];
        }
    }

    void ChangeSecondIcon()
    {
        count--;
        image2.sprite = icons[Random.Range(0, icons.Count)];
        if (count <= 0)
        {
            GetComponent<Animator>().SetBool("Stop", true);
            canTake = true;
        }
    }

    public bool JpOpened()
    {
        return jpOpened;
    }

    void CloseJP()
    {
        jpOpened = false;
        ambient.UnPause();
    }

    void PlaySound(int num)
    {
        if (!soundPlayed)
        {
            soundPlayed = true;
            sound.Play();
        }
    }

    public void BallLoss()
    {
        GameObject ball = GameObject.FindGameObjectWithTag("Ball");
        Rigidbody ballRigidbody = ball.GetComponent<Rigidbody>();
        ballRigidbody.constraints = RigidbodyConstraints.FreezePositionY;
        Game.State.BallLoss();
    }
}
