using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public enum Platform
{
    Board,
    Pc
}

public enum Build
{
    Release,
    Debug
}

public class Game : MonoBehaviour
{
    public const string NAME = "pinball";
    public const int VERSION_NUMBER = 21;
    public static Platform platform = Platform.Pc;
    public static Build build = Build.Release;
    public static bool demo = true;

    private static GameState state;

    private static bool isStateLoaded = false;     // true if state loaded

    float autoexitTimer = 0;

    private static Dictionary<string, Texture2D> numberTextures;
    private static Dictionary<string, Texture2D> markedNumberTextures;

    void Awake()
    {
        if (File.Exists("config.txt"))
        {
            using (StreamReader sr = new StreamReader("config.txt"))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    string[] tokens;
                    tokens = line.Split('=');
                    if (tokens.Length != 2)
                    {
                        continue;
                    }
                    print(tokens[0] + ":" + tokens[1]);
                    switch (tokens[0])
                    {
                        case "platform":
                            if (tokens[1] == "pc")
                            {
                                platform = Platform.Pc;
                            }
                            else
                            {
                                platform = Platform.Board;
                            }
                            break;
                        case "debug":
                            if (tokens[1] == "true")
                            {
                                build = Build.Debug;
                            }
                            else
                            {
                                build = Build.Release;
                            }
                            break;
                    }
                }
            }
        }

        AudioListener.volume = 0;

        if (state == null)
        {
            switch (platform)
            {
                case Platform.Board:
                    DaemonManager.Instance.RequestLoadState();
                    DaemonManager.Instance.RequestLoadSettings();
                    DaemonManager.Instance.RequestLoadBalance();
                    DaemonManager.Instance.RequestLoadStatistics();
                    DaemonManager.Instance.RequestLoadProgressive();
                    break;
                case Platform.Pc:
                    state = Saver.LoadState();
                    Saver.LoadSettings();
                    Saver.LoadBalance();
                    Saver.LoadProgressive();
                    isStateLoaded = true;
                    AudioListener.volume = 1;
                    break;
            }
            if (state == null)
            {
                state = new GameState();
                if (Game.demo)
                {
                    Game.State.PromoIn(1000);
                }
            }
        }

        Texture2D[] textures = Resources.LoadAll<Texture2D>("Numbers/Normal");
        numberTextures = new Dictionary<string, Texture2D>();
        foreach (var t in textures)
        {
            numberTextures.Add(t.name, t);
        }
        textures = Resources.LoadAll<Texture2D>("Numbers/Checked");
        markedNumberTextures = new Dictionary<string, Texture2D>();
        foreach (var t in textures)
        {
            markedNumberTextures.Add(t.name, t);
        }
    }

    void Start()
    {
        if (!isStateLoaded)
        {
            Time.timeScale = 0;
            return;
        }

        autoexitTimer = Time.time;

        StartGame();
    }

    void Update()
    {
        if (InputBroker.GetButtonDown("Exit") && !State.IsPlaying)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }

        if (State.IsPlaying || State.CanPlay())
            autoexitTimer = Time.time;

        if (Time.time - autoexitTimer > 30)
            Application.Quit();
    }

    void OnApplicationQuit()
    {
        Game.Daemon.Exit();
    }

    public static bool IsStateLoaded
    {
        get
        {
            return isStateLoaded;
        }
    }

    public static GameState State
    {
        get
        {
            return state;
        }
    }

    public static DaemonManager Daemon
    {
        get
        {
            return DaemonManager.Instance;
        }
    }

    public static Dictionary<string, Texture2D> NumberTextures
    {
        get
        {
            return numberTextures;
        }
    }

    public static Dictionary<string, Texture2D> MarkedNumberTextures
    {
        get
        {
            return markedNumberTextures;
        }
    }

    public static void LoadState(GameState state)
    {
        Game.state = state;
    }

    public static void StartGame()
    {
        Time.timeScale = 1;

        if (state.IsPlaying)
        {
            state.Continue();
        }
        else
        {
            state.Start();
        }
    }
}
