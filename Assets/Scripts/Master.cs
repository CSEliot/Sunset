using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// General handler & storer, slowly being phased out.
/// Handles scene management and everything that's inter-scene. This includes:
///  - Music
///  - Background Animations
///  - Menu SFX
///  - gameplay/instance data
/// </summary>
public class Master : MonoBehaviour
{
    public GameObject MainCameraRef;

    [System.Serializable]
    public class NameToStrength
    {
        public string Name;
        public float Power;
    }

    public NameToStrength[] StrengthsList;
    private Dictionary<string, float> NameStrengthDict;
    public GameObject[] AllCharacters;
    public int PlayableCharacters;

    public GameObject[] DontUnloadObjs;

    private GameObject clientCharacter;
    public int PlayerCharNum;

    private string stageName;

    public AudioClip[] SFX;
    public AudioClip[] MSX;
    private AudioSource myMusicAudio;
    private AudioSource mySFXAudio;

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
    
    public Text VersionUI;
    public GameObject LoadingUI;
    public GameObject Rays;

    private int charactersUnlockedTotal;
    private int defaultUnlocked = 6;

	public enum Menu
	{
		main,
		map,
		chara,
		options,
		ingame,
        practice
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
    private const int totalUniqueStages = 3;
    private string[] stageNames;

	private bool isNewScene;
    //	private  targetScene;

    public NetworkManager N;
    public MapSelectUIController MapUI;

    private float timeConnecting;
    public float ConnectToRoomMaxWaitTime;
    public float ConnectToServerMaxWaitTime;

    private bool escapeHardened; //A hardened escape requires 3 presses.
    private bool escapeDisabled;
    private int escapePresses;

    public bool IsOfflineMode;

    private bool isFirstTime;
    public GameObject FirstTimeNotice;

    void Awake()
    {
        version = Application.version;
        CBUG.Do("Application Version is: " + version + ".");
		currentMenu = Menu.main;
        currentMap = Map.pillar;
        rmAction = RoomAction.unset;
        
        stageName = "Pillar";
        
        isEast = true;
        //if (Application.isEditor)
        //    PlayerPrefs.DeleteAll();
        bool tempControlsShown = PlayerPrefs.GetInt("isControlsShown", 1) == 1 ? true : false;

        isFirstTime = PlayerPrefs.GetInt("isFirstTime", 1) == 1? true : false;
        FirstTimeNotice.SetActive(isFirstTime);

        isControlsShown = tempControlsShown;

        myMusicAudio = GetComponent<AudioSource>();
        mySFXAudio = transform.GetChild(0).GetComponent<AudioSource>();
        PlayMSX(3);
        if (SceneManager.GetActiveScene().name.Contains("Practice"))
        {
            PlayMSX(0);
            isFirstTime = false;
            PlayerPrefs.SetInt("isFirstTime", 0);
        }
        NameStrengthDict = new Dictionary<string, float>();
        foreach (NameToStrength character in StrengthsList)
        {
            NameStrengthDict.Add(character.Name, character.Power);
        }

        //DontDestroyOnLoad(gameObject); Disabling, we never leave the scene Master is born in.
        AssignPlayerCharacter(0);
        //Cursor.lockState = CursorLockMode.Confined;
        //Cursor.visible = false;
        
        stageNames = new string[totalUniqueStages] { "Pillar", "Void", "Lair" };

        if (IsOfflineMode)
            return;
        VersionUI.text = "BETA " + Application.version;
        N = GameObject.FindGameObjectWithTag("Networking").GetComponent<NetworkManager>();
    }

    // Use this for initialization
    void Start()
    {
        charactersUnlockedTotal = PlayerPrefs.GetInt("UnlockedChars", defaultUnlocked); // 6 = Default usable characters.
        SetMSXVolume(PlayerPrefs.GetFloat("MSXVol", 0.5f));
        SetSFXVolume(PlayerPrefs.GetFloat("SFXVol", 0.5f));

        escapeHardened = false;
        escapeDisabled = false;
        if (IsOfflineMode)
            return;
        CBUG.Print(VersionUI.text);
        //_Audio.Play(10);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("escape")) {
            escapePresses++;
        }

        if (escapeDisabled)
            return;

