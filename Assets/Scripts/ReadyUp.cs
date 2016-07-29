using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ReadyUp : MonoBehaviour{

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

   

    public Text ReadyText;
    public GameObject Yes;
    public GameObject No;

    private bool waiting;
    private bool isSpectating;
    //private int[] SlotList;

    public Text roomName;

    public GameObject Warning;

    private int headSprite;

    // Use this for initialization
    void Start () {
        readyUpBypassCount = 0;
        readyUpBypassTotal = 2;
        waiting = false;
        m = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
        n = GameObject.FindGameObjectWithTag("Networking").GetComponent<ConnectAndJoinRandom>();

        m.PlayMSX(1);
        j = GameObject.FindGameObjectWithTag("SpawnPoints")
            .GetComponent<OnJoinedInstantiate>();

        totalReady = 0;
        m_PhotonView = GetComponent<PhotonView>();
        isReady = false;
        readyUped = false;
        headSprite = getImageNum();
        m = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
        n.ReadyInterface = GetComponent<ReadyUp>();
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
            Warning.SetActive(true);
            return;
        }
        if (isReady)
        {
            return;
        }
        isReady = true;
        m_PhotonView.RPC("ShowReady", PhotonTargets.All, myLogInID);
        SelectorYes.SetActive(false);
        readyUped = true;
        ExitGames.Client.Photon.Hashtable tempPlayerTable = 
            PhotonNetwork.player.customProperties;
        tempPlayerTable["IsReady"] = true;
        PhotonNetwork.player.SetCustomProperties(tempPlayerTable);
    }

    public void NotReady(){
        m.GoBack();
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

        PlayerSlots[readySlot].sprite = getImage(readyChar);
         
        
        if (totalReady == totalLoggedIn)
        {
            startGame();
        }
        else 
        {
            Debug.Log("Not all Ready: " + totalReady + "/" + totalLoggedIn);
        }
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


    public void CheatGame()
    {
        readyUpBypassCount = 3;
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
            if (UIHeads[i].name == m.GetClientCharacterName())
            {
                return i;
            }
        }
        Debug.LogError("No Head Name Found!");
        return -1;
    }

    private void startGame()
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

    private Sprite getImage(int num)
    {
        return UIHeads[num];
    }
}
