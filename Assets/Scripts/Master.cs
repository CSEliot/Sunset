using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class Master : MonoBehaviour
{

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

    private string RoomName;

    public AudioClip[] SFX;
    public AudioClip[] MSX;
    private AudioSource myMusicAudio;
    private AudioSource mySFXAudio;

    private int totalPlayers;

    private bool isEast;

    private bool isTestMode;

    private bool isControlsShown;

    private string version;

	private GameObject[] canvasScenes;
	private int currentScene;

	private int isNewScene;
	private int targetScene;

    void Awake()
    {

		canvasScenes = new GameObject[3];
		canvasScenes [0] = GameObject.Find ("MenuCanvas");
		canvasScenes [1] = GameObject.Find ("CharacterSelectCanvas");
		canvasScenes [2] = GameObject.Find ("MapSelectCanvas");

        version = Application.version;

        RoomName = "Pillar";

        isTestMode = false;
        isEast = true;
        bool tempControlsShown = PlayerPrefs.GetInt("isControlsShown", 1) == 1 ? true : false;
        isControlsShown = tempControlsShown;

        myMusicAudio = GetComponent<AudioSource>();
        mySFXAudio = transform.GetChild(0).GetComponent<AudioSource>();
        PlayMSX(3);
        NameStrengthDict = new Dictionary<string, float>();
        foreach (NameToStrength character in StrengthsList)
        {
            NameStrengthDict.Add(character.Name, character.Power);
        }

        if (GameObject.FindGameObjectsWithTag("Master").Length > 1)
        {
            Destroy(gameObject);
            disableSplash();
        }

        DontDestroyOnLoad(gameObject);
        AssignClientCharacter(0);
        //Cursor.lockState = CursorLockMode.Confined;
        //Cursor.visible = false;
    }

    // Use this for initialization
    void Start()
    {
        totalPlayers = 100;
        SetMSXVolume(PlayerPrefs.GetFloat("MSXVol", 0.5f));
        SetSFXVolume(PlayerPrefs.GetFloat("SFXVol", 0.5f));
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown("escape") || isNewScene == 1)
        {
            if(SceneManager.GetActiveScene().name.Contains("Test"))
            {
                isTestMode = false;
                SceneManager.LoadScene("TitleScreen");
                PlayMSX(0);
            }
            else if (SceneManager.GetActiveScene().name.Contains("GameScreen"))
            {
                PhotonNetwork.Disconnect();
                AssignClientCharacter(0);
                SceneManager.LoadScene("CharacterSelect");
                PlayMSX(0);
            }
            else if (SceneManager.GetActiveScene().name.Contains("MapSelect"))
            {
                AssignClientCharacter(0);
                SceneManager.LoadScene("CharacterSelect");
                PlayMSX(0);
            }
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

    public void endGame()
    {
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
        yield return new WaitForSeconds(6f);
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
		myMusicAudio.time = 0;
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
        PlayerPrefs.SetFloat("SFXVol", amt);
        PlayerPrefs.Save();
        Debug.Log("SFXVolume Set");
    }

    public void SetMSXVolume(float amt)
    {
        if (amt < 0f || amt > 1f)
        {
            Debug.LogError("Volume amount must be betweeen 0 and 1.");
        }
        myMusicAudio.volume = amt;
        PlayerPrefs.SetFloat("MSXVol", amt);
        PlayerPrefs.Save();
        Debug.Log("MSXVolume Set");
    }

    public void SetServer(bool isEast)
    {
        this.isEast = true;
        //this.isEast = isEast;
    }

    public bool GetServerIsEast()
    {
        return true;
        //return isEast;
    }

    public void SetRoomName(string room_name)
    {
        RoomName = room_name;
    }

    public string GetRoomName()
    {
        return RoomName;
    }

    public bool IsTestMode
    {
        get
        {
            return isTestMode;
        }

        set
        {
            isTestMode = value;
        }
    }

    public bool IsControlsShown
    {
        get
        {
            
            return isControlsShown;
        }

        set
        {
            isControlsShown = value;
            int toShow = value ? 1 : 0;
            PlayerPrefs.SetInt("ShowControls", toShow);
            PlayerPrefs.Save();
        }
    }

    public string Version
    {
        get
        {
            return version;
        }

        set
        {
            version = value;
        }
    }

    private void disableSplash()
    {

        GameObject.Find("IntroBG").SetActive(false);
        myMusicAudio.time = 8;
        //GameObject.Find("Canvas").transform.GetChild(3).gameObject.SetActive(true);
    }
}
