using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameInputController : MonoBehaviour {

    private static Dictionary<string, bool> ButtonStatesIsDown;
    private static Dictionary<string, float> AxisStates;

    public delegate bool stringName (string Name);
    public delegate float stringNameFloat (string Name);
    public delegate void stringNameVoid (string Name);

    public delegate void stringNameFloatAxis (string Name, float Axis);
    public delegate void stringNameParams (string Name, params string[] Names);
    
    /// <summary>
    /// Returns true for as long as the button is held down.
    /// </summary>
    public stringName GetButton;
    public stringNameParams SetButtonUp;
    public stringNameParams SetButtonDown;

    public stringNameFloat GetAxis;
    public stringNameVoid SetAxisUp;
    public stringNameFloatAxis SetAxisDown;

    // Use this for initialization
    void Awake () {
        ButtonStatesIsDown = new Dictionary<string, bool>();
        AxisStates = new Dictionary<string, float>();

        GetButton = mobileGetButton;
        SetButtonDown = mobileSetButtonDown;
        SetButtonUp = mobileSetButtonUp;
        GetAxis = mobileGetAxis;
        SetAxisUp = mobileSetAxisUp;
        SetAxisDown = mobileSetAxisDown;

        if (Application.isEditor || Application.isMobilePlatform)
        {
            GetButton = Input.GetButton;
            GetAxis = Input.GetAxis;
        }
    }

// Update is called once per frame
    void Update () {
	
	}

    private void mobileSetButtonDown(string name, params string[] names)
    {
        int totalUp = (names == null) ? -1 : names.Length;

        if (!ButtonStatesIsDown.ContainsKey(name))
        {
            ButtonStatesIsDown.Add(name, true);
        }
        else
        {
            ButtonStatesIsDown[name] = true;
        }

        if (totalUp > 0)
        {
            for (int x = 0; x < totalUp; x++)
            {
                ButtonStatesIsDown[names[x]] = false;
            }
        }
    }

    private void mobileSetButtonUp(string name, params string[] names)
    {
        int totalUp = (names == null) ? -1 : names.Length;

        if (!ButtonStatesIsDown.ContainsKey(name))
        {
            ButtonStatesIsDown.Add(name, false);
        }
        else
        {
            ButtonStatesIsDown[name] = false;
        }

        if (totalUp > 0)
        {
            for (int x = 0; x < totalUp; x++)
            {
                ButtonStatesIsDown[names[x]] = false;
            }
        }
    }

    /// <summary>
    /// Returns true for as long as button is held down.
    /// </summary>
    private bool mobileGetButton(string name)
    {
        if (!ButtonStatesIsDown.ContainsKey(name))
        {
            ButtonStatesIsDown.Add(name, false);
            return false;
        }
        return ButtonStatesIsDown[name];
    }

    //public bool GetButtonUp(string name)
    //{
    //    if (!ButtonStatesIsDown.ContainsKey(name))
    //    {
    //        ButtonStatesIsDown.Add(name, false);
    //        return false;
    //    }

    //    return !ButtonStatesIsDown[name];
    //}

    /// <summary>
    /// SET Axis Down is applied via event system.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private float mobileGetAxis(string name)
    {
        if (!AxisStates.ContainsKey(name))
        {
            AxisStates.Add(name, 0f);
            return 0f;
        }
        float temp = AxisStates[name];
        AxisStates[name] = 0f;

        return temp;
    }

    private void mobileSetAxisDown(string name, float axis)
    {
        if (!AxisStates.ContainsKey(name))
        {
            AxisStates.Add(name, axis);
        }
        else
        {
            AxisStates[name] = axis;
        }
    }

    private void mobileSetAxisUp(string name)
    {
        if (!AxisStates.ContainsKey(name))
        {
            AxisStates.Add(name, 0f);
        }
        else
        {
            AxisStates[name] = 0f;
        }
    }
}
