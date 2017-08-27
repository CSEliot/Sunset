using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// All Pre-game, Post-Character Select UI code. 
/// Dependency on net code and Photon minimized room size calls.
/// </summary>
public class WaitGUIController : MonoBehaviour{

    private bool isReady;
    public Color rdyColor; //Unfaded
    public Color unRdyColor; //Faded

    private Master M;
    private NetworkManager N;

    public Sprite Empty;

    public Text PercentReady;

    public Image[] RoomSlotImages;

    private int readyUpBypassCount;
    private int readyUpBypassTotal;

    private AudioListener stageListener;

    public GameObject ReadyGUI;
    public GameObject YesButton;
    public GameObject NoButton;
    public GameObject BackButton;

    public Text roomName;

    public GameObject Warning;

    private int headSprite;
    public Sprite[] UIHeads;
    public Image PlayerHead;

    public GameObject CountdownObj;
    public Text countdownTextTop;
    public Text countdownTextBottom;

    private bool isPostStartSpectator;

    // Use this for initialization
    void Start () {

        tag = "WaitGUI";

        readyUpBypassCount = 0;
        readyUpBypassTotal = 2;
        M = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
        N = GameObject.FindGameObjectWithTag("Networking").GetComponent<NetworkManager>();
        N.WaitGUI = GetComponent<WaitGUIController>();

        isReady = false;
        headSprite = getImageNum();

        PercentReady.text = "" + N.ReadyTotal + "/" + N.GetInRoomTotal;

        roomName.text = N.CurrentRoom.Name;
        isPostStartSpectator = false;
        if (N.GameStarted) {
            activateSpectatingMode();
            isPostStartSpectator = true;
        }

        rdyColor = new Color(1f, 1f, 1f, 1f);
        unRdyColor = new Color(1f, 1f, 1f, 0.5f);

        stageListener = GameObject.FindGameObjectWithTag("StageCamera").GetComponent<AudioListener>();
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown("p"))
        {
            readyUpBypassCount++;
        }
        if (readyUpBypassTotal < readyUpBypassCount || N.StartTheMatch)
        {
            startMatch();   
        }


        if (N.GameStarted)
            return;

        if(N.GetPlayerStateChange())
            updatePlayerLoginDisplay();

        if (N.GetRdyStateChange())
            updateReadyStatusDisplay();

        if (N.GetCharStateChange())
            updatePlayerCharDisplay();

        updateCountdownDisplay(N.StartCountdown, N.StartTimer);
	}

    #region UI Unity Calls - Called By UI
    public void ReadyUp() 
    {
        if (N.GetInRoomTotal < 2)
        {   
            Warning.SetActive(true);
            return;
        }
        isReady = true;
        N.ReadyButton();
        YesButton.SetActive(false);
        NoButton.SetActive(true);
        M.PlaySFX(1);
    }

    public void UnReady()
    {
        isReady = false;
        N.UnreadyButton();
        YesButton.SetActive(true);
        NoButton.SetActive(false);
        M.PlaySFX(1);
    }
    #endregion

    public void UnReadyUI()
    {
        YesButton.SetActive(true);
        NoButton.SetActive(false);
        M.PlaySFX(1);
    }

    public void Back()
    {

        N.SetCharacter();
    }

    #region Network Activity Sensitive
    private void updateReadyStatusDisplay()
    {
        PercentReady.text = "" + N.ReadyTotal + "/" + N.GetInRoomTotal;

        //Any change to the ready status requires everyone to re-ready up.
        //Yes.SetActive(true);
        //No.SetActive(false);

        int playerNumber;
        for (int x = 0; x < PhotonNetwork.room.PlayerCount; x++)
        {
            playerNumber = NetIDs.PlayerNumber(PhotonNetwork.playerList[x].ID);
            RoomSlotImages[playerNumber].color = N.GetRdyStatus(PhotonNetwork.playerList[x].ID) ? rdyColor : unRdyColor;
        }
    }

    private void updatePlayerCharDisplay()
    {
        int slotNum;
        int charNum;
        for (int x = 0; x < PhotonNetwork.room.PlayerCount; x++)
        {
            slotNum = NetIDs.PlayerNumber(PhotonNetwork.playerList[x].ID);
            charNum = N.GetCharNum(PhotonNetwork.playerList[x].ID);
            RoomSlotImages[slotNum].sprite = UIHeads[charNum];
            CBUG.Log(string.Format("UpdateHUDCharDisplay playerID/slotNum: {0} charNum: {1}", slotNum, charNum));
        }
    }

    private void updatePlayerLoginDisplay()
    {
        bool isActive = true;
        
        for(int i = 0; i < NetworkManager.ROOM_CAP; i++)
        {
            if (i == PhotonNetwork.playerList.Length)
                isActive = false;
            RoomSlotImages[i].transform.gameObject.SetActive(isActive);
        }
    }

    private void updateCountdownDisplay(bool isActive, int secRemaining)
    {
        string timeString = secRemaining.ToString();
        CountdownObj.SetActive(isActive);
        countdownTextBottom.text = timeString;
        countdownTextTop.text = timeString;

        if(secRemaining == 1)
        {
            M.EscapeDisabled = true;
            //on the last second, disable the ability to leave
            NoButton.SetActive(false);
            BackButton.SetActive(false);
        }
    }
    #endregion

    private void activateSpectatingMode()
    {
        YesButton.SetActive(false);
        NoButton.SetActive(false);

        transform.GetChild(0).gameObject.SetActive(true);
        ReadyGUI.GetComponent<Text>().text = "Spectating . . .";
        PercentReady.text = "";
        for (int i = 0; i < RoomSlotImages.Length; i++)
        {
            RoomSlotImages[i].transform.gameObject.SetActive(false);
        }

        CamManager.SetTarget(null);
    }


    public void CheatGame()
    {
        readyUpBypassCount = 3;
    }

    public static void ActivateSpectatingMode()
    {
        getRef().activateSpectatingMode();
    }

    private static WaitGUIController getRef()
    {
        return GameObject.FindGameObjectWithTag("WaitGUI").GetComponent<WaitGUIController>();
    }

    private int getImageNum()
    {
        for (int i = 0; i < UIHeads.Length; i++)
        {
            if (i == M.PlayerCharNum)
            {
                return i;
            }
        }
        CBUG.Error("No Head Found!");
        return -1;
    }

    private Sprite getImage(int num)
    {
        return UIHeads[num];
    }

    private void startMatch()
    {
        CBUG.Do("WAIT UI STARTING MATCH!");
        //j.GameStarted(N.GetSlotNum(PhotonNetwork.player.ID));
        transform.GetChild(0).gameObject.SetActive(false);
        PlayerHead.sprite = getImage(getImageNum());
    }
}
