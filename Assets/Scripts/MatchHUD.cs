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

    private PhotonView m_PhotonView;
    private Master m;
    private ConnectAndJoinRandom n;
    private OnJoinedInstantiate j;

    public Sprite Empty;

    public Text PercentReady;

    public Image[] PlayerSlots;
    
    private int totalReady;

    private int readyUpBypassCount;
    private int readyUpBypassTotal;

    public GameObject MatchCamera;

    public Text ReadyText;
    public GameObject Yes;
    public GameObject No;

    private bool waiting;
    private bool isSpectating;
    //private int[] SlotList;

    public Text roomName;

    public GameObject Warning;

    private int headSprite;
    public Sprite[] UIHeads;
    public Image PlayerHead;

    // Use this for initialization
    void Start () {
        readyUpBypassCount = 0;
        readyUpBypassTotal = 2;
        waiting = false;
        m = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
        n = GameObject.FindGameObjectWithTag("Networking").GetComponent<ConnectAndJoinRandom>();
        
        j = GameObject.FindGameObjectWithTag("SpawnPoints")
            .GetComponent<OnJoinedInstantiate>();

        totalReady = 0;
        m_PhotonView = GetComponent<PhotonView>();
        isReady = false;
        readyUped = false;
        headSprite = getImageNum();

        n.assignMatchHUD();
        m.assignMatchHUD();
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown("p"))
        {
            readyUpBypassCount++;
        }
        if (readyUpBypassTotal < readyUpBypassCount)
        {
            totalReady = 2;
			//j.OnReadyUp(ID_to_SlotNum[myLogInID]);
            startGame();
        }

        if (waiting)
        {   
            if (!isSpectating)
            {
                if (GameObject.FindGameObjectWithTag("PlayerSelf") != null)
                {
                    AssignCameraFollow(GameObject.FindGameObjectWithTag("PlayerSelf").transform);
                    isSpectating = true;
                }
            }
            return;
        }

        PercentReady.text = "" + totalReady + "/" + n.GetInRoomTotal;
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
        if (!readyUped && Input.GetButtonDown("Submit") && isReady
            && n.GetInRoomTotal > 1)
        {
            Ready();
        }
        if (!readyUped && Input.GetButtonDown("Submit") && !isReady)
        {
            NotReady();
        }
	}

    public void Ready() 
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
        m_PhotonView.RPC("ShowReady", PhotonTargets.All, PhotonNetwork.player.ID);
        SelectorYes.SetActive(false);
        readyUped = true;
        ExitGames.Client.Photon.Hashtable tempPlayerTable = 
            PhotonNetwork.player.customProperties;
        tempPlayerTable["IsReady"] = true;
        PhotonNetwork.player.SetCustomProperties(tempPlayerTable);
    }

    public void NotReady(){
        if (!isReady)
        {
            m.GoBack();
            return;
        }
        isReady = false;
        m_PhotonView.RPC("ShowNotReady", PhotonTargets.All, PhotonNetwork.player.ID);
        SelectorYes.SetActive(true);
        readyUped = false;
        ExitGames.Client.Photon.Hashtable tempPlayerTable =
            PhotonNetwork.player.customProperties;
        tempPlayerTable["IsReady"] = false;
        PhotonNetwork.player.SetCustomProperties(tempPlayerTable);
    }

    //Need 2 things: Chosen Character and Player Num
    [PunRPC]
    void ShowReady(int logInID)
    {
        m.PlaySFX(2);
        totalReady++;
        int readyChar = n.GetCharNum(logInID);
        int readySlot = n.GetSlotNum(logInID);

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

    [PunRPC]
    void ShowNotReady(int logInID)
    {
        m.PlaySFX(6);
        totalReady--;
        int readyChar = n.GetCharNum(logInID);
        int readySlot = n.GetSlotNum(logInID);

        PlayerSlots[readySlot].sprite = Empty;

        if (totalReady == n.GetInRoomTotal)
        {
            startGame();
        }
        else
        {
            Debug.Log("Not all Ready: " + totalReady + "/" + n.GetInRoomTotal);
        }
    }


    public void UpdateReadyHUD()
    {
        bool isActive = true;
        for(int i = 0; i < 6; i++)
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
        waiting = true;
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
    /// Does GUI related things in response to @player disconnecting.
    /// </summary>
    /// <param name="player">Disconnected PhotonPlayer</param>
    public void RemovePlayer(PhotonPlayer player)
    {

        totalReady = 0;
        SelectorYes.SetActive(false);
        SelectorNo.SetActive(true);
        isReady = false;
        readyUped = false;
        fixReadyHeads();
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
        fixReadyHeads();
    }

    private void fixReadyHeads()
    {
        for (int i = 0; i < 6; i++)
        {
            PlayerSlots[i].transform.gameObject.SetActive(false);
            PlayerSlots[i].sprite = Empty;
        }
        
        for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
        {
            PlayerSlots[i].transform.gameObject.SetActive(true);
        }
    }

    private void AssignCameraFollow(Transform myTransform)
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
