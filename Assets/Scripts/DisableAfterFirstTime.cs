using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableAfterFirstTime : MonoBehaviour {

	// Use this for initialization
	void Start () {
		if(PlayerPrefs.GetInt("isFirstTime", 1) == 1 ? true : false)
        {
            //enable this?
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
