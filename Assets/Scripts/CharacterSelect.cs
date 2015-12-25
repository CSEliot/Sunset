using UnityEngine;
using System.Collections;

public class CharacterSelect : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetCharacterTo(int charNum)
    {
        GameObject m = GameObject.FindGameObjectWithTag("Master");
        if (m == null)
        {
            Debug.LogError("Master Not Found");
        }
        GameObject.FindGameObjectWithTag("Master").GetComponent<Master>().AssignClientCharacter(charNum);
    }
}
