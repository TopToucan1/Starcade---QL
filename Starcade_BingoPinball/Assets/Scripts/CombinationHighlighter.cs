using UnityEngine;
using System.Collections;
using System.Linq;

public class CombinationHighlighter : MonoBehaviour
{
    private Animator[] animators;

    void Awake()
    {
        GameState.OnNumberHit += Check;
    }

    void Start()
    {
        animators = new Animator[25];
        for (int i = 0; i < 25; i++)
        {
            if (i == 12)
            {
                continue;
            }
            GameObject numberObject = transform.Find("Card Number " + i).gameObject;
            Animator objectAnimator = numberObject.GetComponent<Animator>();
            animators[i] = objectAnimator;
        }
    }

    void OnDestroy()
    {
        GameState.OnNumberHit -= Check;
    }

    private void Check(object obj)
    {
        int[] numbers = Game.State.GetCombinationNumbers();
        int[] fbNumbers = Game.State.GetFreeBallsNumbers();
        if (numbers != null)
        {
            for (int i = 0; i < 25; i++)
            {
                if (i == 12)
                {
                    continue;
                }
                animators[i].SetBool("Blink", numbers.Contains(i + 1));
                if (fbNumbers != null && fbNumbers.Contains(i + 1))
                {
                    animators[i].SetBool("Fast Blink", fbNumbers.Contains(i + 1));
                }
                animators[i].SetTrigger("Reset");
            }
        }
    }
}
