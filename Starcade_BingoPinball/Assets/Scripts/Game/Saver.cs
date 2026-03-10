using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class Saver : MonoBehaviour
{
    public const string STATE_FILENAME = "pinball.state";
    public const string VERSION_FILENAME = "pinball.version";
    public const string BALANCE_FILENAME = "pinball.balance";
    public const string PROGRESSIVE_FILENAME = "pinball.progressive";
    public const string SETTINGS_FILENAME = "pinball.settings";

    private static bool isVersionCorrect;

    void Awake()
    {
        GameState.OnBalanceChange += SaveBalance;
        GameState.OnBallLoss += SaveState;
        GameState.OnBallLoss += SaveStatistics;
        GameState.OnBonusTrigger += SaveState;
        GameState.OnBottomDoorChange += SaveState;
        GameState.OnNewBall += SaveState;
        GameState.OnNewBall += SaveStatistics;
        GameState.OnNumberHit += SaveState;
        GameState.OnTopRolloverSet += SaveState;
        GameState.OnProgressiveChange += SaveProgressive;

        isVersionCorrect = CheckVersion();
        SaveVersion();
    }

    void OnDestroy()
    {
        GameState.OnBalanceChange -= SaveBalance;
        GameState.OnBallLoss -= SaveState;
        GameState.OnBallLoss -= SaveStatistics;
        GameState.OnBonusTrigger -= SaveState;
        GameState.OnBottomDoorChange -= SaveState;
        GameState.OnNewBall -= SaveState;
        GameState.OnNewBall -= SaveStatistics;
        GameState.OnNumberHit -= SaveState;
        GameState.OnTopRolloverSet -= SaveState;
        GameState.OnProgressiveChange -= SaveProgressive;
    }

    public static void Save(string filename, object obj)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(filename);
        bf.Serialize(file, obj);
        file.Close();
    }

    public static void SaveState()
    {
        switch (Game.platform)
        {
            case Platform.Board:
                Game.Daemon.RequestSaveState();
                break;
            case Platform.Pc:
                Save(STATE_FILENAME, Game.State);
                SaveSettings();
                break;
        }
    }

    private static void SaveState(object o)
    {
        SaveState();
    }

    public static void SaveBalance()
    {
        switch (Game.platform)
        {
            case Platform.Board:
                Game.Daemon.RequestSaveBalance();
                break;
            case Platform.Pc:
                Save(BALANCE_FILENAME, new Dictionary<string, int>()
                {
                    { "promo", Game.State.Promo },
                    { "freeways", Game.State.Freeways },
                    { "rewards", Game.State.Rewards }
                });
                break;
        }
    }

    public static void SaveProgressive()
    {
        switch (Game.platform)
        {
            case Platform.Board:
                Game.Daemon.RequestSaveProgressive();
                break;
            case Platform.Pc:
                Save(PROGRESSIVE_FILENAME, Game.State.Progressives);
                break;
        }
    }

    public static void SaveSettings()
    {
        switch (Game.platform)
        {
            case Platform.Board:
                // No need
                break;
            case Platform.Pc:
                Save(SETTINGS_FILENAME, new Dictionary<string, int>() {
                    { "credit_cost", Game.State.CreditCost },
                    { "volume", (int)(AudioListener.volume * 100) }
                });
                break;
        }
    }

    public static void SaveStatistics()
    {
        switch (Game.platform)
        {
            case Platform.Board:
                Game.Daemon.RequestSaveStatistics();
                break;
            case Platform.Pc:
                break;
        }
    }

    private static void SaveVersion()
    {
        return;
        //Save(VERSION_FILENAME, Game.VERSION_NUMBER);
    }

    public static void SaveAll()
    {
        SaveState();
    }

    // For OnNumberHit event
    public static void SaveAll(object obj)
    {
        SaveAll();
    }

    public static object Load(string filename)
    {
        if (File.Exists(filename))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(filename, FileMode.Open);
            object obj = bf.Deserialize(file);
            file.Close();
            return obj;
        }
        return null;
    }

    private static object LoadWithCheck(string filename)
    {
        return isVersionCorrect ? Load(filename) : null;
    }

    public static GameState LoadState()
    {
        return LoadWithCheck(STATE_FILENAME) as GameState;
    }

    public static void LoadBalance()
    {
        Dictionary<string, int> balances = LoadWithCheck(BALANCE_FILENAME) as Dictionary<string, int>;
        if (balances == null)
        {
            return;
        }
        Game.State.Rewards = balances["rewards"];
        Game.State.Promo = balances["promo"];
        Game.State.Freeways = balances["freeways"];
    }

    public static void LoadProgressive()
    {
        var progressives = LoadWithCheck(PROGRESSIVE_FILENAME) as Dictionary<string, Dictionary<string, object>>;
        if (progressives == null)
        {
            return;
        }
        Game.State.Progressives = progressives;
    }

    public static void LoadSettings()
    {
        Dictionary<string, int> settings = LoadWithCheck(SETTINGS_FILENAME) as Dictionary<string, int>;
        if (settings == null)
        {
            return;
        }
        AudioListener.volume = 0.01f * settings["volume"];
        Game.State.CreditCost = settings["credit_cost"];
    }

    private static bool CheckVersion()
    {
        return true;
        //object version = Load(VERSION_FILENAME);
        //return version != null && (int)version == Game.VERSION_NUMBER;
    }
}
