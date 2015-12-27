using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class Master : MonoBehaviour {

    [System.Serializable]
    public class NameToStrength
    {
        public string Name;
        public float Power;
    }
    public NameToStrength[] StrengthsList;
    private Dictionary<string, float> NameStrengthDict;
    public int Max_Players;
    public GameObject[] AllCharacters;
    public int PlayableCharacters;

    private GameObject clientCharacter;
    public int Client_CharNum;
    public int Player_Number;


    void Awake()
    {
        NameStrengthDict = new Dictionary<string,float>();
        foreach (NameToStrength character in StrengthsList){
            NameStrengthDict.Add(character.Name, character.Power);
        }

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

    public Dictionary<string, float> GetStrengthList()
    {
        return NameStrengthDict;
    }
}
