﻿using UnityEngine;
using System.Collections;

public class SettingsManager : MonoBehaviour {

    private static int startLives;
    private static bool infoUpdated;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public static int StartLives {
        get {
            return startLives;
        }
    }

    /// <summary>
    /// Trigger. Returns to false after returning true.
    /// </summary>
    public static bool InfoUpdated {
        get {
            if (infoUpdated) {
                infoUpdated = false;
                return true;
            }
            return false;
        }
    }

    public static void SetGameInfo(int StartingLives)
    {
        startLives = StartingLives;
    }
}