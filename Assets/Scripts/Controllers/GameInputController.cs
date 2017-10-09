using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameInputController : MonoBehaviour {

    public bool DebugTouchscreen;

    class ButtonState { public ButtonState(bool @is) { this.@is = @is; } public bool @is; }
    private static Dictionary<string, ButtonState> ButtonStatesIsDown;
    private static Dictionary<string, ButtonState> ButtonStatesChanged;
    class AxisState { public AxisState(float @is) { this.@is = @is; } public float @is; }
    private static Dictionary<string, AxisState> AxisStates;

    public delegate bool stringName (string Name);
    public delegate float stringNameFloat (string Name);
    public delegate void stringNameVoid (string Name);

    public delegate void stringNameFloatAxis (string Name, float Axis);
    public delegate void stringNameParams (params string[] Names);
    
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
        ButtonStatesIsDown = new Dictionary<string, ButtonState>();
        ButtonStatesChanged = new Dictionary<string, ButtonState>();

        AxisStates = new Dictionary<string, AxisState>();


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
            if (Application.isEditor && DebugTouchscreen)
            {
                SetButtonUp("Left", "Down", "Up", "Right", "Jump", "DownJump");
                SetAxisUp("MoveHorizontal");
            }
                
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
                if (state.Value.@is == true)
                    state.Value.@is = false;
            }
        }
	}

    private void mobileSetButtonDown(params string[] names)
    {
        int totalUp = (names == null) ? -1 : names.Length;
        buttonStateChange = true;

        foreach (var name in names)
        {
            if (!ButtonStatesIsDown.ContainsKey(name))
            {
                ButtonStatesIsDown.Add(name, new ButtonState(true));
                ButtonStatesChanged.Add(name, new ButtonState(true));
            }
            else
            {
                ButtonStatesIsDown[name].@is = true;
                ButtonStatesChanged[name].@is = true;
            }
        }

        if (totalUp > 0)
        {
            for (int x = 0; x < totalUp; x++)
            {
                ButtonStatesIsDown[names[x]].@is = true;
                ButtonStatesChanged[names[x]].@is = true;
            }
        }
    }

    private void mobileSetButtonUp(params string[] names)
    {
        int totalUp = (names == null) ? -1 : names.Length;
        buttonStateChange = true;

        foreach (var name in names)
        {
            if (!ButtonStatesIsDown.ContainsKey(name))
            {
                ButtonStatesIsDown.Add(name, new ButtonState(false));
                ButtonStatesChanged.Add(name, new ButtonState(true));
            }
            else
            {
                ButtonStatesIsDown[name].@is = true;
                ButtonStatesChanged[name].@is = true;
            }
        }

        if (totalUp > 0)
        {
            for (int x = 0; x < totalUp; x++)
            {
                ButtonStatesIsDown[names[x]].@is = false;
                ButtonStatesChanged[names[x]].@is = true;
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
            ButtonStatesIsDown.Add(name, new ButtonState(false));
            return false;
        }
        return ButtonStatesIsDown[name].@is;
    }

    /// <summary>
    /// Returns true for the one frame the button state changed to up.
    /// This function will likely be a frame behind.
    /// </summary>
    private bool mobileGetButtonUp(string name)
    {
        if (!ButtonStatesIsDown.ContainsKey(name))
        {
            ButtonStatesIsDown.Add(name, new ButtonState(false));
            ButtonStatesChanged.Add(name, new ButtonState(true));
            return false;
        }
        if (ButtonStatesChanged[name].@is && ButtonStatesIsDown[name].@is)
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
            ButtonStatesIsDown.Add(name, new ButtonState(false));
            ButtonStatesChanged.Add(name, new ButtonState(true));
            return false;
        }
        if(ButtonStatesChanged[name].@is && !ButtonStatesIsDown[name].@is)
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
            AxisStates.Add(name, new AxisState(0f));
            return 0f;
        }
        float temp = AxisStates[name].@is;
        AxisStates[name].@is = 0f;

        return temp;
    }

    private void mobileSetAxisDown(string name, float axis)
    {
        if (!AxisStates.ContainsKey(name))
        {
            AxisStates.Add(name, new AxisState(axis));
        }
        else
        {
            AxisStates[name].@is = axis;
        }
    }

    private void mobileSetAxisUp(string name)
    {
        if (!AxisStates.ContainsKey(name))
        {
            AxisStates.Add(name, new AxisState(0f));
        }
        else
        {
            AxisStates[name].@is = 0f;
        }
    }
}
