using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// All Pre-game, Post-Character Select UI code. 
/// Dependency on net code and Photon minimized room size calls.
/// </summary>
public class WaitUIController : MonoBehaviour{

    private bool isReady;
    public Color rdyColor; //Unfaded
    public Color unRdyColor; //Faded

    private Master M;
    private NetworkManager N;

    public Sprite Empty;

    public Text PercentReady;

    public Image[] PlayerSlots;

    private int readyUpBypassCount;
    private int readyUpBypassTotal;

    private AudioListener stageListener;

    public Text ReadyText;
    public GameObject Yes;
    public GameObject No;

    public Text roomName;

    public GameObject Warning;

    private int headSprite;
    public Sprite[] UIHeads;
    public Image PlayerHead;

    public GameObject CountdownObj;
    public Text countdownTextTop;
    public Text countdownTextBottom;

    // Use this for initialization
    void Start () {

        tag = "WaitGUI";

        readyUpBypassCount = 0;
        readyUpBypassTotal = 2;
        M = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
        N = GameObject.FindGameObjectWithTag("Networking").GetComponent<NetworkManager>();

        isReady = false;
        headSprite = getImageNum();

        PercentReady.text = "" + N.ReadyTotal + "/" + N.GetInRoomTotal;

        roomName.text = N.CurrentRoom.name;
        if (N.GameStarted)
            _ActivateSpectatingMode();

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

    public void ReadyUp() 
    {
        if (N.GetInRoomTotal < 2)
        {   
            Warning.SetActive(true);
            return;
        }
        if (isReady)
        {
            return;
        }
        isReady = true;
        N.ReadyButton();
    }

    public void UnReady() {
        if (!isReady) {
            Back();
            return;        
        }
        isReady = false;
        N.UnreadyButton();
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

        int playerSlot;
        for (int x = 0; x < PhotonNetwork.room.playerCount; x++)
        {
            playerSlot = N.GetSlotNum(NetID.Convert(PhotonNetwork.playerList[x].ID));
            PlayerSlots[playerSlot].color = N.GetRdyStatus(NetID.Convert(PhotonNetwork.playerList[x].ID)) ? rdyColor : unRdyColor;
        }
    }

    private void updatePlayerCharDisplay()
    {
        int slotNum;
        int playerID;
        int charNum;
        for (int x = 0; x < PhotonNetwork.room.playerCount; x++)
        {
            playerID = NetID.Convert(PhotonNetwork.playerList[x].ID);
            slotNum = N.GetSlotNum(playerID);
            charNum = N.GetCharNum(playerID);
            PlayerSlots[slotNum].sprite = UIHeads[charNum];
            CBUG.Log(string.Format("UpdateHUDCharDisplay playerID: {0} slotNum: {1} charNum: {2}", playerID, slotNum, charNum));
        }
    }

    private void updatePlayerLoginDisplay()
    {
        bool isActive = true;
        
        for(int i = 0; i < Master.MaxRoomSize; i++)
        {
            if (i == PhotonNetwork.playerList.Length)
                isActive = false;
            PlayerSlots[i].transform.gameObject.SetActive(isActive);
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
            No.SetActive(false);
        }
    }
    #endregion

    private void _ActivateSpectatingMode()
    {
        Yes.SetActive(false);
        No.SetActive(false);

        ReadyText.text = "Spectating . . .";
        PercentReady.text = "";
        for (int i = 0; i < PlayerSlots.Length; i++)
        {
            PlayerSlots[i].transform.gameObject.SetActive(false);
        }

        CamManager.SetTarget(null);
    }


    public void CheatGame()
    {
        readyUpBypassCount = 3;
    }

    public static void ActivateSpectatingMode()
    {
        getRef()._ActivateSpectatingMode();
    }

    private static WaitUIController getRef()
    {
        return GameObject.FindGameObjectWithTag("WaitGUI").GetComponent<WaitUIController>();
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
        //j.GameStarted(N.GetSlotNum(PhotonNetwork.player.ID));
        transform.GetChild(0).gameObject.SetActive(false);
        PlayerHead.sprite = getImage(getImageNum());
        stageListener.enabled = true;
    }
}
