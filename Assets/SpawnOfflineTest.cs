using UnityEngine;
using System.Collections;

public class SpawnOfflineTest : MonoBehaviour {

    private Master m;

    public GameObject[] CharacterList;

    public Transform[] SpawnList;

	// Use this for initialization
	void Start () {
        m = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();

        //chosen character is stored by name, we don't know position number

        Instantiate(CharacterList[m.Client_CharNum], SpawnList[0].position, Quaternion.Euler(Vector3.zero));
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
