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

    private int headSprite;

    public Sprite Empty;

    public Image[] PlayerSlots;

    private int totalLoggedIn;

    private Dictionary<int, int> ID_to_SlotNum;
    private Dictionary<int, int> ID_to_CharNum;

    private int myLogInID;

    private int[] SlotList;

	// Use this for initialization
	void Start () {
        m = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
        ID_to_SlotNum = new Dictionary<int, int>();
        ID_to_CharNum = new Dictionary<int, int>();
        totalLoggedIn = 0;
        m_PhotonView = GetComponent<PhotonView>();
        isReady = false;
        readyUped = false;
        headSprite = GameObject.FindGameObjectWithTag("SpawnPoints")
            .GetComponent<OnJoinedInstantiate>().GetImageNum();
	}
	
	// Update is called once per frame
	void Update () {
        SlotList = new int[6];
        for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
        {
            SlotList[i] = PhotonNetwork.playerList[i].ID;
        }

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
        if (!readyUped && Input.GetButtonDown("Submit") && isReady)
        {
            m_PhotonView.RPC("ShowReady", PhotonTargets.All, myLogInID);
            SelectorYes.SetActive(false);
        }
	}

    //Need 2 things: Chosen CHaracter and Player Num
    [PunRPC]
    void ShowReady(int LogInID)
    {
        int readyChar = ID_to_CharNum[LogInID];
        int readySlot = ID_to_SlotNum[LogInID];
        PlayerSlots[readySlot].sprite = GameObject.FindGameObjectWithTag("SpawnPoints")
            .GetComponent<OnJoinedInstantiate>().GetImage(readyChar);
        readyUped = true;
    }

    void OnJoinedRoom()
    {
        myLogInID = PhotonNetwork.player.ID;
        ExitGames.Client.Photon.Hashtable tempTable = PhotonNetwork.player.customProperties;

        //Add self to player info tracking.
        ID_to_CharNum.Add(myLogInID, headSprite);
        ID_to_SlotNum.Add(myLogInID, PhotonNetwork.playerList.Length - 1);

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
                otherCharNum = (int)PhotonNetwork.playerList[i].customProperties["ChosenCharNum"];
                ID_to_CharNum.Add(otherLogInID, otherCharNum);
                ID_to_SlotNum.Add(otherLogInID, otherSlotNum);
            }
        }
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    { 
    }

    void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        int otherLogInID = PhotonNetwork.player.ID; ;
        ID_to_CharNum.Add(otherLogInID
            , (int)player.customProperties["ChosenCharNum"]);
        ID_to_SlotNum.Add(otherLogInID, PhotonNetwork.playerList.Length - 1);
        Debug.Log("Player Connected! ID: " + player.ID);
        //ID_to_ReadyNum.Add(otherLogInID, );
        totalLoggedIn++;
    }

    void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        totalLoggedIn--;
    }


}
