using UnityEngine;
using System.Collections;

public class PlayOnAwakeAssist : MonoBehaviour {

    public int AwakeSFX;

	// Use this for initialization
	void Awake () {
        GameObject.FindGameObjectWithTag("Master").GetComponent<Master>().PlaySFX(AwakeSFX);
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
