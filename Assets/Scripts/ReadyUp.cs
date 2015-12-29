using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

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


    private int myLogInID;

    public Text ReadyText;
    public GameObject Yes;
    public GameObject No;



    private bool waiting;
    //private int[] SlotList;

	// Use this for initialization
	void Start () {
        waiting = false;
        m = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
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
        if (waiting)
            return;
        
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
            m_PhotonView.RPC("ShowReady", PhotonTargets.All, myLogInID);
            SelectorYes.SetActive(false);
            readyUped = true;
            m.PlaySFX(2);
        }
	}

    //Need 2 things: Chosen CHaracter and Player Num
    [PunRPC]
    void ShowReady(int LogInID)
    {
        totalReady++;
        int readyChar = ID_to_CharNum[LogInID];
        int readySlot = ID_to_SlotNum[LogInID];
        ID_to_IsReady[LogInID] = true;

        PlayerSlots[readySlot].sprite = j.GetImage(readyChar);
        if (totalReady == totalLoggedIn)
        {
            Debug.Log("Start Game!");
            j.OnReadyUp(readySlot);
            ExitGames.Client.Photon.Hashtable tempRoomTable = PhotonNetwork
                .room.customProperties;
            tempRoomTable["GameStarted"] = true;
            PhotonNetwork.room.SetCustomProperties(tempRoomTable);
            ExitGames.Client.Photon.Hashtable tempPlayerTable = PhotonNetwork
                .player.customProperties;
            tempPlayerTable["IsReady"] = true;
            PhotonNetwork.player.SetCustomProperties(tempPlayerTable);
            m.GameStarts(totalReady);
            gameObject.SetActive(false);
        }
        else 
        {
            Debug.Log("Not all Ready: " + totalReady + "/" + totalLoggedIn);
        }
    }

    void OnJoinedRoom()
    {
        if (PhotonNetwork.room.customProperties.ContainsKey("GameStarted")
            && (bool)PhotonNetwork.room.customProperties["GameStarted"] == true)
        {
            if (GameObject.FindGameObjectWithTag("PlayerSelf") != null)
                AssignCameraFollow(GameObject.FindGameObjectWithTag("PlayerSelf").transform);
            Yes.SetActive(false);
            SelectorYes.SetActive(false);
            No.SetActive(false);
            SelectorNo.SetActive(false);

            ReadyText.text = "Please Wait. . .";
            PercentReady.text = "";
            waiting = true;
            for (int i = 0; i < PlayerSlots.Length; i++)
            {
                PlayerSlots[i].transform.gameObject.SetActive(false);
            }
            return;
        }
        myLogInID = PhotonNetwork.player.ID;
        ExitGames.Client.Photon.Hashtable tempTable = PhotonNetwork.player.customProperties;
        if(!tempTable.ContainsKey("IsReady"))
            tempTable.Add("IsReady", false);

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
        bool otherIsReady = false;
        for(int i = 0; i < PhotonNetwork.playerList.Length; i++){
            PlayerSlots[i].transform.gameObject.SetActive(true);
            otherLogInID = PhotonNetwork.playerList[i].ID;
            if (myLogInID != otherLogInID)
            {
                otherSlotNum = i;
                otherCharNum = (int)PhotonNetwork.playerList[i]
                    .customProperties["ChosenCharNum"];
                otherIsReady = (bool)PhotonNetwork.playerList[i]
                    .customProperties["IsReady"];
                ID_to_CharNum.Add(otherLogInID, otherCharNum);
                ID_to_SlotNum.Add(otherLogInID, otherSlotNum);
                ID_to_IsReady.Add(otherLogInID, otherIsReady);
            }
            totalLoggedIn++;
        }
    }

    [PunRPC]
    void RESTARTGAME()
    {
        m.endGame();
    }

    public void EndGame()
    {
        PhotonNetwork.room.customProperties["GameStarted"] = false;
        m_PhotonView.RPC("RESTARTGAME", PhotonTargets.All);
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
        //ID_to_ReadyNum.Add(otherLogInID, );
        totalLoggedIn++;
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
        if (myTransform == null)
        {
            return;
        }
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<UnityStandardAssets._2D.Camera2DFollow>()
            .SetTarget(myTransform);
    }
}
