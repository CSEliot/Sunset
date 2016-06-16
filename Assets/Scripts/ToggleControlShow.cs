using UnityEngine;
using System.Collections;

public class ToggleControlShow : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Show() {
        GameObject.FindGameObjectWithTag("Master").GetComponent<Master>().IsControlsShown = !GameObject.FindGameObjectWithTag("Master").GetComponent<Master>().IsControlsShown;
    }
}
