using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class InputBroker
{
    private static Dictionary<string, bool> buttonPressedEvents = new Dictionary<string, bool>();
    private static HashSet<string> pressedButtons = new HashSet<string>();

    public static bool GetButtonDown(string name)
    {
        if (buttonPressedEvents.ContainsKey(name) && buttonPressedEvents[name])
        {
            buttonPressedEvents.Remove(name);
            return true;
        }
		if (Game.platform == Platform.Pc)
			return Input.GetButtonDown (name);
		else
			return false;
    }

    public static void SetButtonDown(string name)
    {
        if (!pressedButtons.Contains(name))
        {
            pressedButtons.Add(name);
        }

        if (buttonPressedEvents.ContainsKey(name))
        {
            buttonPressedEvents[name] = true;
        }
        else
        {
            buttonPressedEvents.Add(name, true);
        }
    }

    public static bool GetButtonUp(string name)
    {
        if (buttonPressedEvents.ContainsKey(name) && !buttonPressedEvents[name])
        {
            buttonPressedEvents.Remove(name);
            return true;
        }
		if (Game.platform == Platform.Pc)
			return Input.GetButtonUp (name);
		else
			return false;
    }

    public static void SetButtonUp(string name)
    {
        if (pressedButtons.Contains(name))
        {
            pressedButtons.Remove(name);
        }

        if (buttonPressedEvents.ContainsKey(name))
        {
            buttonPressedEvents[name] = false;
        }
        else
        {
            buttonPressedEvents.Add(name, false);
        }
    }

    public static bool GetButton(string name)
    {
        return Input.GetButton(name) || pressedButtons.Contains(name);
    }
}
