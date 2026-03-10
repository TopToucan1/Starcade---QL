using UnityEngine;
using System.Collections;
using System.Linq;
using System;
using System.Text;
using System.Collections.Generic;

public class BingoCardViewer : MonoBehaviour
{
    public GameObject number;
    public float xPadding;
    public float zPadding;
    public float emission;
    public float lightIntensity;
    public Color lightColor;

    private Transform[] numbers;
    private Renderer[] numberRenderers;
    private Animator[] numberAnimators;

    void Start()
    {
        GameState.OnNewBall += UpdateCard;
        GameState.OnNumberHit += UpdateNumber;

        Vector3 offset = new Vector3();
        Vector3 rotation = new Vector3(0, 180, 0);
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (i == 2 && j == 2)
                {
                    continue;
                }

                offset.x = xPadding * j;
                offset.z = -zPadding * i;
                GameObject clone = Instantiate(number, transform.position, transform.rotation) as GameObject;
                clone.transform.parent = transform;
                clone.transform.localPosition += offset;
                clone.transform.localScale *= 100;
                clone.transform.localEulerAngles = rotation;
                clone.name = number.name + " " + (i * 5 + j);
            }
        }

        numbers = new Transform[25];
        numberAnimators = new Animator[25];
        numberRenderers = new Renderer[25];
        Transform objectTransform;
        for (int i = 0; i < 25; i++)
        {
            if (i == 12)
            {
                continue;
            }
            objectTransform = transform.Find("Card Number " + i);
            numbers[i] = objectTransform;
            numberRenderers[i] = objectTransform.gameObject.GetComponent<Renderer>();
            numberAnimators[i] = objectTransform.gameObject.GetComponent<Animator>();
        }

        UpdateCard();
    }

    void OnDestroy()
    {
        GameState.OnNewBall -= UpdateCard;
        GameState.OnNumberHit -= UpdateNumber;
    }

    public void UpdateCard()
    {
        BingoCard card = Game.State.GetBingoCard();
        List<int> markedNumbersIndices = card.MarkedNumbersIndices;
        Renderer numberRenderer;
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (i == 2 && j == 2)
                {
                    continue;
                }

                int number = card.GetNumber(i, j);
                numberRenderer = numberRenderers[i * 5 + j];

                if (markedNumbersIndices.Contains(i * 5 + j + 1))
                {
                    numberRenderer.material.SetTexture("_MainTex", Game.MarkedNumberTextures[number.ToString()]);
                    SetLight(numberAnimators[i * 5 + j], true);
                }
                else
                {
                    numberRenderer.material.SetTexture("_MainTex", Game.NumberTextures[number.ToString()]);
                    SetLight(numberAnimators[i * 5 + j], false);
                }
            }
        }
    }

    private void UpdateNumber(object index)
    {
        int[] intIndex = (int[])index;
        Transform objectTransform = numbers[intIndex[0] * 5 + intIndex[1]];
        Renderer numberRenderer;
        Texture oldTexture;
        if (objectTransform != null)
        {
            numberRenderer = numberRenderers[intIndex[0] * 5 + intIndex[1]];
            oldTexture = numberRenderer.material.GetTexture("_MainTex");
            numberRenderer.material.SetTexture("_MainTex", Game.MarkedNumberTextures[oldTexture.name]);
            SetLight(numberAnimators[intIndex[0] * 5 + intIndex[1]], true);
        }
    }

    private void SetLight(Animator animator, bool enable)
    {
        if (enable)
        {
            animator.SetBool("Enabled", true);
        }
        else
        {
            animator.SetBool("Enabled", false);
            animator.SetBool("Blink", false);
            animator.SetBool("Fast Blink", false);
        }
    }
}
