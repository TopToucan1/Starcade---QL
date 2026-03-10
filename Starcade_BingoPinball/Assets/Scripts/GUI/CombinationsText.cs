using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CombinationsText : MonoBehaviour
{
    public Combinations combination;

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

    private void OnBalanceChange()
    {
        text.text = (Game.State.Bet * Bingo.Paytable[combination]).ToString();
    }
}
