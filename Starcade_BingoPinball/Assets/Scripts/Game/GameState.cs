using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

enum Currency
{
    Tickets,
    Dollars
}

[Serializable]
public class GameState
{
    public delegate void Action();
    public static event Action OnBalanceChange;
    public static event Action OnNewBall;
    public static event Action OnBallLoss;
    public static event Action OnBonusTrigger;
    public static event Action OnTopRolloverSet;
    public static event Action OnBottomDoorChange;
    public static event Action OnBetChange;
    public static event Action OnProgressiveChange;
    public delegate void ActionObject(object obj);
    public static event ActionObject OnNumberHit;

    private Bingo bingo;

    // 3-Balance in cents
    [NonSerialized]
    private int promo;
    [NonSerialized]
    private int freeways;
    [NonSerialized]
    private int rewards;

    [NonSerialized]
    private Currency currencyType = Currency.Tickets;

    // For debug
    private static int[] creditCosts = { 1, 2, 5, 10, 20, 25, 50, 100 };
    [NonSerialized]
    private int creditCost = 1;         // In cents
    private int ratio = 93;

    private int bet = 4;

    private int freeBalls;  // Free Balls count
    private const int FREE_BALLS_AMOUNT = 2;

    [NonSerialized]
    private Dictionary<string, Dictionary<string, object>> progressives;
    private bool isProgressiveWon;
    private string winningFundName;
    private int jpWin;

    [NonSerialized]
    private Dictionary<string, int> statistics;

    // TODO: Remove
    private bool[] topRollovers = { false, false, false, false };   // Currently enabled rollovers
    private Dictionary<Side, bool> bottomDoorsOpened = new Dictionary<Side, bool>()   // Is doors opened
    {
        { Side.Left, true },
        { Side.Right, true }
    };

    private bool isPlaying;

    [NonSerialized]
    private Dictionary<string, object> history;

    [NonSerialized]
    private bool debugProgressive;

    // RTP
    private float maxTotalWin = 0f;
    private float totalWin = 0f;
    private int gamesCount = 0;

    private bool firstBallPlayed = false;

    //////////////////////////////////////////////////////////////////
    public GameState()
    {
        bingo = new Bingo();

        freeBalls = 0;
        isProgressiveWon = false;
        ResetTopRollovers();

        InitProgressive();
    }

    public void Start()
    {
        EmitBalanceChange();
        EmitBottomDoorChange();
        EmitNewBall();
        EmitBetChange();
    }

    public void Continue()
    {
        BallLoss();     // Take winnings
        EmitBalanceChange();
        EmitBottomDoorChange();
        EmitNewBall();
        EmitBetChange();
    }

    public bool IsPlaying
    {
        get
        {
            return isPlaying;
        }
    }

    public void Reset()
    {
        foreach (var side in bottomDoorsOpened.Keys.ToList())
        {
            bottomDoorsOpened[side] = true;
        }
    }
    //////////////////////////////////////////////////////////////////
    // Balance
    //////////////////////////////////////////////////////////////////
    public int Promo
    {
        get
        {
            return promo;
        }
        set
        {
            promo = value;
        }
    }

    public int Freeways
    {
        get
        {
            return freeways;
        }
        set
        {
            freeways = value;
        }
    }

    public int Rewards
    {
        get
        {
            return rewards;
        }
        set
        {
            rewards = value;
        }
    }

    public string FormattedPromo
    {
        get
        {
            return promo.ToString();
        }
    }

    public string FormattedFreeways
    {
        get
        {
            return FormatMoney(freeways);
        }
    }

    public string FormattedRewards
    {
        get
        {
            int win = 0;
            if (isPlaying)
            {
                win = GetBingoWin();
            }
            return FormatMoney(rewards + win);
        }
    }

    public string FormattedBingoWin
    {
        get
        {
            return FormatMoney(GetBingoWin());
        }
    }

    public int CreditCost
    {
        get
        {
            return creditCost;
        }
        set
        {
            creditCost = value;
        }
    }

    public int Ratio
    {
        get
        {
            return ratio;
        }
    }

    public void PromoIn(int amount)
    {
        promo += amount;
        EmitBalanceChange();
    }

    public void FreewaysIn(int amount)
    {
        freeways += amount;
        EmitBalanceChange();
    }

    public void ToggleCurrency()
    {
        currencyType = currencyType == Currency.Dollars ? Currency.Tickets : Currency.Dollars;
        EmitBalanceChange();
        EmitBetChange();
    }

