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
public class ConnectAndJoinRandom : Photon.MonoBehaviour{
    private string version;

    /// <summary>if we don't want to connect in Start(), we have to "remember" if we called ConnectUsingSettings()</summary>
    private bool isConnectAllowed;

    private bool doConnect; // DELETE ME

    public int SendRate;

    private Master m;
    private MatchHUD matchHUD;
    //private gameManager gameMan;

    private bool inLobby = false;
    private RoomInfo[] latestRooms;
    public Text RoomPlayerCount;
    public Text RoomPlayerCountBG;
    public Text TotalPlayerCount;
    public Text TotalPlayerCountBG;
    private string totalPlayerCountStr;
    private int onlineLobbyTotal;
    private int onlineRoomTotal;

    private bool isFirstTimeConnect; //first connect should be from main -> map, then same state if reconnect.
    private int savedState;
    private string waitingRoomName;
    private int serverPlayerMax;
    private bool isEastServer;

    private float previousUpdateTime;
    public float ServerUpdateLength;
    
    private Dictionary<int, int> ID_to_SlotNum;
    private Dictionary<int, int> ID_to_CharNum;

    private TypedLobby inMatchLobby;

    void Start()
    {

        doConnect = false;
        ID_to_SlotNum = new Dictionary<int, int>();
        ID_to_CharNum = new Dictionary<int, int>();
        
        Debug.Log("Setting Send Rate to: " + SendRate);
        PhotonNetwork.sendRate = SendRate;
        
        PhotonNetwork.sendRateOnSerialize = SendRate;
        m = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
        
        version = m.Version; //Debug.isDebugBuild ? "test" : 
        isEastServer = PlayerPrefs.GetInt("Server", 0) == 1 ? true : false;
        //"Server" returns either 0=none, 1 = east, 2 = West
        //isEastServer won't be used if 0, and instead 'best' region is used.
  
        previousUpdateTime = Time.time;

        serverPlayerMax = 100;
        waitingRoomName = "Waiting"; 
        isFirstTimeConnect = true;
        isConnectAllowed = false; //Enabled when server region given.

        PhotonNetwork.autoJoinLobby = true;

        inMatchLobby = new TypedLobby("inMatch", LobbyType.Default);
    }

    public void Update()
    {
        if (isConnectAllowed && !PhotonNetwork.connected)
        {
            Debug.Log("Beginning Connection . . .");

            isConnectAllowed = false;
            if(PlayerPrefs.GetInt("Server", 0) == 0)
            {
                PhotonNetwork.ConnectToBestCloudServer(version);
                Debug.Log("Connecting to Best Region.");
            }
            else
            {
                PhotonNetwork.ConnectToRegion(isEastServer ? CloudRegionCode.us : CloudRegionCode.usw, version);
                Debug.Log("Connecting to Chosen Region.");
            }

            Debug.Log("Version Number is: " + version);
            PhotonNetwork.sendRate = SendRate;
            PhotonNetwork.sendRateOnSerialize = SendRate;
            //PhotonNetwork.ConnectToMaster("52.9.58.118", 5055,
            //    "d4e9e94d-de9a-44ec-9668-5f1898d4e76c",
            //    version); FOR USING PERSONAL SERVER

            savedState = m.CurrentMenu;
            //m.GoTo(-1); //Disable menus while connecting is in progress.
        }

        if (!PhotonNetwork.connectedAndReady && PhotonNetwork.connecting)
            printStatus();

        //if(!inLobby)
        //    return;

        if (Time.time - previousUpdateTime > ServerUpdateLength)
        {
            previousUpdateTime = Time.time;
            countTotalOnline();
        }

        totalPlayerCountStr = "Players Online\n" + (onlineLobbyTotal);
        TotalPlayerCount.text = totalPlayerCountStr;
        TotalPlayerCountBG.text = totalPlayerCountStr;
    }

    public void DELETEME()
    {
        Debug.Log("SAVE ME");
        PhotonNetwork.JoinOrCreateRoom(m.GetRoomName(), new RoomOptions() { MaxPlayers = Convert.ToByte(m.Max_Players) }, null);
        //StartCoroutine(DELETEMETOO());
    }

