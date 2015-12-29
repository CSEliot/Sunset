using UnityEngine;
using System.Collections;

public class MenuSFX : MonoBehaviour {

    private Master m;

	// Use this for initialization
	void Start () {
        m = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
        
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown("Horizontal") || Input.GetButtonDown("Vertical"))
        {
            m.PlaySFX(1);
        }
        if (Input.GetButtonDown("Submit"))
        {
            m.PlaySFX(0);
        }
	}
}
