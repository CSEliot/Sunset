using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// This script automatically connects to Photon (using the settings file),
/// tries to join a random room and creates one if none was found (which is ok).
/// </summary>
public class ConnectAndJoinRandom : Photon.MonoBehaviour
{
    private string version;

    /// <summary>if we don't want to connect in Start(), we have to "remember" if we called ConnectUsingSettings()</summary>
    private bool isConnectAllowed;

    public int SendRate;

    private Master m;
    private ReadyUp readyInterface;

    private bool isScannable = false;
    private RoomInfo[] latestRooms;
    public Text RoomPlayerCount;
    public Text RoomPlayerCountBG;
    public Text TotalPlayerCount;
    public Text TotalPlayerCountBG;
    private string totalPlayerCountStr;
    private int onlineTotalCount;

    private bool isFirstTimeConnect; //first connect should be from main -> map, then same state if reconnect.
    private int savedState;
    private string waitingRoomName;
    private int serverPlayerMax;
    private bool isEastServer;

    private float previousUpdateTime;
    private float serverUpdateLength;

    private int totalLoggedIn;
    private Dictionary<int, int> ID_to_SlotNum;
    private Dictionary<int, int> ID_to_CharNum;
    private Dictionary<int, bool> ID_to_IsReady;

    private ReadyUp readyMenu;

    public void Awake()
    {
        ID_to_SlotNum = new Dictionary<int, int>();
        ID_to_CharNum = new Dictionary<int, int>();
        ID_to_IsReady = new Dictionary<int, bool>();

        totalLoggedIn = 0;

        serverUpdateLength = 5f; // Seconds

        Debug.Log("Setting Send Rate to: " + SendRate);
        PhotonNetwork.sendRate = SendRate;
        PhotonNetwork.sendRateOnSerialize = SendRate;
        m = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
        version = Debug.isDebugBuild ? "Test2" : m.Version;
    }

    public virtual void Start()
    {

        previousUpdateTime = Time.time;

        serverPlayerMax = 100;
        waitingRoomName = "Waiting"; 
        isFirstTimeConnect = true;
        isConnectAllowed = false; //Enabled when server region given.
        isEastServer = false;

        PhotonNetwork.autoJoinLobby = true;    
    }

    public virtual void Update()
    {
        if (isConnectAllowed && !PhotonNetwork.connected)
        {
            Debug.Log("Beginning Connection . . .");

            isConnectAllowed = false;
            PhotonNetwork.ConnectToRegion( isEastServer ? CloudRegionCode.us : CloudRegionCode.usw, version);
            Debug.Log("Version Number is: " + version);
            PhotonNetwork.sendRate = SendRate;
            PhotonNetwork.sendRateOnSerialize = SendRate;
            //PhotonNetwork.ConnectToMaster("52.9.58.118", 5055,
            //    "d4e9e94d-de9a-44ec-9668-5f1898d4e76c",
            //    version); FOR USING PERSONAL SERVER

            savedState = m.CurrentMenu;
            m.GoTo(-1); //Disable menus while connecting is in progress.
        }

        if (!isScannable)
            return;

        if (Time.time - previousUpdateTime > serverUpdateLength)
        {
            previousUpdateTime = Time.time;
            CountTotalOnline();
        }

        totalPlayerCountStr = "Players Online\n" + onlineTotalCount;
        TotalPlayerCount.text = totalPlayerCountStr;
        TotalPlayerCountBG.text = totalPlayerCountStr;
    }

    /// <summary>
    /// All "InNet" for all public functions relating to networking, but not a Photon function.
    /// </summary>
    public void SetCharacterInNet()
    {
        Debug.Log("Adding player property");
        ExitGames.Client.Photon.Hashtable playerProperties = PhotonNetwork.player.customProperties;
        ////Track each player's chosen character.
        if (playerProperties.ContainsKey("characterNum"))
        {
            playerProperties["characterNum"] = m.PlayerCharNum;
        }
        else
        {
            playerProperties.Add("characterNum", m.PlayerCharNum);
        }

        PhotonNetwork.player.SetCustomProperties(playerProperties);

        //Add self to player info tracking.
        ID_to_CharNum.Add(PhotonNetwork.player.ID, m.PlayerCharNum);
        ID_to_SlotNum.Add(PhotonNetwork.player.ID, PhotonNetwork.playerList.Length - 1);
        ID_to_IsReady.Add(PhotonNetwork.player.ID, false);
    }

    /// <summary>
    /// Note: NEED GUI FOR IF ROOM IS FULL.
    /// </summary>
    public void JoinRoom()
    {
        PhotonNetwork.JoinOrCreateRoom(m.GetRoomName(), 
            new RoomOptions() { MaxPlayers = Convert.ToByte(m.Max_Players) }, null);
    }

    public virtual void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster() was succesfull.");
        ////Get room properties, containing things such as player info and selected character.