    public void ChangeDenomination()
    {
        // Just debug code
        int i;
        for (i = 0; i < creditCosts.Length; i++)
        {
            if (creditCosts[i] == creditCost)
            {
                break;
            }
        }
        creditCost = creditCosts[(i + 1) % creditCosts.Length];
        EmitBalanceChange();
        EmitBetChange();
    }

    private void IncRewards(int amount)
    {
        rewards += amount;
        IncWinStatistics(amount);
    }

    private string FormatMoney(int cents)
    {
        if (currencyType == Currency.Dollars)
        {
            return FormatToDollars(cents);
        }
        else if (currencyType == Currency.Tickets)
        {
            return FormatToCredits(cents);
        }
        return null;
    }

    private string FormatToDollars(int cents)
    {
        return "$" + ((float)cents / 100).ToString("0.00");
    }

    private string FormatToCredits(int cents)
    {
        return (cents / creditCost).ToString("0");
    }

    public int Win
    {
        get
        {
            return GetBingoWin();
        }
    }

    public int BallsLeft
    {
        get
        {
            return (int)Mathf.Floor((promo + freeways + rewards) / bet);
        }
    }
    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    // Bet
    //////////////////////////////////////////////////////////////////
    public string FormattedBet
    {
        get
        {
            string betString = null;
            switch (currencyType)
            {
                case Currency.Dollars:
                    betString = "$" + ((float)bet / 100).ToString("0.00");
                    break;
                case Currency.Tickets:
                    betString = bet.ToString("0");
                    break;
            }
            return betString;
        }
    }

    public int Bet
    {
        get
        {
            return bet;
        }
    }
    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    // Bingo
    //////////////////////////////////////////////////////////////////
    public BingoCard GetBingoCard()
    {
        return bingo.Card;
    }

    public void BingoNumberHit(int number)
    {
        if (!bingo.TrySetNumber(number))
        {
            return;
        }
        int[] index = bingo.Card.GetIndexOfNumber(number);
        EmitNumberHit(index);
    }

    public int[,] GetBingoNumbers()
    {
        return bingo.Card.Numbers;
    }

    public int GetNumberToHit()
    {
        return bingo.GetNumberToHit();
    }

    public List<int> GetNumbersToHit()
    {
        return bingo.Card.NumbersToHit;
    }

    public List<int> GetNumbersNotOnCard()
    {
        return bingo.Card.NumbersNotOnCard;
    }

    public List<int> GetNumbersAlreadyHit()
    {
        return bingo.Card.NumbersAlreadyHit;
    }

    public int[] GetCombinationNumbers()
    {
        bool jpCombo = bingo.IsJackpotHit();
        if (jpCombo)
        {
            // Assume that only one combination for JP
            return Bingo.Patterns[Combinations.Jackpot][0];
        }
        return bingo.GetCombinationNumbers();
    }

    public Combinations GetCombination()
    {
        return bingo.GetCombination();
    }

    public bool IsJackpotHit()
    {
        return bingo.IsJackpotHit();
    }

    public int[] GetFreeBallsNumbers()
    {
        return Bingo.CheckFreeBalls(bingo.Card);
    }

    public bool IsFreeBallsHit()
    {
        return bingo.IsFreeBallsHit();
    }

    public bool IsNumberOnCard(int number)
    {
        return bingo.Card.IsNumberOnCard(number);
    }
    // For Debug
    public void SetJackpotNumbers()
    {
        bingo.SetJackpotNumbers();
    }

    // For Debug
    public void SetFreeNumbers()
    {
        bingo.SetFreeNumbers();
    }

    private void TakeBingoWin()
    {
        var win = GetBingoWin();
        IncRewards(win);
    }

    private int GetBingoWin()
    {
        return (int)(bingo.GetWin() * bet);
    }
    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    // Settings
    //////////////////////////////////////////////////////////////////
    public void SetSetting(string name, string value, string type)
    {
        switch (name)
        {
            case "credit_cost":
                creditCost = Int32.Parse(value);
                break;
            case "bet":
                bet = Int32.Parse(value);
                break;
            case "volume":
                AudioListener.volume = 0.01f * Int32.Parse(value);
                break;
            case "ratio":
                ratio = Int32.Parse(value);
                break;
            default:
                break;
        }
    }
    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    // Bonus
    //////////////////////////////////////////////////////////////////
    public int FreeBalls
    {
        get
        {
            return freeBalls;
        }
    }

