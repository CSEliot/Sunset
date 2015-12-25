using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Master : MonoBehaviour {

    public int Max_Players;
    public GameObject[] AllCharacters;
    public int PlayableCharacters;

    private GameObject clientCharacter;
    public int Client_CharNum;

    void Awake()
    {
        if (GameObject.FindGameObjectsWithTag("Master").Length > 1)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        clientCharacter = AllCharacters[0];
        Client_CharNum = 0;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
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
        return clientCharacter.name;
    }
}
