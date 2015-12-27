using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class Master : MonoBehaviour {

    [Serializable]
    public class MyEnumEntry
    {
        public MyEnum key;
        public int value;
    }
    public Dictionary<string, float> CharStrengths;
    public int Max_Players;
    public GameObject[] AllCharacters;
    public int PlayableCharacters;

    private GameObject clientCharacter;
    public int Client_CharNum;
    public int Player_Number;


    void Awake()
    {
        if (GameObject.FindGameObjectsWithTag("Master").Length > 1)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        AssignClientCharacter(0);
        Cursor.lockState = CursorLockMode.Confined;
    }

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown("escape"))
        {
            //PhotonNetwork.DestroyPlayerObjects();
            PhotonNetwork.Disconnect();
            AssignClientCharacter(0);
            SceneManager.LoadScene("CharacterSelect");
        }
	}

    public void AssignClientCharacter(int chosenChar)
    {
        clientCharacter = AllCharacters[chosenChar];
        Client_CharNum = chosenChar;
    }

    public string GetClientCharacter()
    {
        Debug.Log("Sending: " + clientCharacter.name);
        return clientCharacter.name;
    }
}