        if(escapePresses == 1 && !escapeHardened)
        {
			GoBack ();
            escapePresses = 0;
        }else if(escapePresses == 3) {
            GoBack();
            escapePresses = 0;
            //ACTIVATE "ARE YOU SURE YOU WANT TO QUIT?"
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
                    SetStageName("Pillar"); //reset level from practice level back to 0: Pillar.
                    break;
                }
                currentMenu = Menu.map;
                switchCanvas((int)Menu.map);
                unloadStage();
                N.LeaveRoom();
                break;
		    case Menu.map: 
			    currentMenu = Menu.main;
			    switchCanvas ((int)Menu.main);
                N.LeaveServer();
			    break;
		    case Menu.ingame:
                //Leaving after game started means GOTO->Map Select, so unload stage.
                if (GameManager.GameStarted) {
                    currentMenu = Menu.map;
                    loadMenu();
                    EscapeHardened = true;
                    switchCanvas((int)Menu.map);
                    unloadStage();
                    N.LeaveRoom();
                }else {//Else we're just changing character.
                    currentMenu = Menu.chara;
                    loadMenu();
                    escapeHardened = false;
                    GameObject.FindGameObjectWithTag("StageCamera").GetComponent<AudioListener>().enabled = false;
                }
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
                StartCoroutine(gotoMap());
			    break;
		    case (int)Menu.chara:
                StartCoroutine(gotoCharacterSelect());
                break; 
            case (int)Menu.options:
                switchCanvas((int)Menu.options);
                break;
            case (int)Menu.ingame: //game is actually loaded up from map -> chara.
                //This just disables the main menu.
                switchCanvas((int)Menu.ingame);
                unloadMenu();
                PlayMSX(1);
                GameObject.FindGameObjectWithTag("StageCamera").GetComponent<AudioListener>().enabled = true;
                break;
            case (int)Menu.practice:
                loadStage();
                switchCanvas((int)Menu.ingame);
                unloadMenu();
                PlayMSX(1);
                break;
            case -1: //Disabling all UI
                switchCanvas(-1);
                break;
            default:
                CBUG.Error("BAD MENU SCENE GIVEN!! :: " + to);
			    break;
		    }
	}
    
    /// <summary>
    /// Waits for the connection to finish. If more than @connectingWaitTime passes
    /// and there's no connection, we return to the previous screen.
    /// </summary>
    /// <returns></returns>
    private IEnumerator gotoMap()
    {
        timeConnecting = Time.time;
        ToggleConnectLoadScreen(true);
        GoTo(-1);
        while (!PhotonNetwork.connectedAndReady)
        {
            if (Time.time - timeConnecting > ConnectToServerMaxWaitTime)
            {
                GoTo(0);
                ToggleConnectLoadScreen(false);
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
            _Audio.Play(12); //Say "Pillar"
        }
    }

    private IEnumerator gotoCharacterSelect()
    {
        if (rmAction == RoomAction.unset)
        {
            CBUG.Error("No room action given! Create or Join?");
        }
        else if (rmAction == RoomAction.join)
        {
            N.JoinRoom();
        }
        else
        {
            N.CreateRoom();
        }
        timeConnecting = Time.time;
        ToggleConnectLoadScreen(true);
        GoTo(-1);
        while (!PhotonNetwork.inRoom)
        {
            if (Time.time - timeConnecting > ConnectToRoomMaxWaitTime)
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
            loadStage(); //the InGame map is loaded in the background.
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

    private void loadStage()
    {
        SceneManager.LoadScene((int)currentMap, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneAt(1));
    }

    private void unloadStage()
    {
        SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(1));
    }

    private void unloadMenu()
    {
        GameObject[] menuObjs = SceneManager.GetSceneByName("MainMenu").GetRootGameObjects();
        for (int i = 0; i < menuObjs.Length; i++)
        {
            menuObjs[i].SetActive(false);
        }
        //Certain OBJs must never be set inactive.
        for(int i = 0; i < DontUnloadObjs.Length; i++){
            DontUnloadObjs[i].SetActive(true);
        }
    }

    private void loadMenu()
    {
        PlayMSX(0);
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
        CBUG.Log("Sending: " + clientCharacter.name);
        return clientCharacter.name;
    }

    public Dictionary<string, float> GetStrengthList()
    {
        return NameStrengthDict;
    }

    public void GameStarts(int roomSize)
    {
       CBUG.Log("Game Starting with " + roomSize + " players.");
        escapeDisabled = false;
        escapeHardened = true;
    }

    public void NewGame()
    {
        escapeHardened = false;
        GoBack();
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

    #region Getters and Setters
    public void SetSFXVolume(float amt)
    {
        if (amt < 0f || amt > 1f)
        {
            CBUG.Error("Volume amount must be betweeen 0 and 1.");
        }
        mySFXAudio.volume = amt;
        PlayerPrefs.SetFloat("SFXVol", amt);
        PlayerPrefs.Save();
        CBUG.Log("SFXVolume Set");
    }

    public void SetMSXVolume(float amt)
    {
        if (amt < 0f || amt > 1f)
        {
            CBUG.Error("Volume amount must be betweeen 0 and 1.");
        }
        myMusicAudio.volume = amt;
        PlayerPrefs.SetFloat("MSXVol", amt);
        PlayerPrefs.Save();
        CBUG.Log("MSXVolume Set");
    }

    public static float GetSFXVolume()
    {
        return GameObject.FindGameObjectWithTag("Master").transform.GetChild(0).GetComponent<AudioSource>().volume;
    }

    public void SetStageName(string stage_name)
    {
        stageName = stage_name;
        switch (stage_name)
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
                CBUG.Error("WRONG ROOMMNAME GIVEN.");
                break;
        }
    }

    public string GetStageName()
    {
        return stageName;
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

    public Menu CurrentMenu
    {
        get
        {
            return currentMenu;
        }
    }

    public int CurrentMap
    {
        get
        {
            return (int)currentMap;
        }
    }

    public int TotalUniqueStages
    {
        get
        {
            return totalUniqueStages;
        }
    }

    public string[] StageNames
    {
        get
        {
            return stageNames;
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

    public int CharactersUnlockedTotal
    {
        get
        {
            return charactersUnlockedTotal;
        }
    }

    public bool EscapeHardened
    {
        get
        {
            return escapeHardened;
        }

        set
        {
            escapeHardened = value;
        }
    }

    public bool EscapeDisabled {
        get {
            return escapeDisabled;
        }

        set {
            escapeDisabled = value;
        }
    }

    public void SetRoomAction(int action)
    {
        rmAction = (RoomAction)action;
    }
    #endregion

    public void ToggleConnectLoadScreen(bool isActive)
    {
        LoadingUI.SetActive(isActive);
        Rays.SetActive(!isActive);
        if (isActive)
            myMusicAudio.Pause();
        else
            myMusicAudio.Play();
    }

    void OnApplicationQuit()
    {
        isFirstTime = false;
        PlayerPrefs.SetInt("isFirstTime", 0);
    }
}
