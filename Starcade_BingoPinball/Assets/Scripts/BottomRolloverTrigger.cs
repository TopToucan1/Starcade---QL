using UnityEngine;
using System.Collections;

public enum Position
{
    Inner,
    Outer
}

public class BottomRolloverTrigger : MonoBehaviour
{
    public Side side;
    public Position position;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            if (position == Position.Inner)
            {
                Game.State.TriggerInnerBottomRollover(side);
            }
            else
            {
                Game.State.TriggerOuterBottomRollover(side);
            }
        }
    }
}
