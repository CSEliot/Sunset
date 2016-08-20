using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MatchHUD : MonoBehaviour{

    public GameObject SelectorYes;
    public GameObject SelectorNo;

    private bool isReady;
    private bool readyUped;

    private Master m;
    private ConnectAndJoinRandom n;
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
        m = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
        n = GameObject.FindGameObjectWithTag("Networking").GetComponent<ConnectAndJoinRandom>();
        
        j = GameObject.FindGameObjectWithTag("SpawnPoints")
            .GetComponent<OnJoinedInstantiate>();

        isReady = false;
        readyUped = false;
        headSprite = getImageNum();
        
        
        if(n.GameStarted)
            ActivateSpectating();
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown("p"))
        {
            readyUpBypassCount++;
        }
        if (readyUpBypassTotal < readyUpBypassCount)
        {
			//j.OnReadyUp(ID_to_SlotNum[myLogInID]);
            startGame();
        }

        if (n.GameStarted)
            return;

        PercentReady.text = "" + N+ "/" + n.GetInRoomTotal;
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
            && n.GetInRoomTotal > 1)
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
        if (n.GetInRoomTotal < 2)
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



    public void ShowPlayerReadyStatus(int playerID, bool readyStatus)
    {
        m.PlaySFX(2);
        int readyChar = n.GetCharNum(playerID);
        int readySlot = n.GetSlotNum(playerID);

        PlayerSlots[readySlot].sprite = getImage(readyChar);
         

        if (totalReady == n.GetInRoomTotal)
        {
            startGame();
        }
        else 
        {
            Debug.Log("Not all Ready: " + totalReady + "/" + n.GetInRoomTotal);
        }
    }
    
    public void ShowPlayerNotReady(int playerID)
    {
        m.PlaySFX(6);

        int playerNum = n.GetSlotNum(playerID);

        PlayerSlots[playerNum].sprite = Empty;

        Debug.Log("Not all Ready: " + totalReady + "/" + n.GetInRoomTotal);
        
    }


    public void UpdateReadyHUD()
    {
        bool isActive = true;
        
        for(int i = 0; i < m.Max_Players; i++)
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
         = true;
        for (int i = 0; i < PlayerSlots.Length; i++)
        {
            PlayerSlots[i].transform.gameObject.SetActive(false);
        }
    }


    public void CheatGame()
    {
        readyUpBypassCount = 3;
    }

    /// <summary>
    /// Does GUI related things in response to @player connecting.
    /// </summary>
    /// <param name="player">Connected PhotonPlayer</param>
    public void AddPlayer(PhotonPlayer player)
    {
        PlayerSlots[PhotonNetwork.playerList.Length - 1].transform.gameObject.SetActive(true);
        ///Reset GFX & Ready Status for new players.
        totalReady = 0;
        SelectorYes.SetActive(false);
        SelectorNo.SetActive(true);
        isReady = false;
        readyUped = false;
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
            if (i == m.PlayerCharNum)
            {
                return i;
            }
        }
        Debug.LogError("No Head Found!");
        return -1;
    }

    private void startGame()
    {
        m.PlayMSX(2);
        Debug.Log("Start Game!");
        j.OnReadyUp(n.GetSlotNum(PhotonNetwork.player.ID));
        ExitGames.Client.Photon.Hashtable tempRoomTable = PhotonNetwork
            .room.customProperties;
        tempRoomTable["GameStarted"] = true;
        PhotonNetwork.room.SetCustomProperties(tempRoomTable);
        m.GameStarts(totalReady);
        gameObject.SetActive(false);
        PlayerHead.sprite = getImage(getImageNum());
    }

    private Sprite getImage(int num)
    {
        return UIHeads[num];
    }
}
