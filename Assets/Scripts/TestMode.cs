using UnityEngine;
using System.Collections;

public class TestMode : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetMode(bool isTest)
    {
        GameObject.FindGameObjectWithTag("Master").GetComponent<Master>().IsTestMode = isTest;
    }
    
}
