using UnityEngine;
using System.Collections;

public class CheatPlayButton : MonoBehaviour {

    private int totalPresses;
    public int CheatPressCount;
    private MatchHUD matchHUD;

	// Use this for initialization
	void Start () {
        matchHUD = GameObject.FindGameObjectWithTag("MatchHUD").GetComponent<MatchHUD>();
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
            matchHUD.CheatGame();
        }
    }
}