    public int FutureFreeBalls
    {
        get
        {
            return (bingo.IsFreeBallsHit() && isPlaying) ? FREE_BALLS_AMOUNT : 0;
        }
    }
    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    // Progressive
    //////////////////////////////////////////////////////////////////
    public Dictionary<string, Dictionary<string, object>> Progressives
    {
        get
        {
            if (progressives == null)
            {
                InitProgressive();
            }
            return progressives;
        }
        set
        {
            progressives = value;
        }
    }

    public void SetProgressiveEnabled(string name, bool enabled)
    {
        if (progressives == null)
        {
            InitProgressive();
        }
        progressives[name]["enabled"] = enabled;
    }

    public void SetProgressiveStatus(string name, string status)
    {
        if (progressives == null)
        {
            InitProgressive();
        }
        progressives[name]["issuance_status"] = status;
    }

    public void SetProgressiveParameter(string name, string setting, float value)
    {
        if (progressives == null)
        {
            InitProgressive();
        }
        progressives[name][setting] = value;
    }

    public void SetDebugProgressive()
    {
        debugProgressive = true;
    }

    public bool IsProgressiveWon
    {
        get
        {
            return isProgressiveWon;
        }
    }

    public int ProgressiveWin
    {
        get
        {
            if (!isProgressiveWon)
            {
                return 0;
            }
            return (int)Math.Floor((float)progressives[winningFundName]["current"]);
        }
    }

    public int GetJackpot()
    {
        int i = 0;
        foreach (var p in progressives)
        {
            if ((bool)p.Value["enabled"] && (float)p.Value["min_bet"] <= bet
                && bet <= (float)p.Value["max_bet"] && (string)p.Value["issuance_status"] == "pending")
            {
                return i;
            }
            i++;
        }
        return -1;
    }

    private void InitProgressive()
    {
        string[] names = { "jp_red", "jp_blue", "jp_white", "jp_gold", "jp_silver", "jp_platinum" };
        string[] parameters = { "current", "init", "max_bet", "contribution", "start", "limit", "enabled", "issuance_status", "min_bet" };
        Dictionary<string, object> progressiveParams = new Dictionary<string, object>();
        foreach (var param in parameters)
        {
            object value = null;
            // For PC version
            switch (param)
            {
                case "min_bet":
                    value = 0f;
                    break;
                case "current":
                case "init":
                case "max_bet":
                case "start":
                    value = 100f;
                    break;
                case "contribution":
                    value = 0.1f;
                    break;
                case "limit":
                    value = 500f;
                    break;
                case "enabled":
                    value = true;
                    break;
                case "issuance_status":
                    value = "-";
                    break;
            }
            progressiveParams.Add(param, value);
        }
        progressives = new Dictionary<string, Dictionary<string, object>>();
        foreach (var name in names)
        {
            progressives.Add(name, new Dictionary<string, object>(progressiveParams));
        }
    }

    private bool CheckProgressiveWin()
    {
        Func<Dictionary<string, object>, bool> check = (fund) =>
        {
            return (bool)fund["enabled"] && bet >= (float)fund["min_bet"] && bet <= (float)fund["max_bet"];
        };

        Dictionary<string, double> probabilities = new Dictionary<string, double>();
        foreach (var fund in progressives)
        {
            if (!check(fund.Value))
            {
                continue;
            }
            float current = (float)fund.Value["current"];
            float min = (float)fund.Value["start"];
            float max = (float)fund.Value["limit"];
            if (current < min)
            {
                continue;
            }
            var probability = Math.Pow((current - min) / (max - min), 6);
            probabilities.Add(fund.Key, probability);
        }

        if (debugProgressive)
        {
            string[] keys = progressives.Keys.ToArray();
            // ignore min_bet and max_bet for debug
            string key = keys[UnityEngine.Random.Range(0, keys.Length)];
            winningFundName = key;
            jpWin = (int)Math.Floor((float)progressives[winningFundName]["current"]);
            progressives[key]["issuance_status"] = "pending";
            debugProgressive = false;
            return true;
        }

        var rnd = UnityEngine.Random.Range(0, 1f);
        foreach (var p in probabilities)
        {
            if (!check(progressives[p.Key]))
            {
                continue;
            }
            if (progressives[p.Key]["issuance_status"].Equals("pending"))
            {
                winningFundName = p.Key;
                jpWin = (int)Math.Floor((float)progressives[winningFundName]["current"]);
                return true;
            }
            if (rnd < p.Value)
            {
                progressives[p.Key]["issuance_status"] = "pending";
                winningFundName = p.Key;
                jpWin = (int)Math.Floor((float)progressives[winningFundName]["current"]);
                return true;
            }
        }

        return false;
    }