    private IEnumerator DELETEMETOO()
    {
        Debug.Log("SAVE MEEEE");
        yield return new WaitForSeconds(1f);
        Debug.Log("SAVE MEEE222E");

    }

    /// <summary>
    /// All "InNet" for all public functions relating to networking, but not a Photon function.
    /// </summary>
    public void SetCharacterInNet(int chosenChar)
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
        if(!ID_to_CharNum.ContainsKey(PhotonNetwork.player.ID))
            ID_to_CharNum.Add(PhotonNetwork.player.ID, m.PlayerCharNum);
        else
            ID_to_CharNum[PhotonNetwork.player.ID] = m.PlayerCharNum;
        if (!ID_to_SlotNum.ContainsKey(PhotonNetwork.player.ID))
            ID_to_SlotNum.Add(PhotonNetwork.player.ID, PhotonNetwork.playerList.Length - 1);
        else
            ID_to_SlotNum[PhotonNetwork.player.ID] = PhotonNetwork.playerList.Length - 1;
    }

    /// <summary>
    /// Note: NEED GUI FOR IF ROOM IS FULL.
    /// </summary>
    public void JoinRoom()
    {
        Debug.Log("Attempt Lobbi con");
        DELETEME();
    }

    public void assignMatchHUD()
    {
        matchHUD = GameObject.FindGameObjectWithTag("MatchHUD").GetComponent<MatchHUD>();
    }

    public void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster() was succesfull.");
        ////Get room properties, containing things such as player info and selected character.

        //if (isFirstTimeConnect)
        //{
        //    isFirstTimeConnect = false;
        //}else
        //{
        //    m.GoTo(savedState);
        //}
    }

    public void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby() was a success.");
        doConnect = true;
        //if(PhotonNetwork.lobby.Name == "inMatch")
            //PhotonNetwork.JoinOrCreateRoom(m.GetRoomName(), new RoomOptions() { MaxPlayers = Convert.ToByte(m.Max_Players) }, null);
        //else
        //    PhotonNetwork.JoinOrCreateRoom("Waiting", new RoomOptions() { MaxPlayers = Convert.ToByte(serverPlayerMax) }, null);

        inLobby = true;
        //if (isFirstTimeConnect)
        //{
        //    isFirstTimeConnect = false;
        //    m.GoTo(1); // go to map select on first game load.
        //}
        //else
        //{
        //    m.GoTo(savedState);
        //}
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

    public void OnLeftRoom()
    {
        Debug.Log("Left!");
        //PhotonNetwork.JoinOrCreateRoom(m.GetRoomName(), new RoomOptions() { MaxPlayers = Convert.ToByte(serverPlayerMax) }, null);
    }

    public void OnJoinedRoom()
    {
        // All callbacks are listed in enum: PhotonNetworkingMessage.
        Debug.Log("On Joined Room: " + PhotonNetwork.room.name);
        //if (PhotonNetwork.room.name == "Waiting")
        //{
        //    PhotonNetwork.LeaveRoom();
        //    return;
        //}

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
            matchHUD.ActivateSpectating();
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
        int playerID;
        for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
        {
            playerID = PhotonNetwork.playerList[i].ID;
            //If ID doesn't match your local Client ID, then it's another Player.
            if (PhotonNetwork.player.ID != playerID)
            {
                //Add other players to info tracking.
                if (!ID_to_CharNum.ContainsKey(playerID))
                    ID_to_CharNum.Add(playerID, (int)PhotonNetwork.playerList[i].customProperties["characterNum"]);
                else
                    ID_to_CharNum[playerID] = (int)PhotonNetwork.playerList[i].customProperties["characterNum"];
                if (!ID_to_SlotNum.ContainsKey(playerID))
                    ID_to_SlotNum.Add(playerID, i);
                else
                    ID_to_SlotNum[playerID] = i;
            }
            onlineRoomTotal++;
        }     
    }

    void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        matchHUD.AddPlayer(player);

        //Add other players to info tracking.
        if (!ID_to_CharNum.ContainsKey(player.ID))
            ID_to_CharNum.Add(player.ID, (int)player.customProperties["characterNum"]);
        else
            ID_to_CharNum[player.ID] = (int)player.customProperties["characterNum"];
        if (!ID_to_SlotNum.ContainsKey(player.ID))
            ID_to_SlotNum.Add(player.ID, PhotonNetwork.playerList.Length - 1);
        else
            ID_to_SlotNum[player.ID] = PhotonNetwork.playerList.Length - 1;
        
        onlineRoomTotal++;

        setReadyStatusInNet(false);
    }

    void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        Debug.Log("OnPhotonPlayerDisconnected: " + player);

        //attempt reconnection.
        isConnectAllowed = true;

        int otherID = player.ID;
        ID_to_SlotNum.Remove(otherID);
        ID_to_CharNum.Remove(otherID);
        onlineRoomTotal--;

        setReadyStatusInNet(false);
    }

    public void JoinServer(bool attempt)
    {
        if(attempt)
            isConnectAllowed = true;
        else
        {
            isConnectAllowed = false;
            PhotonNetwork.Disconnect();
        }
    }

    

    private void countTotalOnline()
    {
        int tempOnlineCount = PhotonNetwork.countOfPlayers;
        latestRooms = PhotonNetwork.GetRoomList();

        Debug.Log("Is online? " + PhotonNetwork.connected);
        if (PhotonNetwork.insideLobby)
        {
            Debug.Log("Inside Lobby? " + PhotonNetwork.insideLobby);
            Debug.Log("Total Lobbies: " + PhotonNetwork.LobbyStatistics.Count);
            Debug.Log("Lobby Name: " + PhotonNetwork.LobbyStatistics[PhotonNetwork.LobbyStatistics.Count-1].Name);
            Debug.Log("Nums : " + PhotonNetwork.LobbyStatistics[PhotonNetwork.LobbyStatistics.Count - 1].PlayerCount);
            tempOnlineCount = PhotonNetwork.LobbyStatistics[PhotonNetwork.LobbyStatistics.Count - 1].PlayerCount;
            Debug.Log("Lobby Type: " + PhotonNetwork.LobbyStatistics[PhotonNetwork.LobbyStatistics.Count - 1].Type);
            Debug.Log("Lobby Room Total: " + PhotonNetwork.LobbyStatistics[PhotonNetwork.LobbyStatistics.Count - 1].RoomCount);
            Debug.Log("Lobby Default? " + PhotonNetwork.LobbyStatistics[PhotonNetwork.LobbyStatistics.Count - 1].IsDefault);
        }else
        {
            Debug.Log("Room Name: " + PhotonNetwork.room.name);
            Debug.Log("Room Total Players: " + PhotonNetwork.room.playerCount + "/" + PhotonNetwork.room.maxPlayers);
        }


        for (int x = 0; x < latestRooms.Length; x++)
        {
            Debug.Log("Room " + latestRooms[x].name + " has " + latestRooms[x].playerCount + "players.");
        }
        Debug.Log("There are a total of " + tempOnlineCount + " online.");
        onlineLobbyTotal = tempOnlineCount;
    }

    public bool IsEastServer
    {
        get
        {
            return isEastServer;
        }

        set
        {
            isEastServer = value;
            PlayerPrefs.SetInt("Server", value ? 1 : 2);
            PlayerPrefs.Save();
        }
    }

    public void ForgetServer()
    {
        PlayerPrefs.DeleteKey("Server");
    }

    public int GetInRoomTotal
    {
        get
        {
            return onlineRoomTotal;
        }
    }

    public int GetCharNum(int logInID)
    {
        return ID_to_CharNum[logInID];
    }

    public int GetSlotNum(int logInID)
    {
       return ID_to_SlotNum[logInID]; ;
    }

    public void LeaveServer()
    {
        Debug.Log("Leaving server . . .");
        PhotonNetwork.Disconnect();
        isFirstTimeConnect = true;
        inLobby = false;
    }

    private void printStatus()
    {
        Debug.Log("Connection Status: " + PhotonNetwork.connectionStateDetailed);
    }

    private void setReadyStatusInNet(bool isReady)
    {
        ExitGames.Client.Photon.Hashtable tempPlayerTable = PhotonNetwork.player.customProperties;
        tempPlayerTable["IsReady"] = isReady;
        PhotonNetwork.player.SetCustomProperties(tempPlayerTable);
    }
}
