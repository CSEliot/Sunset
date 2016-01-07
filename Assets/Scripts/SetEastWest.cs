using UnityEngine;
using System.Collections;

public class SetEastWest : MonoBehaviour {

    private Master m;

	// Use this for initialization
	void Start () {
	    m = GameObject.FindGameObjectWithTag("Master")
            .GetComponent<Master>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetServer(bool isEast)
    {
        m.SetServer(isEast);
    }
}
