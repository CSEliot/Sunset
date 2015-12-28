using UnityEngine;
using System.Collections;

public class ReadyUp : MonoBehaviour {

    public GameObject SelectorYes;
    public GameObject SelectorNo;

    private bool isReady;

	// Use this for initialization
	void Start () {
        isReady = false;
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown("Left") && !isReady)
        {
            SelectorYes.SetActive(true);
            SelectorNo.SetActive(false);
        }
        if (Input.GetButtonDown("Right") && isReady)
        {
            SelectorYes.SetActive(false);
            SelectorNo.SetActive(true);
        }
        if (Input.GetButtonDown("Submit"))
        {

        }
	}
}
