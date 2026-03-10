using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public enum ProgressiveType
{
    White,
    Blue,
    Red,
    Silver,
    Gold,
    Platinum
}

public class ProgressiveText : MonoBehaviour
{
    public ProgressiveType progressiveType;

    private Text text;

    void Start()
    {
        GameState.OnBalanceChange += OnBalanceChange;
        text = GetComponent<Text>();
    }

    void OnDestroy()
    {
        GameState.OnBalanceChange -= OnBalanceChange;
    }

    private void UpdateBalance()
    {
        var funds = Game.State.Progressives;
        switch (progressiveType)
        {
            case ProgressiveType.White:
                text.text = ((float)funds["jp_white"]["current"]).ToString("0");
                break;
            case ProgressiveType.Blue:
                text.text = ((float)funds["jp_blue"]["current"]).ToString("0");
                break;
            case ProgressiveType.Red:
                text.text = ((float)funds["jp_red"]["current"]).ToString("0");
                break;
            case ProgressiveType.Silver:
                text.text = ((float)funds["jp_silver"]["current"]).ToString("0");
                break;
            case ProgressiveType.Gold:
                text.text = ((float)funds["jp_gold"]["current"]).ToString("0");
                break;
            case ProgressiveType.Platinum:
                text.text = ((float)funds["jp_platinum"]["current"]).ToString("0");
                break;
        }
    }

    private void OnBalanceChange()
    {
        UpdateBalance();
    }
}
