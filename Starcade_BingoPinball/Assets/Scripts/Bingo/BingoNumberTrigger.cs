using UnityEngine;
using System.Collections;
using System;

public class BingoNumberTrigger : MonoBehaviour
{
    public delegate void Action(Transform t);
    public static event Action OnNumberHit;

    public Texture2D questionTexture;
    public float emission;
    public float lightIntensity;
    public Color lightColor;
    public Color winColor;

    private bool isNumberOpened = false;
    private Light numberLight;
    private Renderer numberRenderer;
    private AudioSource sound;
    private int currentNumber;

    void Start()
    {
        GameObject targetObject;
        numberLight = transform.parent.GetComponentInChildren<Light>();
        targetObject = transform.parent.Find("Number").gameObject;
        numberRenderer = targetObject.GetComponent<Renderer>();
        sound = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball") && !isNumberOpened)
        {
            int number = currentNumber;
            numberRenderer.material.SetTexture("_MainTex", Game.MarkedNumberTextures[number.ToString()]);
            Color color = lightColor;
            if (Game.State.IsNumberOnCard(number))
            {
                color = winColor;
            }
            numberRenderer.material.SetColor("_EmissionColor", color * Mathf.LinearToGammaSpace(emission));
            numberLight.intensity = lightIntensity;
            numberLight.color = color;

            Game.State.BingoNumberHit(currentNumber);

            StartCoroutine(HideNumber());
            isNumberOpened = true;

            sound.clip = SoundManager.Instance.TargetSoundClip;
            sound.Play();

            EmitNumberHit();
        }
    }

    public bool IsNumberOpened
    {
        get
        {
            return isNumberOpened;
        }
    }

    public int Number
    {
        get
        {
            return currentNumber;
        }
        set
        {
            currentNumber = value;
        }
    }

    private IEnumerator HideNumber()
    {
        yield return new WaitForSeconds(2);
        int number = UnityEngine.Random.Range(1, 75);
        numberRenderer.material.SetTexture("_MainTex", Game.MarkedNumberTextures[number.ToString()]);
        numberRenderer.material.SetColor("_EmissionColor", Color.white * Mathf.LinearToGammaSpace(0f));
        numberLight.intensity = 0;
        isNumberOpened = false;
    }

    private void EmitNumberHit()
    {
        if (OnNumberHit != null)
        {
            OnNumberHit(transform);
        }
    }
}
