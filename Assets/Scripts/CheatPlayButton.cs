using UnityEngine;
using System.Collections;

public class CheatPlayButton : MonoBehaviour {

    private int totalPresses;
    public int CheatPressCount;
    private WaitGUIController gameHUD;

	// Use this for initialization
	void Start () {
        gameHUD = GameObject.FindGameObjectWithTag("MatchHUD").GetComponent<WaitGUIController>();
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
            gameHUD.CheatGame();
        }
    }
}
