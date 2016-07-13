using UnityEngine;
using System.Collections;

public class ExitHelper : MonoBehaviour {

    private Master m;

	// Use this for initialization
	void Start () {

        m = GameObject.Find("Master").GetComponent<Master>();

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void ExitMatch()
    {
        m.PlaySFX(0);
        m.GoBack();
    }
}
