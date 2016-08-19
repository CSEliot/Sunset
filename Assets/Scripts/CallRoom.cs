using UnityEngine;
using System.Collections;

public class CallRoom : MonoBehaviour {

    public string RoomName;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void AssignNewRoom()
    {
        GameObject.Find("Master").GetComponent<Master>().SetArenaName(RoomName);
    }
}
