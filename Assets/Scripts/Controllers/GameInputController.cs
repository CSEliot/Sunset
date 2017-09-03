using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameInputController : MonoBehaviour {

    private static Dictionary<string, bool> ButtonStatesIsDown;
    private static Dictionary<string, bool> ButtonStatesChanged;
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
    public stringName GetButtonUp;
    public stringName GetButtonDown;
    public stringNameParams SetButtonUp;
    public stringNameParams SetButtonDown;

    public stringNameFloat GetAxis;
    public stringNameVoid SetAxisUp;
    public stringNameFloatAxis SetAxisDown;

    private bool buttonStateChange;

    // Use this for initialization
    void Awake () {
        ButtonStatesIsDown = new Dictionary<string, bool>();
        ButtonStatesChanged = new Dictionary<string, bool>();

        AxisStates = new Dictionary<string, float>();


        GetButton = mobileGetButton;
        GetButtonDown = mobileGetButtonDown;
        GetButtonUp = mobileGetButtonUp;
        SetButtonDown = mobileSetButtonDown;
        SetButtonUp = mobileSetButtonUp;
        GetAxis = mobileGetAxis;
        SetAxisUp = mobileSetAxisUp;
        SetAxisDown = mobileSetAxisDown;

        if (Application.isEditor || !Application.isMobilePlatform)
        {
            GetButton = Input.GetButton;
            GetButtonDown = Input.GetButtonDown;
            GetButtonUp = Input.GetButtonUp;
            GetAxis = Input.GetAxis;
        }

        buttonStateChange = false;
    }

// Update is called once per frame
    void LateUpdate () {
        //Ensure that "ButtonStatesChanged" only holds state for one frame.
        if (buttonStateChange)
        {
            buttonStateChange = false;
            foreach (var state in ButtonStatesChanged)
            {
                if (state.Value == true)
                    ButtonStatesChanged[state.Key] = false;
            }
        }
	}

    private void mobileSetButtonDown(string name, params string[] names)
    {
        int totalUp = (names == null) ? -1 : names.Length;
        buttonStateChange = true;

        if (!ButtonStatesIsDown.ContainsKey(name))
        {
            ButtonStatesIsDown.Add(name, true);
            ButtonStatesChanged.Add(name, true);
        }
        else
        {
            ButtonStatesIsDown[name] = true;
            ButtonStatesChanged[name] = true;

        }

        if (totalUp > 0)
        {
            for (int x = 0; x < totalUp; x++)
            {
                ButtonStatesIsDown[names[x]] = true;
                ButtonStatesChanged[names[x]] = true;
            }
        }
    }

    private void mobileSetButtonUp(string name, params string[] names)
    {
        int totalUp = (names == null) ? -1 : names.Length;
        buttonStateChange = true;

        if (!ButtonStatesIsDown.ContainsKey(name))
        {
            ButtonStatesIsDown.Add(name, false);
            ButtonStatesChanged.Add(name, true);
        }
        else
        {
            ButtonStatesIsDown[name] = false;
            ButtonStatesChanged[name] = true;
        }

        if (totalUp > 0)
        {
            for (int x = 0; x < totalUp; x++)
            {
                ButtonStatesIsDown[names[x]] = false;
                ButtonStatesChanged[names[x]] = true;
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

    /// <summary>
    /// Returns true for the one frame the button state changed to up.
    /// This function will likely be a frame behind.
    /// </summary>
    private bool mobileGetButtonUp(string name)
    {
        if (!ButtonStatesIsDown.ContainsKey(name))
        {
            ButtonStatesIsDown.Add(name, false);
            ButtonStatesChanged.Add(name, true);
            return false;
        }
        if (ButtonStatesChanged[name] && ButtonStatesIsDown[name])
            return true;
        else
            return false;
    }

    /// <summary>
    /// Returns true for the one frame the button state changed to down.
    /// </summary>
    private bool mobileGetButtonDown(string name)
    {
        if (!ButtonStatesIsDown.ContainsKey(name))
        {
            ButtonStatesIsDown.Add(name, false);
            ButtonStatesChanged.Add(name, true);
            return false;
        }
        if(ButtonStatesChanged[name] && !ButtonStatesIsDown[name])
            return true;
        else
            return false; 
    }

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
