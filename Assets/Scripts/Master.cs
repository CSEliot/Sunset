using UnityEngine;
using UnityEngine.UI;
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
    public int PlayerCharNum;
    public int InRoomNumber;

    private string RoomName;

    public AudioClip[] SFX;
    public AudioClip[] MSX;
    private AudioSource myMusicAudio;
    private AudioSource mySFXAudio;

    private int maxPlayers;
    private int currentlyOnline;

    private bool isEast;

    private bool isControlsShown;

    private string version;

    public Sprite[] UIHeads;
    public Image PlayerHead;

    public GameObject[] MenuCanvasList;
    // 0 = main
    // 1 = map
    // 2 = character select
    // 3 = Options

    private MatchHUD matchHUD;

    public Text VersionUI;
    public GameObject LoadingUI;
    public GameObject Rays;

	private enum menu
	{
		main,
		map,
		chara,
		options,
		practice,
		ingame // opens pause menu
	};
	private menu currentMenu;

    private enum map
    {
        inmenu, //NOT USED. 
        practice,
        pillar,
        _void,
        lair
    };
    private map currentMap;

	private bool isNewScene;
    //	private  targetScene;

    private ConnectAndJoinRandom n;

    private float timeConnecting;
    private float connectingWaitTime;


    void Awake()
    {

        n = GameObject.FindGameObjectWithTag("Networking").GetComponent<ConnectAndJoinRandom>();
        
        version = Application.version;
        VersionUI.text = "BETA " + Application.version;
		currentMenu = menu.main;
        currentMap = map.pillar;

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

        //DontDestroyOnLoad(gameObject); Disabling, we never leave the scene Master is born in.
        AssignPlayerCharacter(0);
        //Cursor.lockState = CursorLockMode.Confined;
        //Cursor.visible = false;
        

        connectingWaitTime = 60; //Seconds
    }

    // Use this for initialization
    void Start()
    {
        maxPlayers = 100;
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
		switch (currentMenu) {
		    case menu.main:
			    PlayerPrefs.Save ();
			    Application.Quit ();
			    break; 
		    case menu.chara:
                if (currentMap == map.practice)
                {
                    GoTo(3);
                    SetRoomName("Pillar"); //reset level from practice level back to 0: Pillar.
                    break;
                }
                currentMenu = menu.map;
                switchCanvas((int)menu.map);
                switchOutGame();
                break;
		    case menu.map: 
			    currentMenu = menu.main;
			    switchCanvas ((int)menu.main);
                n.LeaveServer();
			    break;
		    case menu.practice:
			    currentMenu = menu.options;
                loadMenu();
			    break;
		    case menu.ingame:
                currentMenu = menu.chara;
                loadMenu();
                //PhotonNetwork.room.customProperties["GameStarted"] = false;
                GameObject.FindGameObjectWithTag("GameCam").SetActive(false);
                break;
            case menu.options:
                currentMenu = menu.main;
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
                n.JoinServer(true);
                timeConnecting = Time.time;
                StartCoroutine(gotoMapHelper());
			    break;
		    case (int)menu.chara:
                n.JoinRoom();
			    switchCanvas ((int)menu.chara);
                switchInGame();
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
                unloadMenu();
                matchHUD.MatchCamera.SetActive(true);
                break;
            case -1:
                switchCanvas(-1);
                break;
            default:
			    break;
		    }
	}
    
    /// <summary>
    /// Waits for the connection to finish. If more than @connectingWaitTime passes
    /// and there's no connection, we return to the previous screen.
    /// </summary>
    /// <returns></returns>
    private IEnumerator gotoMapHelper()
    {
        ToggleConnectLoadScreen(true);
        GoTo(-1);
        while (!PhotonNetwork.connectedAndReady)
        {
            if (Time.time - timeConnecting > connectingWaitTime)
            {
                GoTo(0);
                n.JoinServer(false);
                break;
            }else
            {
                yield return null;
            }
        }
        if (PhotonNetwork.connectedAndReady)
        {
            AssignPlayerCharacter(0);
            switchCanvas((int)menu.map);
        }
        ToggleConnectLoadScreen(false);
    }

	private void switchCanvas( int switchTo){
		currentMenu = (menu)switchTo;
		MenuCanvasList [0].SetActive (switchTo == 0 ? true : false); //main
		MenuCanvasList [1].SetActive (switchTo == 1 ? true : false); //map select
		MenuCanvasList [2].SetActive (switchTo == 2 ? true : false); //char select
        MenuCanvasList [3].SetActive (switchTo == 3 ? true : false); //options
    }

    private void switchInGame()
    {
        SceneManager.LoadScene((int)currentMap, LoadSceneMode.Additive);
        PlayMSX(2);
    }

    private void switchOutGame()
    {
        SceneManager.UnloadScene(SceneManager.GetSceneAt(1));
        PlayMSX(3);
    }

    private void unloadMenu()
    {
        GameObject[] menuObjs = SceneManager.GetSceneByName("MainMenu").GetRootGameObjects();
        for (int i = 0; i < menuObjs.Length; i++)
        {
            menuObjs[i].SetActive(false);
        }
        gameObject.SetActive(true); //Master gets disabled in this general sweep, we don't want that.
        n.gameObject.SetActive(true); // Same w/ Networking.
    }

    private void loadMenu()
    {
        PlayMSX(3);
        GameObject[] menuObjs = SceneManager.GetSceneByName("MainMenu").GetRootGameObjects();
        for (int i = 0; i < menuObjs.Length; i++)
        {
            menuObjs[i].SetActive(true);
        }
        switchCanvas((int)currentMenu);
    }

    public void AssignPlayerCharacter(int chosenChar)
    {
        clientCharacter = AllCharacters[chosenChar];
        PlayerCharNum = chosenChar;
    }

    public string GetClientCharacterName()
    {
        Debug.Log("Sending: " + clientCharacter.name);
        return clientCharacter.name;
    }

    public Dictionary<string, float> GetStrengthList()
    {
        return NameStrengthDict;
    }

    public void GameStarts(int myMaxPlayers)
    {
        Debug.Log("Game Starting with " + myMaxPlayers + " players.");
        maxPlayers = myMaxPlayers;
    }

    public void Idied()
    {
        maxPlayers--;
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

    public void SetRoomName(string room_name)
    {
        RoomName = room_name;
        switch (room_name)
        {
            case "Practice":
                currentMap = map.practice;
                break;
            case "Pillar":
                currentMap = map.pillar;
                break;
            case "Void":
                currentMap = map._void;
                break;
            case "Lair":
                currentMap = map.lair;
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

    public int CurrentMenu
    {
        get
        {
            return (int)currentMenu;
        }
    }

    public int CurrentMap
    {
        get
        {
            return (int)currentMap;
        }
    }

    public void assignMatchHUD()
    {
        matchHUD = GameObject.FindGameObjectWithTag("MatchHUD").GetComponent<MatchHUD>();
    }

    private void ToggleConnectLoadScreen(bool isActive)
    {
        LoadingUI.SetActive(isActive);
        Rays.SetActive(!isActive);
        if (isActive)
            myMusicAudio.Pause();
        else
            myMusicAudio.Play();
    }


}
