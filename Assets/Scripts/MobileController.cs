using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MobileController : MonoBehaviour {

    private Dictionary<string, bool> ButtonStates;

	// Use this for initialization
	void Start () {
        ButtonStates = new Dictionary<string, bool>();
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
}
