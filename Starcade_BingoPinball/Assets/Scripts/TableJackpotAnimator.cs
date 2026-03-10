using UnityEngine;
using System.Collections;

public class TableJackpotAnimator : MonoBehaviour
{
    public Animator capAnimator;

    private Material mat;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
    }

    void Update()
    {
        if (Game.State.IsProgressiveWon)
        {
            mat.SetColor("_EmissionColor", Color.white * Mathf.LinearToGammaSpace(0.1f));
            capAnimator.SetBool("IsOpened", true);
        }
        else
        {
            mat.SetColor("_EmissionColor", Color.white * Mathf.LinearToGammaSpace(0f));
            capAnimator.SetBool("IsOpened", false);
        }
    }
}
