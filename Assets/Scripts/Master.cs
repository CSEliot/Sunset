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

    public AudioClip[] SFX;
    public AudioClip[] MSX;
    private AudioSource myMusicAudio;
    private AudioSource mySFXAudio;

    private int totalPlayers;

    private bool isEast;

    void Awake()
    {
        myMusicAudio = GetComponent<AudioSource>();
        mySFXAudio = transform.GetChild(0).GetComponent<AudioSource>();
        PlayMSX(0);
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
        totalPlayers = 100;
	}
	
	// Update is called once per frame
	void Update () {
        
        if (Input.GetKeyDown("escape"))
        {
            //PhotonNetwork.DestroyPlayerObjects();
            PhotonNetwork.Disconnect();
            AssignClientCharacter(0);
            SceneManager.LoadScene("CharacterSelect");
            PlayMSX(0);
        }
        if (totalPlayers <= 1)
        {
            if (GameObject.FindGameObjectWithTag("PlayerSelf") != null)
            {
                GameObject.FindGameObjectWithTag("PlayerSelf").GetComponent<JumpAndRunMovement>().CheckWon();
                StartCoroutine(returnToSelect());
                totalPlayers = 100;
            }
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

    public void endGame(){
        PlayMSX(0);
        PhotonNetwork.Disconnect();
        AssignClientCharacter(0);
        SceneManager.LoadScene("CharacterSelect");
    }

    public void GameStarts(int myTotalPlayers)
    {
        Debug.Log("Game Starting with " + myTotalPlayers + " players.");
        totalPlayers = myTotalPlayers;
    }

    public void Idied()
    {
        totalPlayers--;
    }

    IEnumerator returnToSelect()
    {
        yield return new WaitForSeconds(3f);
        PhotonNetwork.Disconnect();
        AssignClientCharacter(0);
        SceneManager.LoadScene("CharacterSelect");
    }

    public void PlaySFX(int num)
    {
        mySFXAudio.Stop();
        mySFXAudio.clip = SFX[num];
        mySFXAudio.Play();
    }

    public void PlayMSX(int num)
    {
        myMusicAudio.Stop();
        myMusicAudio.clip = MSX[num];
        myMusicAudio.Play();
    }

    public void SetSFXVolume(float amt)
    {
        if (amt < 0f || amt > 1f)
        {
            Debug.LogError("Volume amount must be betweeen 0 and 1.");
        }
        mySFXAudio.volume = amt;
        Debug.Log("SFXVolume Set");
    }

    public void SetMSXVolume(float amt)
    {
        if (amt < 0f || amt > 1f)
        {
            Debug.LogError("Volume amount must be betweeen 0 and 1.");
        }
        myMusicAudio.volume = amt;
        Debug.Log("MSXVolume Set");
    }

    public void SetServer(bool isEast)
    {
        this.isEast = isEast;
    }

    public bool GetServerIsEast()
    {
        return isEast;
    }
}
