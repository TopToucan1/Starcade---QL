using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public AudioClip[] targetSounds;

    private static SoundManager instance;
    private int targetSoundIndex;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            return;
        }
    }

    public AudioClip TargetSoundClip
    {
        get
        {
            return targetSounds[targetSoundIndex++ % targetSounds.Length];
        }
    }

    public static SoundManager Instance
    {
        get
        {
            return instance;
        }
    }
}
