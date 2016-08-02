using UnityEngine;
using System.Collections;

public class CheatPlayButton : MonoBehaviour {

    private int totalPresses;
    public int CheatPressCount;
    private ReadyUp rdyScript;

	// Use this for initialization
	void Start () {
        rdyScript = GameObject.FindGameObjectWithTag("GameHUD").GetComponent<ReadyUp>();
        totalPresses = 0;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void CHEAT()
    {
        totalPresses++;
        if(totalPresses > CheatPressCount)
        {
            totalPresses = 0;
            rdyScript.CheatGame();
        }
    }
}
