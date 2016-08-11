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

	public enum Menu
	{
		main,
		map,
		chara,
		options,
		practice,
		ingame // opens pause menu
	};
	private Menu currentMenu;

    public enum Map
    {
        inmenu, //NOT USED. 
        practice,
        pillar,
        _void,
        lair
    };
    private Map currentMap;

    public enum RoomAction
    {
        unset,
        join,
        create
    };
    private RoomAction rmAction;

    /// <summary>
    /// Should go up whenever a new map is added. Currently there are: 3
    /// </summary>
    private const int totalUniqueArenas = 3;
    private string[] arenaNames;

	private bool isNewScene;
    //	private  targetScene;

    public ConnectAndJoinRandom N;
    public MapSelectUIController MapUI;

    private float timeConnecting;
    private float connectingWaitTime;

    void Awake()
    {

        N = GameObject.FindGameObjectWithTag("Networking").GetComponent<ConnectAndJoinRandom>();
        
        version = Application.version;
        VersionUI.text = "BETA " + Application.version;
		currentMenu = Menu.main;
        currentMap = Map.pillar;
        rmAction = RoomAction.unset;

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
        
        arenaNames = new string[totalUniqueArenas] { "Pillar", "Void", "Lair" };

        connectingWaitTime = 7; //Seconds
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
		    case Menu.main:
			    PlayerPrefs.Save ();
			    Application.Quit ();
			    break; 
		    case Menu.chara:
                if (currentMap == Map.practice)
                {
                    GoTo(3);
                    SetRoomName("Pillar"); //reset level from practice level back to 0: Pillar.
                    break;
                }
                currentMenu = Menu.map;
                switchCanvas((int)Menu.map);
                unloadArena();
                N.LeaveRoom();
                break;
		    case Menu.map: 
			    currentMenu = Menu.main;
			    switchCanvas ((int)Menu.main);
                N.LeaveServer();
			    break;
		    case Menu.practice:
			    currentMenu = Menu.options;
                loadMenu();
			    break;
		    case Menu.ingame:
                currentMenu = Menu.chara;
                PlayMSX(0);
                loadMenu();
                matchHUD.MatchCamera.SetActive(false);
                break;
            case Menu.options:
                currentMenu = Menu.main;
                switchCanvas((int)Menu.main);
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
		    case (int)Menu.main:
			    switchCanvas ((int)Menu.main);
			    break;
		    case (int)Menu.map:
                N.JoinServer(true);
                StartCoroutine(gotoMapHelper());
			    break;
		    case (int)Menu.chara:
                if(rmAction == RoomAction.unset)
                {
                    Debug.LogError("No room action given! Create or Join?");
                }else if(rmAction == RoomAction.join)
                {
                    N.JoinRoom();
                }else
                {
                    N.CreateRoom();
                }
                StartCoroutine(gotoCharHelper());
                break; 
            case (int)Menu.options:
                switchCanvas((int)Menu.options);
                break;
            case (int)Menu.practice:
                switchCanvas((int)Menu.chara);
                SetRoomName("Practice");
                break;
            case (int)Menu.ingame:
                switchCanvas((int)Menu.ingame);
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
        timeConnecting = Time.time;
        ToggleConnectLoadScreen(true);
        GoTo(-1);
        while (!PhotonNetwork.connectedAndReady)
        {
            if (Time.time - timeConnecting > connectingWaitTime)
            {
                GoTo(0);
                N.JoinServer(false);
                break;
            }else
            {
                yield return null;
            }
        }
        if (PhotonNetwork.connectedAndReady)
        {
            AssignPlayerCharacter(0);
            switchCanvas((int)Menu.map);
        }
        ToggleConnectLoadScreen(false);
    }

    private IEnumerator gotoCharHelper()
    {
        timeConnecting = Time.time;
        ToggleConnectLoadScreen(true);
        GoTo(-1);
        while (!PhotonNetwork.inRoom)
        {
            if (Time.time - timeConnecting > connectingWaitTime)
            {
                GoTo(1);
                MapUI.FullRoomWarning.SetActive(true);
                break;
            }
            else
            {
                yield return null;
            }
        }
        if (PhotonNetwork.inRoom)
        {
            switchCanvas((int)Menu.chara);
            loadArena(); //the InGame map is loaded in the background.
        }
        ToggleConnectLoadScreen(false);
    }



    private void switchCanvas( int switchTo){
        currentMenu = (Menu)switchTo;
        MenuCanvasList[0].SetActive(switchTo == 0 ? true : false); //main
        MenuCanvasList[1].SetActive(switchTo == 1 ? true : false); //map select
        MenuCanvasList[2].SetActive(switchTo == 2 ? true : false); //char select
        MenuCanvasList[3].SetActive(switchTo == 3 ? true : false); //options
    }

    private void loadArena()
    {
        SceneManager.LoadScene((int)currentMap, LoadSceneMode.Additive);
    }

    private void unloadArena()
    {
        SceneManager.UnloadScene(SceneManager.GetSceneAt(1));
    }

    private void unloadMenu()
    {
        GameObject[] menuObjs = SceneManager.GetSceneByName("MainMenu").GetRootGameObjects();
        for (int i = 0; i < menuObjs.Length; i++)
        {
            menuObjs[i].SetActive(false);
        }
        gameObject.SetActive(true); //Master gets disabled in this general sweep, we don't want that.
        N.gameObject.SetActive(true); // Same w/ Networking.
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
                currentMap = Map.practice;
                break;
            case "Pillar":
                currentMap = Map.pillar;
                break;
            case "Void":
                currentMap = Map._void;
                break;
            case "Lair":
                currentMap = Map.lair;
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

    public int TotalUniqueArenas
    {
        get
        {
            return totalUniqueArenas;
        }
    }

    public string[] ArenaNames
    {
        get
        {
            return arenaNames;
        }
    }

    public RoomAction RmAction
    {
        get
        {
            return rmAction;
        }

        set
        {
            rmAction = value;
        }
    }

    public void SetRoomAction(int action)
    {
        rmAction = (RoomAction)action;
    }

    public void assignMatchHUD()
    {
        Debug.Log("Match HUD assigned.");
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
