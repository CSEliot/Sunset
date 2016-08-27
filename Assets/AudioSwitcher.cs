using UnityEngine;
using System.Collections;

/// <summary>
/// Switches AudioListeners between UI and Stage
/// </summary>
public class AudioSwitcher : MonoBehaviour {

    private AudioListener stageListener;
    private Master _M;
    private bool isOn = false;

	// Use this for initialization
	void Start () {
        stageListener = GameObject.FindGameObjectWithTag("StageCamera")
            .GetComponent<AudioListener>();

        _M = GameObject.FindGameObjectWithTag("Master")
            .GetComponent<Master>();

	}
	
	// Update is called once per frame
	void Update () {
	
        if(!isOn && _M.CurrentMenu == Master.Menu.ingame)
        {
            isOn = true;
            stageListener.enabled = true;
        }

        if (isOn && _M.CurrentMenu != Master.Menu.ingame)
        {
            isOn = false;
            stageListener.enabled = false;
        }


    }

}
