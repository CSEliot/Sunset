﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// A statically available debugger for on-screen data visualization.
/// Focus on ease-of-use and not optimized.
///  - Eliot Carney-Seim
/// </summary>
///
[RequireComponent (typeof(Text))]
public class CBUG : MonoBehaviour {

    #region Public Unity-Assigned Vars
    public bool ALL_DEBUG_TOGGLE;
    public bool SendToConsole;
    public bool DisableOnScreen;
    public float ClearTime;
    public bool Clear;
    public int ClearAmount;
    public bool ClearAll;
    #endregion

    #region Private Vars
    private Text logText;
    private List<string> lines;
    private List<int> occurrences;
    private bool isParented;
    private float previousClear;
    private bool neverClear;
    private int maxLines = 33; //Tested, based on 24pt Min.
    #endregion


    // Use this for initialization
    void Start () {
        logText = GetComponent<Text>();
        lines = new List<string>();
        occurrences = new List<int>();
        if (DisableOnScreen)
            logText.color = new Color(0, 0, 0, 0);
        if (ClearTime == 0)
            neverClear = true;
        if (ClearAll)
            ClearAmount = -1;

        transform.tag = "CBUG";
	}
	
	// Update is called once per frame
	void Update () {

        if (!Debug.isDebugBuild)
            gameObject.SetActive(false);

        if (!ALL_DEBUG_TOGGLE)
            return;

        if (Clear) {
            Clear = false;
            _ClearLines(ClearAmount);
        }

        if (!isParented && GameObject.Find("CanvasGroup") != null) {
            isParented = true;
            GameObject.Find("CanvasGroup").transform.SetParent(transform, true);
        }

        logText.text = "";
        for(int x = 0; x < lines.Count; x++) {
            logText.text += lines[x] + " || " + occurrences[x] + "\n";
        }

        if (lines.Count > maxLines) {
            for (int x = 0; x < lines.Count - maxLines; x++) {
                lines.RemoveAt(x);
            }
        }

        if(!neverClear && Time.time - previousClear > ClearTime) {
            Clear = true;
            previousClear = Time.time;
        }
	}

    #region Debug Aliases
    public static void Log(string line)
    {
        GetRef()._Print(line);
    }

    public static void Do(string line)
    {
        GetRef()._Print(line);
    }

    public static void Log(string line, bool debugOn)
    {
        GetRef()._Print(line, debugOn);
    }

    public static void Do(string line, bool debugOn)
    {
        GetRef()._Print(line, debugOn);
    }
    #endregion


    #region Helper Functions
    private void _ClearLines(int amount)
    {
        if (lines.Count == 0)
            return;

        if(amount == -1)
            lines.Clear();
        else {
            amount = amount > lines.Count ? lines.Count : amount;
            for(int x = 0; x < amount; x++) {
                CBUG.Do("Removing line " + x + ". Total: " + amount);
                lines.RemoveAt(x);
            }
        }
    }

    private static CBUG GetRef()
    {
        return GameObject.FindGameObjectWithTag("CBUG").GetComponent<CBUG>();
    }

    private void _Print(string line)
    {
        if (!ALL_DEBUG_TOGGLE)
            return;

        if(SendToConsole)
            Debug.Log(line);

        if (lines.Contains(line)) {
            for (int x = 0; x < GetRef().lines.Count; x++) {
                if (lines[x] == line) {
                    occurrences[x]++;
                    break;
                }
            }
        } else {
            lines.Add(line);
            occurrences.Add(1);
        }
    }

    private void _Print(string line, bool debugOn)
    {
        if (ALL_DEBUG_TOGGLE && debugOn)
            _Print(line);
    }

    private void _Error(string line)
    {
        _Print("ERROR <~> " + line);
        logText.color = Color.red;
    }
    #endregion

    #region Public Static Functions
    public static void ClearLines(int amount)
    {
        GetRef()._ClearLines(amount);
    }

    public static void Print(string line)
    {
        GetRef()._Print(line);
    }

    public static void Print(string line, bool debugOn)
    {
        GetRef()._Print(line, debugOn);
    }

    public static void Error(string line)
    {
        GetRef()._Error(line);
    }
    #endregion
}