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

    public GameObject SelectorYes;
    public GameObject SelectorNo;

    private bool isReady;
    private bool readyUped;
    private Color rdyColor; //Unfaded
    private Color unRdyColor; //Faded

    private Master M;
    private ConnectAndJoinRandom N;
    private OnJoinedInstantiate j;

    public Sprite Empty;

    public Text PercentReady;

    public Image[] PlayerSlots;

    private int readyUpBypassCount;
    private int readyUpBypassTotal;

    public GameObject MatchCamera;

    public Text ReadyText;
    public GameObject Yes;
    public GameObject No;

    public Text roomName;

    public GameObject Warning;

    private int headSprite;
    public Sprite[] UIHeads;
    public Image PlayerHead;

    // Use this for initialization
    void Start () {
        readyUpBypassCount = 0;
        readyUpBypassTotal = 2;
        M = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
        N = GameObject.FindGameObjectWithTag("Networking").GetComponent<ConnectAndJoinRandom>();
        
        j = GameObject.FindGameObjectWithTag("SpawnPoints")
            .GetComponent<OnJoinedInstantiate>();

        isReady = false;
        readyUped = false;
        headSprite = getImageNum();


        PercentReady.text = "" + N.ReadyTotal + "/" + N.GetInRoomTotal;

        if (N.GameStarted)
            ActivateSpectating();

        rdyColor = new Color(1f, 1f, 1f, 1f);
        unRdyColor = new Color(1f, 1f, 1f, 0.5f);
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown("p"))
        {
            readyUpBypassCount++;
        }
        if (readyUpBypassTotal < readyUpBypassCount || N.StartMatch)
        {
            startMatch();   
        }


        if (N.GameStarted)
            return;

        if (N.GetRdyStateChange())
            updatePlayerReadyStatus();

        if (N.GetCharStateChange())
            updatePlayerCharDisplay();

        if (!readyUped && Input.GetButtonDown("Left") && !isReady)
        {
            SelectorYes.SetActive(true);
            SelectorNo.SetActive(false);
            isReady = true;
        }
        if (!readyUped && Input.GetButtonDown("Right") && isReady)
        {
            SelectorYes.SetActive(false);
            SelectorNo.SetActive(true);
            isReady = false;
        }
        if ( !readyUped && Input.GetButtonDown("Submit") && isReady
            && N.GetInRoomTotal > 1)
        {
            ReadyUp();
        }
        if (!readyUped && Input.GetButtonDown("Submit") && !isReady)
        {
            UnReady();

        }
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
        SelectorYes.SetActive(false);
        readyUped = true;
    }

    public void UnReady(){            
        isReady = false;
        readyUped = false;
        SelectorNo.SetActive(false);
        SelectorYes.SetActive(true);
        readyUped = false;
    }

    private void updatePlayerReadyStatus()
    {
        PercentReady.text = "" + N.ReadyTotal + "/" + N.GetInRoomTotal;

        M.PlaySFX(2);
        int playerSlot;
        for (int x = 0; x < PhotonNetwork.room.playerCount; x++)
        {
            playerSlot = N.GetSlotNum(PhotonNetwork.playerList[x].ID);
            PlayerSlots[x].color = N.GetRdyStatus(PhotonNetwork.playerList[x].ID) ? rdyColor : unRdyColor;
        }

        updateReadyHUD();
    }

    private void updatePlayerCharDisplay()
    {
        int playerSlot;
        for (int x = 0; x < PhotonNetwork.room.playerCount; x++)
        {
            playerSlot = N.GetSlotNum(PhotonNetwork.playerList[x].ID);
            PlayerSlots[x].sprite = UIHeads[N.GetCharNum(PhotonNetwork.playerList[x].ID)];
        }
    }

    private void updateReadyHUD()
    {
        bool isActive = true;
        
        for(int i = 0; i < M.Max_Players; i++)
        {
            if (i == PhotonNetwork.playerList.Length)
                isActive = false;
            PlayerSlots[i].transform.gameObject.SetActive(isActive);
        }
    }

    public void ActivateSpectating()
    {
        Yes.SetActive(false);
        SelectorYes.SetActive(false);
        No.SetActive(false);
        SelectorNo.SetActive(false);

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
    }
}
