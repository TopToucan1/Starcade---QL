using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class CombinationsHighlighter : MonoBehaviour
{
    public AudioClip winSound;
    public AudioClip freeBallsSound;

    private Dictionary<Combinations, Animator> animators;

    void Awake()
    {
        GameState.OnNumberHit += Check;
        GameState.OnNewBall += Reset;
    }

    void Start()
    {
        animators = new Dictionary<Combinations, Animator>();
        GameObject numberObject = transform.Find("Jackpot").gameObject;
        Animator objectAnimator = numberObject.GetComponent<Animator>();
        animators.Add(Combinations.Jackpot, objectAnimator);

        numberObject = transform.Find("Z").gameObject;
        objectAnimator = numberObject.GetComponent<Animator>();
        animators.Add(Combinations.Z, objectAnimator);

        numberObject = transform.Find("X").gameObject;
        objectAnimator = numberObject.GetComponent<Animator>();
        animators.Add(Combinations.X, objectAnimator);

        numberObject = transform.Find("L").gameObject;
        objectAnimator = numberObject.GetComponent<Animator>();
        animators.Add(Combinations.L, objectAnimator);

        numberObject = transform.Find("Four Corners").gameObject;
        objectAnimator = numberObject.GetComponent<Animator>();
        animators.Add(Combinations.FourCorneres, objectAnimator);

        numberObject = transform.Find("Y").gameObject;
        objectAnimator = numberObject.GetComponent<Animator>();
        animators.Add(Combinations.Y, objectAnimator);

        numberObject = transform.Find("T").gameObject;
        objectAnimator = numberObject.GetComponent<Animator>();
        animators.Add(Combinations.T, objectAnimator);

        numberObject = transform.Find("Line").gameObject;
        objectAnimator = numberObject.GetComponent<Animator>();
        animators.Add(Combinations.FiveInRow, objectAnimator);

        numberObject = transform.Find("Free Balls").gameObject;
        objectAnimator = numberObject.GetComponent<Animator>();
        animators.Add(Combinations.FreeBalls, objectAnimator);
    }

    void OnDestroy()
    {
        GameState.OnNumberHit -= Check;
        GameState.OnNewBall -= Reset;
    }

    private void Check(object obj)
    {
        bool isPlaying = false;
        if (Game.State.IsJackpotHit())
        {
            if (!animators[Combinations.Jackpot].GetBool("Enabled"))
            {
                AudioSource.PlayClipAtPoint(winSound, transform.position);
                isPlaying = true;
            }
            animators[Combinations.Jackpot].SetBool("Enabled", true);
        }
        if (Game.State.IsFreeBallsHit())
        {
            if (!animators[Combinations.FreeBalls].GetBool("Enabled") && !isPlaying)
            {
                AudioSource.PlayClipAtPoint(freeBallsSound, transform.position);
                isPlaying = true;
            }
            animators[Combinations.FreeBalls].SetBool("Enabled", true);
        }
        Combinations win = Game.State.GetCombination();
        if (win != Combinations.Nothing)
        {
            if (!animators[win].GetBool("Enabled") && !isPlaying)
            {
                AudioSource.PlayClipAtPoint(winSound, transform.position);
            }
            animators[win].SetBool("Enabled", true);
        }
        // Disable old lower combinations
        foreach (var animator in animators)
        {
            if (animator.Key != win && animator.Key != Combinations.Jackpot && animator.Key != Combinations.FreeBalls)
            {
                animator.Value.SetBool("Enabled", false);
            }
        }
    }

    private void Reset()
    {
        foreach (var animator in animators)
        {
            animator.Value.SetBool("Enabled", false);
        }
    }
}
