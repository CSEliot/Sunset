using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ToggleControlShow : MonoBehaviour {

    /// <summary>
    /// Setting isOn is the most "Unity" way for this toggle.
    /// But assigning the toggle also activates the toggle so
    /// it just undoes itself. Yeah, it's dumb, I know.
    /// </summary>
    private bool dontRetoggle;

	// Use this for initialization
	void Awake () {
        GetComponent<Image>().enabled = PlayerPrefs.GetInt("ShowControls", 1) == 1 ? true : false;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Show() {
        GameObject.FindGameObjectWithTag("Master").GetComponent<Master>().IsControlsShown = 
            !GameObject.FindGameObjectWithTag("Master").GetComponent<Master>().IsControlsShown;
    }
}
