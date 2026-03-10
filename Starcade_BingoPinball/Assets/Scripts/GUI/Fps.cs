using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Fps : MonoBehaviour
{
    public Text textField;

    int frames = 0;
    float seconds = 0f;

    void Start()
    {
        textField.text = "";
    }

    void Update()
    {
        if (Game.build == Build.Release)
        {
            return;
        }

        frames++;
        seconds += Time.deltaTime;
        if (seconds > 1f)
        {
            textField.text = "FPS: " + (frames / seconds).ToString("0");
            frames = 0;
            seconds = 0f;
        }
    }
}