    public void TakeProgressive()
    {
        var win = (int)Math.Floor((float)progressives[winningFundName]["current"]);
        IncRewards(win);
        progressives[winningFundName]["current"] = (float)progressives[winningFundName]["current"] - win;
        progressives[winningFundName]["current"] = (float)progressives[winningFundName]["current"] + (float)progressives[winningFundName]["init"];
        progressives[winningFundName]["issuance_status"] = "-";
        isProgressiveWon = false;
        EmitBalanceChange();
        EmitProgressiveChange();
    }
    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    // Statistics
    //////////////////////////////////////////////////////////////////
    public Dictionary<string, int> Statistics
    {
        get
        {
            if (statistics == null)
            {
                InitStatisitcs();
            }
            return statistics;
        }
    }

    private void InitStatisitcs()
    {
        string[] parameters = { "total_bet", "total_win" };
        statistics = new Dictionary<string, int>();
        foreach (var p in parameters)
        {
            statistics.Add(p, 0);
        }
    }

    public void IncBetStatistics(int bet)
    {
        if (statistics == null)
        {
            InitStatisitcs();
        }
        statistics["total_bet"] += bet;
    }

    public void IncWinStatistics(int win)
    {
        if (statistics == null)
        {
            InitStatisitcs();
        }
        statistics["total_win"] += win;
    }

    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    // Pinball methods
    //////////////////////////////////////////////////////////////////
    public void StartGame()
    {
        // Skip all stuff after loading after crash
        if (isPlaying)
        {
            return;
        }

        firstBallPlayed = true;

        if (freeBalls > 0)
        {
            Debug.Log("Free ball played");
            freeBalls--;
        }
        else
        {
            if (promo >= bet)
            {
                promo -= bet;
            }
            else if (freeways >= bet)
            {
                freeways -= bet;
            }
            else if (promo > 0 && promo + rewards >= bet)
            {
                rewards -= (bet - promo);
                promo = 0;
            }
            else if (freeways > 0 && freeways + rewards >= bet)
            {
                rewards -= (bet - freeways);
                freeways = 0;
            }
            else
            {
                rewards -= bet;
            }
            IncBetStatistics(bet);

            foreach (var fund in progressives)
            {
                if ((float)fund.Value["limit"] >= (float)fund.Value["current"])
                {
                    continue;
                }
                fund.Value["current"] = (float)fund.Value["current"] + (float)fund.Value["contribution"] * bet * 0.01f;
            }
            isProgressiveWon = CheckProgressiveWin();

            EmitProgressiveChange();
            EmitBalanceChange();
        }

        bingo.NewGame(isProgressiveWon);
        gamesCount++;
        maxTotalWin += bingo.GetMaxWin();

        isPlaying = true;

        EmitNewBall();
    }

    public bool IsFirstBallPlayed
    {
        get
        {
            return firstBallPlayed;
        }
    }

    public void BallLoss()
    {
        isPlaying = false;

        TakeBingoWin();
        if (bingo.IsFreeBallsHit())
        {
            Debug.Log("Free Balls won");
            freeBalls += FREE_BALLS_AMOUNT;
        }
        UpdateHistory();

        EmitBallLoss();
        EmitBalanceChange();
        EmitBottomDoorChange();

        totalWin += bingo.GetWin();
    }

    public bool CanPlay()
    {
        return promo + rewards >= bet || freeways + rewards >= bet || freeBalls > 0;
    }

    public void SetTopRollover(int number)
    {
        if (number < 0 || number > 3)
        {
            Debug.Log("Error: rollover must be between 0 and 3");
        }
        topRollovers[number] = true;
        EmitTopRolloverSet();

        if (CheckTopRollovers())
        {
            freeBalls++;
            ResetTopRollovers();
            EmitTopRolloverSet();
            EmitBonusAchieve();
        }
    }

    public bool[] TopRollovers
    {
        get
        {
            return topRollovers;
        }
    }

    private void ResetTopRollovers()
    {
        for (int i = 0; i < topRollovers.Length; i++)
        {
            topRollovers[i] = false;
        }
    }

    private bool CheckTopRollovers()
    {
        foreach (var ro in topRollovers)
        {
            if (!ro)
            {
                return false;
            }
        }
        // TODO: remove or fix
        return false;
        //return true;
    }

