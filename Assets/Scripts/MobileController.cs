using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MobileController : MonoBehaviour {

    private Dictionary<string, bool> ButtonStates;
    private Dictionary<string, float> AxisStates;

    // Use this for initialization
    void Start () {
        ButtonStates = new Dictionary<string, bool>();
        AxisStates = new Dictionary<string, float>();
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetButtonDown(string name)
    {
        if (!ButtonStates.ContainsKey(name))
        {
            ButtonStates.Add(name, true);
        }
        else
        {
            ButtonStates[name] = true;
        }
    }

    public void SetButtonUp(string name)
    {
        if (!ButtonStates.ContainsKey(name))
        {
            ButtonStates.Add(name, false);
        }
        else
        {
            ButtonStates[name] = false;
        }
    }

    public bool GetButtonDown(string name)
    {
        if (!ButtonStates.ContainsKey(name))
        {
            ButtonStates.Add(name, false);
            return false;
        }
        bool temp = ButtonStates[name];
        ButtonStates[name] = false;
        
        return temp;
    }

    public bool GetButtonDownStay(string name)
    {
        if (!ButtonStates.ContainsKey(name))
        {
            ButtonStates.Add(name, false);
            return false;
        }
        bool temp = ButtonStates[name];

        return temp;
    }

    /// <summary>
    /// SET Axis Down is applied via event system.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public float GetAxis(string name)
    {
        if (!ButtonStates.ContainsKey(name))
        {
            AxisStates.Add(name, 0f);
            return 0f;
        }
        float temp = AxisStates[name];
        AxisStates[name] = 0f;

        return temp;
    }

    public void SetAxisDown(string name)
    {
        if (!ButtonStates.ContainsKey(name))
        {
            if(name == "MoveLeft")
            {
                AxisStates.Add(name, -1f);
            }
            else
            {
                AxisStates.Add(name, 1f);
            }
        }
        else
        {
            if (name == "MoveLeft")
            {
                AxisStates[name] = -1f;
            }
            else
            {
                AxisStates[name] = 1f;
            }
            AxisStates[name] = 1f;
        }
    }

    public void SetAxisUp(string name)
    {
        if (!ButtonStates.ContainsKey(name))
        {
            AxisStates.Add(name, 0f);
        }
        else
        {
            AxisStates[name] = 0f;
        }
    }
}
