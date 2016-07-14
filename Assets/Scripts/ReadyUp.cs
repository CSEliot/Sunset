using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ReadyUp : MonoBehaviour {

    public GameObject SelectorYes;
    public GameObject SelectorNo;

    private bool isReady;
    private bool readyUped;

    private PhotonView m_PhotonView;
    private Master m;
    private OnJoinedInstantiate j;

    private int headSprite;

    public Sprite Empty;

    public Text PercentReady;

    public Image[] PlayerSlots;

    private int totalLoggedIn;
    private int totalReady;

    private Dictionary<int, int> ID_to_SlotNum;
    private Dictionary<int, int> ID_to_CharNum;
    private Dictionary<int, bool>ID_to_IsReady;

    private int readyUpBypassCount;
    private int readyUpBypassTotal;

    private int myLogInID;

    public Text ReadyText;
    public GameObject Yes;
    public GameObject No;

    private bool waiting;
    private bool isSpectating;
    //private int[] SlotList;

    public Text roomName;
    
	// Use this for initialization
	void Start () {
        readyUpBypassCount = 0;
        readyUpBypassTotal = 2;
        waiting = false;
        m = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
        m.PlayMSX(1);
        j = GameObject.FindGameObjectWithTag("SpawnPoints")
            .GetComponent<OnJoinedInstantiate>();
        ID_to_SlotNum = new Dictionary<int, int>();
        ID_to_CharNum = new Dictionary<int, int>();
        ID_to_IsReady = new Dictionary<int, bool>();

        totalLoggedIn = 0;
        totalReady = 0;
        m_PhotonView = GetComponent<PhotonView>();
        isReady = false;
        readyUped = false;
        headSprite = j.GetImageNum();
        m = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
        
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
			j.OnReadyUp(ID_to_SlotNum[myLogInID]);
            StartGame();
        }

        if (waiting)
        {   
            if (GameObject.FindGameObjectsWithTag("PlayerSelf").Length <= 1)
            {
                EndGame();
            }
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
        
        //SlotList = new int[6];
        //for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
        //{
        //    SlotList[i] = PhotonNetwork.playerList[i].ID;
        //}
        PercentReady.text = "" + totalReady + "/" + totalLoggedIn;
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
            && totalLoggedIn > 1)
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
        if (totalLoggedIn < 2)
        {
            return;
        }
        isReady = true;
        m_PhotonView.RPC("ShowReady", PhotonTargets.All, myLogInID);
        SelectorYes.SetActive(false);
        readyUped = true;
        ExitGames.Client.Photon.Hashtable tempPlayerTable = PhotonNetwork
        .player.customProperties;
        tempPlayerTable["IsReady"] = true;
        PhotonNetwork.player.SetCustomProperties(tempPlayerTable);
    }

    public void NotReady(){
        isReady = false;
        readyUped = false;
        PhotonNetwork.Disconnect();
        m.AssignClientCharacter(0);
        m.GoBack();
        m.PlayMSX(4);
    }

    //Need 2 things: Chosen CHaracter and Player Num
    [PunRPC]
    void ShowReady(int LogInID)
    {
        m.PlaySFX(2);
        totalReady++;
        int readyChar = ID_to_CharNum[LogInID];
        int readySlot = ID_to_SlotNum[LogInID];
        ID_to_IsReady[LogInID] = true;

        PlayerSlots[readySlot].sprite = j.GetImage(readyChar);
        
        
        if (totalReady == totalLoggedIn)
        {
            StartGame();
        }
        else 
        {
            Debug.Log("Not all Ready: " + totalReady + "/" + totalLoggedIn);
        }
    }

    private void StartGame()
    {
        m.PlayMSX(2);
        Debug.Log("Start Game!");
        j.OnReadyUp(ID_to_SlotNum[myLogInID]);
        ExitGames.Client.Photon.Hashtable tempRoomTable = PhotonNetwork
            .room.customProperties;
        tempRoomTable["GameStarted"] = true;
        PhotonNetwork.room.SetCustomProperties(tempRoomTable);
        m.GameStarts(totalReady);
        gameObject.SetActive(false);
    }

    void OnJoinedRoom()
    {
        roomName.text = PhotonNetwork.room.name;    
        if (PhotonNetwork.room.customProperties.ContainsKey("GameStarted")
            && (bool)PhotonNetwork.room.customProperties["GameStarted"] == true)
        {
            SetSpectating();
            return;
        }

        myLogInID = PhotonNetwork.player.ID;
        ExitGames.Client.Photon.Hashtable tempTable = PhotonNetwork.player.customProperties;
        if (!tempTable.ContainsKey("IsReady"))
            tempTable.Add("IsReady", false);
        else
            tempTable["IsReady"] = false;

        //Create IsReady state. Begins false.
        PhotonNetwork.player.SetCustomProperties(tempTable);

        //Add self to player info tracking.
        ID_to_CharNum.Add(myLogInID, headSprite);
        ID_to_SlotNum.Add(myLogInID, PhotonNetwork.playerList.Length - 1);
        ID_to_IsReady.Add(myLogInID, false);

        //Add info from others already logged in.
        int otherLogInID = 0;
        int otherSlotNum = 0;
        int otherCharNum = 0;
        for(int i = 0; i < PhotonNetwork.playerList.Length; i++){
            PlayerSlots[i].transform.gameObject.SetActive(true);
            otherLogInID = PhotonNetwork.playerList[i].ID;
            if (myLogInID != otherLogInID)
            {
                otherSlotNum = i;
                otherCharNum = (int)PhotonNetwork.playerList[i]
                    .customProperties["ChosenCharNum"];
                ID_to_CharNum.Add(otherLogInID, otherCharNum);
                ID_to_SlotNum.Add(otherLogInID, otherSlotNum);
                //Everyone Unreadies when someone new joins.
                ID_to_IsReady.Add(otherLogInID, false);
            }
            totalLoggedIn++;
        }
    }

    public void EndGame()
    {
        PhotonNetwork.room.customProperties["GameStarted"] = false;
        m.endGame();
    }

    public void SetSpectating()
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

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    { 
    }

    void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        PlayerSlots[PhotonNetwork.playerList.Length - 1]
            .transform.gameObject.SetActive(true);
        int otherLogInID = player.ID;
        //Debug.Log("Player Connected! ID: " + player.ID);
        //foreach (object k in player.customProperties.Keys)
        //{
        //    Debug.Log("Key: " + (string)k);
        //}
         ID_to_CharNum.Add(otherLogInID
            , (int)player.customProperties["ChosenCharNum"]);
        ID_to_SlotNum.Add(otherLogInID, PhotonNetwork.playerList.Length - 1);
        ID_to_IsReady.Add(otherLogInID, false);
        totalLoggedIn++;

        ///Reset GFX & Ready Status for new players.
        totalReady = 0;
        SelectorYes.SetActive(false);
        SelectorNo.SetActive(true);
        isReady = false;
        readyUped = false;
        fixReadyHeads();

        ExitGames.Client.Photon.Hashtable tempPlayerTable = PhotonNetwork
            .player.customProperties;
        tempPlayerTable["IsReady"] = false;
        PhotonNetwork.player.SetCustomProperties(tempPlayerTable);
    }

    void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        int otherID = player.ID;
        ID_to_SlotNum.Remove(otherID);
        ID_to_CharNum.Remove(otherID);
        ID_to_IsReady.Remove(otherID);
        totalLoggedIn--;
        totalReady = 0;
        SelectorYes.SetActive(false);
        SelectorNo.SetActive(true);
        isReady = false;
        readyUped = false;
        fixReadyHeads();
        //ExitGames.Client.Photon.Hashtable tempPlayerTable = PhotonNetwork
        //    .player.customProperties;
        //tempPlayerTable["IsReady"] = false;
        //player.SetCustomProperties(tempPlayerTable);
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

    public void CheatGame()
    {
        readyUpBypassCount = 3;
    }
}
