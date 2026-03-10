using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum BalanceType
{
    Promo,
    Freeways,
    Rewards
}

public class BalanceText : MonoBehaviour
{
    public BalanceType balanceType;

    private Text text;

    void Start()
    {
        GameState.OnBalanceChange += OnBalanceChange;
        GameState.OnNumberHit += OnBalanceChange;
        text = GetComponent<Text>();
        UpdateBalance();
    }

    void OnDestroy()
    {
        GameState.OnBalanceChange -= OnBalanceChange;
        GameState.OnNumberHit -= OnBalanceChange;
    }

    private void UpdateBalance()
    {
        switch (balanceType)
        {
            case BalanceType.Freeways:
                text.text = Game.State.FormattedFreeways;
                break;
            case BalanceType.Promo:
                text.text = Game.State.FormattedPromo;
                break;
            case BalanceType.Rewards:
                text.text = Game.State.FormattedRewards;
                break;
        }
    }

    private void OnBalanceChange()
    {
        UpdateBalance();
    }

    private void OnBalanceChange(object obj)
    {
        UpdateBalance();
    }
}
