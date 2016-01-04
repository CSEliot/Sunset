using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuSFX : MonoBehaviour {

    private Master m;


	// Use this for initialization
	void Start () {
        m = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
        //GetComponent<Slider>().onValueChanged.
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown("Up") || Input.GetButtonDown("Right")
            || Input.GetButtonDown("Left") || Input.GetButtonDown("Down"))
        {
            m.PlaySFX(1);
        }
        if (Input.GetButtonDown("Submit"))
        {
            m.PlaySFX(0);
        }
	}
}
