using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RtpText : MonoBehaviour
{
    private Text text;

    void Start()
    {
        GameState.OnBallLoss += UpdateRtp;

        text = GetComponent<Text>();
        if (Game.build == Build.Release)
        {
            text.text = "";
        }
    }

    void OnDestroy()
    {
        GameState.OnBallLoss -= UpdateRtp;
    }

    private void UpdateRtp()
    {
        if (Game.build == Build.Debug)
        {
            text.text = "RTP: " + Game.State.GetRtp().ToString("0.000") + "\nMax RTP: " + Game.State.GetMaxRtp().ToString("0.000");
        }
        else
        {
            text.text = "";
        }
    }
}
