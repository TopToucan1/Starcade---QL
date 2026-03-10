using UnityEngine;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;

public class BingoNumberViewer : MonoBehaviour
{
    public Texture2D questionTexture;
    public int targetsCount;
    public float updateDelay;

    private IEnumerator updateNumbers;
    private bool isUpdating = false;

    private Renderer[] numberRenderers;
    private BingoNumberTrigger[] triggers;

    void Awake()
    {
        GameState.OnNewBall += UpdateNumbers;
        GameState.OnBallLoss += OnBallLoss;

        numberRenderers = new Renderer[targetsCount];
        triggers = new BingoNumberTrigger[targetsCount];
        for (int i = 1; i < 1 + targetsCount; i++)
        {
            GameObject targetObject = transform.Find("Bingo Number " + i).gameObject;
            numberRenderers[i - 1] = targetObject.transform.Find("Number").gameObject.GetComponent<Renderer>();
            triggers[i - 1] = targetObject.GetComponentInChildren<BingoNumberTrigger>();
        }
    }

    void OnDestroy()
    {
        GameState.OnNewBall -= UpdateNumbers;
        GameState.OnBallLoss -= OnBallLoss;
    }

    void Start()
    {
        UpdateNumbers();
    }

    private void UpdateNumbers()
    {
        if (isUpdating)
        {
            return;
        }

        List<int> range = new List<int>(Game.State.GetNumbersToHit());
        range.AddRange(Game.State.GetNumbersToHit());      // Increase probability (x2) of numbers on card than not hit yet
        range.AddRange(Game.State.GetNumbersToHit());      // Increase probability (x3) of numbers on card than not hit yet
        range.AddRange(Game.State.GetNumbersAlreadyHit());
        range.AddRange(Game.State.GetNumbersNotOnCard());
        List<int> triggerIndices = Enumerable.Range(0, targetsCount).ToList();
        BingoNumberTrigger trigger;
        Renderer numberRenderer;
        for (int i = 1; i < 1 + targetsCount; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, triggerIndices.Count);
            int triggerIndex = triggerIndices[randomIndex];
            trigger = triggers[triggerIndex];
            triggerIndices.RemoveAt(randomIndex);
            if (trigger.IsNumberOpened)
            {
                continue;
            }
            int number = range[i - 1];
            
            numberRenderer = numberRenderers[triggerIndex];
            numberRenderer.material.SetTexture("_MainTex", Game.MarkedNumberTextures[number.ToString()]);

            trigger.Number = number;
        }

        updateNumbers = WaitAndUpdate();
        StartCoroutine(updateNumbers);
    }

    private IEnumerator WaitAndUpdate()
    {
        yield return new WaitForSeconds(updateDelay);
        isUpdating = false;
        UpdateNumbers();
        isUpdating = true;
    }

    private void OnBallLoss()
    {
        StopCoroutine(updateNumbers);
        isUpdating = false;
    }
}
