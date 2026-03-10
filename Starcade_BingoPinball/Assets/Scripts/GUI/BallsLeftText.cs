using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BallsLeftText : MonoBehaviour
{
    private Text text;

    void Start()
    {
        GameState.OnNewBall += UpdateText;
        GameState.OnBallLoss += UpdateText;
        GameState.OnBalanceChange += UpdateText;
        text = GetComponent<Text>();
        UpdateText();
    }

    void OnDestroy()
    {
        GameState.OnNewBall -= UpdateText;
        GameState.OnBallLoss -= UpdateText;
        GameState.OnBalanceChange -= UpdateText;
    }

    private void UpdateText()
    {
        text.text = Game.State.BallsLeft.ToString();
    }
}
