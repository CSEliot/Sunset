using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// This script automatically connects to Photon (using the settings file),
/// tries to join a random room and creates one if none was found (which is ok).
/// </summary>
public class ConnectAndJoinRandom : Photon.MonoBehaviour
{
    private string version;

    /// <summary>if we don't want to connect in Start(), we have to "remember" if we called ConnectUsingSettings()</summary>
    private bool isConnect;

    public int SendRate;

    private Master m;

    private bool isScannable = false;
    private int totalFrames = 0;
    private RoomInfo[] latestRooms;
    public Text RoomPlayerCount;
    public Text RoomPlayerCountBG;

    private ShowStatusWhenConnecting netGUI;

    private bool isFirstTime; //first connect should be from main -> map, then same state if reconnect.
    private int savedState;
    private string waitingRoomName;
    private int serverPlayerMax;

    TypedLobby defaultLobby;

    public void Awake()
    {

        defaultLobby = new TypedLobby("default", LobbyType.Default);

        netGUI = GetComponent<ShowStatusWhenConnecting>();

        Debug.Log("Setting Send Rate to: " + SendRate);
        PhotonNetwork.sendRate = SendRate;
        PhotonNetwork.sendRateOnSerialize = SendRate;
        m = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
        version = m.Version;
    }

    public virtual void Start()
    {
        serverPlayerMax = 100;
        waitingRoomName = "Waiting"; 
        isFirstTime = true;
        IsConnect = false;
        PhotonNetwork.autoJoinLobby = false;    // we join randomly. always. no need to join a lobby to get the list of rooms.
    }

    public virtual void Update()
    {
        if (isConnect && !PhotonNetwork.connected)
        {
            Debug.Log("Update() was called by Unity. Scene is loaded. Let's connect to the Photon Master Server. Calling: PhotonNetwork.ConnectUsingSettings();");

            isConnect = false;
            PhotonNetwork.ConnectUsingSettings(version);
            PhotonNetwork.sendRate = SendRate;
            PhotonNetwork.sendRateOnSerialize = SendRate;
            //PhotonNetwork.ConnectToMaster("52.9.58.118", 5055,
            //    "d4e9e94d-de9a-44ec-9668-5f1898d4e76c",
            //    version); FOR USING PERSONAL SERVER

            savedState = m.CurrentLevel;
            m.GoTo(-1); //Disable menus while connecting is in progress.
        }

        if (!isScannable)
            return;

        totalFrames++;
        if (totalFrames % 60 == 0)
        {
            totalFrames = 0;
            CountTotalOnline();
        }
    }

    /// <summary>
    /// All "InNet" for all public functions relating to networking, but not a 
    /// </summary>
    public void SetCharacterInNet()
    {
        Debug.Log("Adding player property");
        ExitGames.Client.Photon.Hashtable playerProperties = PhotonNetwork.player.customProperties;
        ////Track each player's chosen character.
        if (playerProperties.ContainsKey("ChosenCharNum"))
        {
            playerProperties["ChosenCharNum"] = m.Client_CharNum;
        }
        else
        {
            playerProperties.Add("ChosenCharNum", m.Client_CharNum);
        }

        PhotonNetwork.player.SetCustomProperties(playerProperties);
    }
    // below, we implement some callbacks of PUN
    // you can find PUN's callbacks in the class PunBehaviour or in enum PhotonNetworkingMessage

    /// <summary>
    /// Note: NEED GUI FOR IF ROOM IS FULL.
    /// </summary>
    public void JoinMatchInNet()
    {
        PhotonNetwork.JoinOrCreateRoom(m.GetRoomName(), 
            new RoomOptions() { maxPlayers = Convert.ToByte(m.Max_Players) }, defaultLobby);
    }

    public virtual void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster() was called by PUN. Now this client is connected and could join a room. Calling: PhotonNetwork.JoinRandomRoom();");
        ////Get room properties, containing things such as player info and selected character.

        if (isFirstTime)
        {
            isFirstTime = false;
            m.GoTo(1);
        }else
        {
            m.GoTo(savedState);
        }

        PhotonNetwork.JoinLobby(defaultLobby);
        
    }

    public virtual void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby(). This client is connected and does get a room-list, which gets stored as PhotonNetwork.GetRoomList(). This script now calls: PhotonNetwork.JoinRandomRoom();");
        PhotonNetwork.JoinOrCreateRoom("Waiting", new RoomOptions() { maxPlayers = Convert.ToByte(serverPlayerMax) }, defaultLobby);
        isScannable = true;    
   }

    public virtual void OnPhotonRandomJoinFailed()
    {
        
        Debug.Log("OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one. Calling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 6}, null);");
    }

    // the following methods are implemented to give you some context. re-implement them as needed.

    public virtual void OnFailedToConnectToPhoton(DisconnectCause cause)
    {
        if (!m.GetServerIsEast())
        {
            PhotonNetwork.Disconnect();
            Debug.Log("Server Connecting Failed!");
            bool isEast = true;
            m.SetServer(isEast);
            m.GoBack();
        }
        else
        {
            Debug.LogError("Cause: " + cause);
        }
    }

    public void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom() called by PUN. Now this client is in a room. From here on, your game would be running. For reference, all callbacks are listed in enum: PhotonNetworkingMessage");
        //Get room properties, containing things such as player info and selected character.
        //ExitGames.Client.Photon.Hashtable playerProperties = PhotonNetwork.room.customProperties;
        //Get total number of players logged into room.
        int totalPlayersFound = PhotonNetwork.playerList.Length;
        PhotonNetwork.playerName = "Player " + totalPlayersFound;
        m.Player_Number = totalPlayersFound-1;
        if (!PhotonNetwork.room.customProperties.ContainsKey("GameStarted"))
        {
            ExitGames.Client.Photon.Hashtable tempTable = PhotonNetwork
                .room.customProperties;
            tempTable.Add("GameStarted", false);
            PhotonNetwork.room.SetCustomProperties(tempTable);
        }
        //Track each player's chosen character.
        //playerProperties.Add(PhotonNetwork.playerName, m.Client_CharNum);
    }

    void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        Debug.Log("OnPhotonPlayerDisconnected: " + player);

        //attempt reconnection.
        IsConnect = true;
    }

    private void CountTotalOnline()
    {
        int onlineCount = 0;
        latestRooms = PhotonNetwork.GetRoomList();
        Debug.Log("Inside Lobby? " + PhotonNetwork.insideLobby);
        Debug.Log("Nums : " + PhotonNetwork.LobbyStatistics[0].PlayerCount);
        for (int x = 0; x < latestRooms.Length; x++)
        {
            Debug.Log("Room " + latestRooms[x].name + " has " + latestRooms[x].playerCount + "players.");
            onlineCount += latestRooms[x].playerCount;
        }
        Debug.Log("There are a total of " + onlineCount + " online.");
        m.CurrentlyOnline = onlineCount;
    }

    public bool IsConnect
    {
        get
        {
            return isConnect;
        }

        set
        {
            isConnect = value;
            if (isConnect)
                netGUI.IsOnline = true;
        }
    }
}
