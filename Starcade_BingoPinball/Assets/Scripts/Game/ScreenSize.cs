using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScreenSize : MonoBehaviour
{
    int screenWidth = 768;
    int screenHeight = 1366;

    CanvasScaler canv;

    void Awake()
    {
        canv = FindObjectOfType<CanvasScaler>();
    }

    void Start()
    {
        if (Game.platform == Platform.Pc)
        {
            Screen.SetResolution(screenWidth, screenHeight, false);
        }
        else
        {
            Screen.SetResolution(1080, 1920, true);
        }
        screenWidth = 0;
        screenHeight = 0;
    }

    void Update()
    {
        if (Screen.width != screenWidth || Screen.height != screenHeight)
            ChangeResolution();
    }

    void ChangeResolution()
    {
        int width = Screen.width;
        int height = Screen.height;
        float p = 1080.0f * height / (1920.0f * width);
        if (p <= 1f)
        {
            GetComponent<Camera>().rect = new Rect((1f - p) / 2f, 0, p, 1);
            canv.matchWidthOrHeight = 1;
        }
        else
        {
            p = 1.0f / p;
            GetComponent<Camera>().rect = new Rect(0, (1f - p) / 2f, 1, p);
            canv.matchWidthOrHeight = 0;
        }
        screenWidth = width;
        screenHeight = height;
    }
}
