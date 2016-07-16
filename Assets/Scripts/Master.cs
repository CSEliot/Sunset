using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Handles scene management and everything that's inter-scene. This includes:
///  - Music
///  - Background Animations
///  - Menu SFX
///  - gameplay/instance data
/// </summary>
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

    private bool isControlsShown;

    private string version;

	public GameObject[] MenuCanvasList;
	// 0 = main
	// 1 = map
	// 2 = character select
	// 3 = Options


	private enum menu
	{
		main,
		map,
		chara,
		options,
		practice,
		ingame // opens pause menu
	};
	private menu currentScene;

    private enum map
    {
        inmenu, //NOT USED. 
        practice,
        pillar,
        _void,
        heck
    };
    private map currentLevel;

	private bool isNewScene;
	//	private  targetScene;


    void Awake()
    {
        
        version = Application.version;
		currentScene = menu.main;
        currentLevel = map.pillar;

        RoomName = "Pillar";
        
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
        if (Input.GetKeyDown("escape"))
        {
			GoBack ();
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

	/// <summary>
	/// The main menu, (title, character select and map select) is all one
	/// scene but the master class doesn't discern between them.
	/// 
	/// This is for when hitting a phone's back button or in-game back
	/// button. Going Back FROM in-game scene to menu or menu to menu.
	/// </summary>
	public void GoBack()
	{
		switch (currentScene) {
		    case menu.main:
			    PlayerPrefs.Save ();
			    Application.Quit ();
			    break; 
		    case menu.chara:
                if (currentLevel == map.practice)
                {
                    GoTo(3);
                    SetRoomName("Pillar"); //reset level from practice level back to 0: Pillar.
                    break;
                }
			    currentScene = menu.map;
			    switchCanvas ((int)menu.map);
			    break;
		    case menu.map: 
			    currentScene = menu.main;
			    switchCanvas ((int)menu.main);
			    break;
		    case menu.practice:
			    currentScene = menu.options;
                loadMenu();
			    break;
		    case menu.ingame:
                currentScene = menu.chara;
                loadMenu();
                PhotonNetwork.Disconnect();
                PlayMSX(3);
                SceneManager.UnloadScene(SceneManager.GetSceneAt(1));
                break;
            case menu.options:
                currentScene = menu.main;
                switchCanvas((int)menu.main);
                break;
            default:
			    break;
		}
	}

	/// <summary>
	/// To other Menu Canvas "Scenes" or to an in-game scene.
	/// </summary>
	/// <param name="to">To.</param>
	public void GoTo(int to)
	{
		switch (to) {
		    case (int)menu.main:
			    switchCanvas ((int)menu.main);
			    break;
		    case (int)menu.map:
			    AssignClientCharacter (0);
			    switchCanvas ((int)menu.map);
			    break;
		    case (int)menu.chara:
			    switchCanvas ((int)menu.chara);
			    break; 
            case (int)menu.options:
                switchCanvas((int)menu.options);
                break;
            case (int)menu.practice:
                switchCanvas((int)menu.chara);
                SetRoomName("Practice");
                break;
            case (int)menu.ingame:
                switchCanvas((int)menu.ingame);
                switchInGame();
                break;
            default:
			    break;
		    }
	}

	private void switchCanvas( int switchTo){
		currentScene = (menu)switchTo;
		MenuCanvasList [0].SetActive (switchTo == 0 ? true : false); //main
		MenuCanvasList [1].SetActive (switchTo == 1 ? true : false); //map select
		MenuCanvasList [2].SetActive (switchTo == 2 ? true : false); //char select
        MenuCanvasList [3].SetActive (switchTo == 3 ? true : false); //options
    }

    private void switchInGame()
    {
        SceneManager.LoadScene((int)currentLevel, LoadSceneMode.Additive);
        unloadMenu();
    }

    private void unloadMenu()
    {
        GameObject[] menuObjs = SceneManager.GetSceneByName("MainMenu").GetRootGameObjects();
        for (int i = 0; i < menuObjs.Length; i++)
        {
            menuObjs[i].SetActive(false);
        }
    }

    private void loadMenu()
    {
        PlayMSX(3);
        GameObject[] menuObjs = SceneManager.GetSceneByName("MainMenu").GetRootGameObjects();
        for (int i = 0; i < menuObjs.Length; i++)
        {
            menuObjs[i].SetActive(true);
        }
        switchCanvas((int)currentScene);
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
		myMusicAudio.time = (num == 3) ? 8 : 0; //Song 4 starts at 8, post intro.
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
        switch (room_name)
        {
            case "Practice":
                currentLevel = map.practice;
                break;
            case "Pillar":
                currentLevel = map.pillar;
                break;
            case "Void":
                currentLevel = map._void;
                break;
            case "Heck":
                currentLevel = map.heck;
                break;
            default:
                Debug.LogError("WRONG ROOMMNAME GIVEN.");
                break;
        }
            
    }

    public string GetRoomName()
    {
        return RoomName;
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

    public int CurrentLevel
    {
        get
        {
            return (int)currentLevel;
        }
    }

    private void disableSplash()
    {

        GameObject.Find("IntroBG").SetActive(false);
        myMusicAudio.time = 8;
        //GameObject.Find("Canvas").transform.GetChild(3).gameObject.SetActive(true);
    }


}