    public void TriggerOuterBottomRollover(Side side)
    {
        // TODO: discuss and fix
        //IncRewards(BONUS_AMOUNT * GetBet());
        //EmitBalanceChange();
    }

    public void TriggerInnerBottomRollover(Side side)
    {
        OpenBottomDoor(side);
    }

    public Dictionary<Side, bool> BottomDoorsOpened
    {
        get
        {
            return bottomDoorsOpened;
        }
    }

    public void CloseBottomDoor(Side side)
    {
        bottomDoorsOpened[side] = false;
    }

    private void OpenBottomDoor(Side side)
    {
        bottomDoorsOpened[side] = true;
        EmitBottomDoorChange();
    }
    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    // History
    //////////////////////////////////////////////////////////////////
    public Dictionary<string, object> History
    {
        get
        {
            return history;
        }
    }

    private void UpdateHistory()
    {
        if (Game.platform == Platform.Pc)
        {
            return;
        }
        if (history == null)
        {
            InitHistory();
        }

        if (!IsJackpotHit())
        {
            jpWin = 0;
        }

        history["free_play_balance"] = promo;
        history["play_balance"] = freeways;
        history["start_balance"] = rewards - Win - jpWin;
        history["end_balance"] = rewards;
        history["bet"] = bet;
        history["win"] = Win;
        history["jp_win"] = jpWin;
        history["jp_red"] = Mathf.Floor((float)progressives["jp_red"]["current"]);
        history["jp_blue"] = Mathf.Floor((float)progressives["jp_blue"]["current"]);
        history["jp_white"] = Mathf.Floor((float)progressives["jp_white"]["current"]);
        history["jp_platinum"] = Mathf.Floor((float)progressives["jp_platinum"]["current"]);
        history["jp_gold"] = Mathf.Floor((float)progressives["jp_gold"]["current"]);
        history["jp_silver"] = Mathf.Floor((float)progressives["jp_silver"]["current"]);
        history["field"] = bingo.GetFieldString();

        Game.Daemon.RequestSaveHistory();
    }

    private void InitHistory()
    {
        history = new Dictionary<string, object>()
        {
            { "game_id", Game.NAME },
            { "free_play_balance", 0 },
            { "play_balance", 0 },
            { "start_balance", 0 },
            { "end_balacne", 0 },
            { "bet", 0 },
            { "win", 0 },
            { "jp_win", 0 },
            { "jp_red", 0 },
            { "jp_blue", 0 },
            { "jp_white", 0 },
            { "jp_platinum", 0 },
            { "jp_gold", 0 },
            { "jp_silver", 0 },
            { "field", "" }
        };
    }
    //////////////////////////////////////////////////////////////////

    //////////////////////////////////////////////////////////////////
    // RTP
    //////////////////////////////////////////////////////////////////
    public float GetRtp()
    {
        return totalWin / gamesCount;
    }

    public float GetMaxRtp()
    {
        return maxTotalWin / gamesCount;
    }
    //////////////////////////////////////////////////////////////////

    #region Events
    //////////////////////////////////////////////////////////////////
    // Events
    //////////////////////////////////////////////////////////////////
    private void EmitBalanceChange()
    {
        if (OnBalanceChange != null)
        {
            OnBalanceChange();
        }
    }

    private void EmitNewBall()
    {
        if (OnNewBall != null)
        {
            OnNewBall();
        }
    }

    private void EmitBallLoss()
    {
        if (OnBallLoss != null)
        {
            OnBallLoss();
        }
    }

    private void EmitNumberHit(int[] index)
    {
        if (OnNumberHit != null)
        {
            OnNumberHit(index);
        }
    }

    private void EmitBonusAchieve()
    {
        if (OnBonusTrigger != null)
        {
            OnBonusTrigger();
        }
    }

    private void EmitTopRolloverSet()
    {
        if (OnTopRolloverSet != null)
        {
            OnTopRolloverSet();
        }
    }

    private void EmitBottomDoorChange()
    {
        if (OnBottomDoorChange != null)
        {
            OnBottomDoorChange();
        }
    }

    private void EmitBetChange()
    {
        if (OnBetChange != null)
        {
            OnBetChange();
        }
    }

    private void EmitProgressiveChange()
    {
        if (OnProgressiveChange != null)
        {
            OnProgressiveChange();
        }
    }
    //////////////////////////////////////////////////////////////////
    #endregion
}
