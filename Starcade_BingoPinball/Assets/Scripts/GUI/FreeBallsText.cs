using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FreeBallsText : MonoBehaviour
{
    private Text text;

    void Start()
    {
        GameState.OnNewBall += UpdateText;
        GameState.OnBallLoss += UpdateText;
        GameState.OnNumberHit += UpdateText;
        text = GetComponent<Text>();
        UpdateText();
    }

    void OnDestroy()
    {
        GameState.OnNewBall -= UpdateText;
        GameState.OnBallLoss -= UpdateText;
        GameState.OnNumberHit -= UpdateText;
    }

    private void UpdateText()
    {
        text.text = (Game.State.FreeBalls + Game.State.FutureFreeBalls).ToString();
    }

    private void UpdateText(object obj)
    {
        UpdateText();
    }
}