        if (isFirstTimeConnect)
        {
            isFirstTimeConnect = false;
            m.GoTo(1); // go to map select on first game load.
        }else
        {
            m.GoTo(savedState);
        }
    }

    public virtual void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby() was a success.");
        //PhotonNetwork.JoinOrCreateRoom("Waiting", new RoomOptions() { maxPlayers = Convert.ToByte(serverPlayerMax) }, defaultLobby);
        isScannable = true;
        if (isFirstTimeConnect)
        {
            isFirstTimeConnect = false;
            m.GoTo(1); // go to map select on first game load.
        }
        else
        {
            m.GoTo(savedState);
        }
    }

    public virtual void OnPhotonRandomJoinFailed()
    {
        
        Debug.Log("OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one. Calling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 6}, null);");
    }

    // the following methods are implemented to give you some context. re-implement them as needed.
        
    public virtual void OnFailedToConnectToPhoton(DisconnectCause cause)
    {

        Debug.LogError("Failed to Connect!");
        Debug.LogError("Cause: " + cause);
        m.GoBack();
    }

    public void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom() called by PUN. Now this client is in a room. From here on, your game would be running. For reference, all callbacks are listed in enum: PhotonNetworkingMessage");

        //Get total number of players logged into room.
        int totalPlayersFound = PhotonNetwork.playerList.Length;
        PhotonNetwork.playerName = "Player " + totalPlayersFound;
        m.InRoomNumber = totalPlayersFound--;

        // If it's a new room, create descriptor keys
        ExitGames.Client.Photon.Hashtable tempTable;
        if (!PhotonNetwork.room.customProperties.ContainsKey("GameStarted"))
        {
            tempTable = PhotonNetwork
                .room.customProperties;
            tempTable.Add("GameStarted", false);
            PhotonNetwork.room.SetCustomProperties(tempTable);

        } else 
        // if room is already made AND the game started . . .
        if ((bool)PhotonNetwork.room.customProperties["GameStarted"] == true)
        {
            readyInterface.SetSpectating();
            return;
        }
        
        //if we join in and game is ready to start, un-ready,
        //because a new player forces all players to re-ready.
        tempTable = PhotonNetwork.player.customProperties;
        if (!tempTable.ContainsKey("IsReady"))
            tempTable.Add("IsReady", false);
        else
            tempTable["IsReady"] = false;

        //Create IsReady state. Begins false.
        PhotonNetwork.player.SetCustomProperties(tempTable);

        //Add info from others already logged in.
        int otherLogInID = 0;
        int otherSlotNum = 0;
        int otherCharNum = 0;
        for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
        {
            PlayerSlots[i].transform.gameObject.SetActive(true);
            otherLogInID = PhotonNetwork.playerList[i].ID;
            if (PhotonNetwork.player.ID != otherLogInID)
            {
                otherSlotNum = i;
                otherCharNum = (int)PhotonNetwork.playerList[i]
                    .customProperties["characterNum"];
                ID_to_CharNum.Add(otherLogInID, otherCharNum);
                ID_to_SlotNum.Add(otherLogInID, otherSlotNum);
                //Everyone Unreadies when someone new joins.
                ID_to_IsReady.Add(otherLogInID, false);
            }
            totalLoggedIn++;
        }     
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
           , (int)player.customProperties["characterNum"]);
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

        Debug.Log("OnPhotonPlayerDisconnected: " + player);

        //attempt reconnection.
        isConnectAllowed = true;

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

    private void CountTotalOnline()
    {
        int tempOnlineCount = PhotonNetwork.countOfPlayers;
        latestRooms = PhotonNetwork.GetRoomList();

        Debug.Log("Is online? " + PhotonNetwork.connected);
        Debug.Log("Inside Lobby? " + PhotonNetwork.insideLobby);
        Debug.Log("Total Lobbies: " + PhotonNetwork.LobbyStatistics.Count);
        Debug.Log("Lobby Name: " + PhotonNetwork.LobbyStatistics[PhotonNetwork.LobbyStatistics.Count-1].Name);
        Debug.Log("Nums : " + PhotonNetwork.LobbyStatistics[PhotonNetwork.LobbyStatistics.Count - 1].PlayerCount);
        tempOnlineCount = PhotonNetwork.LobbyStatistics[PhotonNetwork.LobbyStatistics.Count - 1].PlayerCount;
        Debug.Log("Lobby Type: " + PhotonNetwork.LobbyStatistics[PhotonNetwork.LobbyStatistics.Count - 1].Type);
        Debug.Log("Lobby Room Total: " + PhotonNetwork.LobbyStatistics[PhotonNetwork.LobbyStatistics.Count - 1].RoomCount);
        Debug.Log("Lobby Default? " + PhotonNetwork.LobbyStatistics[PhotonNetwork.LobbyStatistics.Count - 1].IsDefault);


        for (int x = 0; x < latestRooms.Length; x++)
        {
            Debug.Log("Room " + latestRooms[x].name + " has " + latestRooms[x].playerCount + "players.");
        }
        Debug.Log("There are a total of " + tempOnlineCount + " online.");
    }

    public bool IsEastServer
    {
        get
        {
            return isEastServer;
        }

        set
        {
            isConnectAllowed = true;
            isEastServer = value;
        }
    }

    public ReadyUp ReadyInterface
    {
        get
        {
            return readyInterface;
        }

        set
        {
            readyInterface = value;
        }
    }

    public void LeaveServer()
    {
        PhotonNetwork.Disconnect();
        isFirstTimeConnect = true;
        isScannable = false;
    }
}
