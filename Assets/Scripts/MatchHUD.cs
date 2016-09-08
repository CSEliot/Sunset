using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// All Pre-game, Post-Character Select UI code. 
/// Dependency on net code and Photon minimized room size calls.
/// </summary>
public class MatchHUD : MonoBehaviour{

    private bool isReady;
    public Color rdyColor; //Unfaded
    public Color unRdyColor; //Faded

    private Master M;
    private ConnectAndJoinRandom N;
    private OnJoinedInstantiate j;

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
        readyUpBypassCount = 0;
        readyUpBypassTotal = 2;
        M = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
        N = GameObject.FindGameObjectWithTag("Networking").GetComponent<ConnectAndJoinRandom>();
        
        j = GameObject.FindGameObjectWithTag("SpawnPoints")
            .GetComponent<OnJoinedInstantiate>();

        isReady = false;
        headSprite = getImageNum();

        PercentReady.text = "" + N.ReadyTotal + "/" + N.GetInRoomTotal;

        if (N.GameStarted)
            ActivateSpectating();

        rdyColor = new Color(1f, 1f, 1f, 1f);
        unRdyColor = new Color(1f, 1f, 1f, 0.5f);

        stageListener = GameObject.FindGameObjectWithTag("StageCamera").GetComponent<AudioListener>();
        roomName.text = N.CurrentRoom.name;
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
            updateHUDPlayerDisplay();

        if (N.GetRdyStateChange())
            updateHUDStatusDisplay();

        if (N.GetCharStateChange())
            updateHUDCharDisplay();

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
        if (!isReady)
            return;        
        isReady = false;
        N.UnreadyButton();
    }

    private void updateHUDStatusDisplay()
    {
        PercentReady.text = "" + N.ReadyTotal + "/" + N.GetInRoomTotal;
        
        int playerSlot;
        for (int x = 0; x < PhotonNetwork.room.playerCount; x++)
        {
            playerSlot = N.GetSlotNum(PhotonNetwork.playerList[x].ID);
            PlayerSlots[playerSlot].color = N.GetRdyStatus(PhotonNetwork.playerList[x].ID) ? rdyColor : unRdyColor;
        }
    }

    private void updateHUDCharDisplay()
    {
        int slotNum;
        int playerID;
        int charNum;
        for (int x = 0; x < PhotonNetwork.room.playerCount; x++)
        {
            playerID = PhotonNetwork.playerList[x].ID;
            slotNum = N.GetSlotNum(playerID);
            charNum = N.GetCharNum(playerID);
            PlayerSlots[slotNum].sprite = UIHeads[charNum];
            if(Master.DEBUG_ON)
                Debug.Log(string.Format("UpdateHUDCharDisplay playerID: {0} slotNum: {1} charNum: {2}", playerID, slotNum, charNum));
        }
    }

    private void updateHUDPlayerDisplay()
    {
        bool isActive = true;
        
        for(int i = 0; i < M.Max_Players; i++)
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
            M.EscapeEnabled = false;
            //on the last second, disable the ability to leave
            No.SetActive(false);
        }
    }

    public void ActivateSpectating()
    {
        Yes.SetActive(false);
        No.SetActive(false);

        ReadyText.text = "Spectating . . .";
        PercentReady.text = "";
        for (int i = 0; i < PlayerSlots.Length; i++)
        {
            PlayerSlots[i].transform.gameObject.SetActive(false);
        }
    }


    public void CheatGame()
    {
        readyUpBypassCount = 3;
    }
    
    private void assignCameraFollow(Transform myTransform)
    {
        Debug.Log("Testing Assign Camera.");
        if (myTransform == null)
        { 
            return;
        }
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CamManager>()
            .SetTarget(myTransform);
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
        Debug.LogError("No Head Found!");
        return -1;
    }

    private Sprite getImage(int num)
    {
        return UIHeads[num];
    }

    private void startMatch()
    {
        j.GameStarted(N.GetSlotNum(PhotonNetwork.player.ID));
        gameObject.SetActive(false);
        PlayerHead.sprite = getImage(getImageNum());
        stageListener.enabled = true;
    }
}
