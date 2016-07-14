using UnityEngine;
using System.Collections;

public class ACTIVATEGAME : MonoBehaviour {

    public GameObject BG;
    public GameObject MB;
    public AudioSource A;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void GO ()
    {
        BG.SetActive(true);
        MB.SetActive(true);
        A.time = 8;
        gameObject.SetActive(false);
    }
}
