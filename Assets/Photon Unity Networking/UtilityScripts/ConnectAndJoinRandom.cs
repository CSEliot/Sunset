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
    
    public int SendRate;

    public Master M;
    public MapSelectUIController MapUI;
    public PhotonView M_PhotonView;

    private bool inLobby = false;
    private RoomInfo[] latestRooms;
    private int totalOnline;
    
    private int serverPlayerMax;
    private bool isEastServer;

    private float previousUpdateTime;
    public float ServerUpdateLength;
    
    private Dictionary<int, int> ID_to_SlotNum;
    private Dictionary<int, int> ID_to_CharNum;
    private Dictionary<int, bool> ID_to_IsRdy;

    private TypedLobby inMatchLobby;

    #region Room State Tracking
    private bool gameStarted;
    private int readyTotal;
    private bool rdyStateChange = true;
    private bool charStateChange = true;
    private bool plrStateChange = true;
    private bool startMatch;
    #endregion

    public enum ReadyCount
    {
        Add,
        Subtract,
        Reset
    };

    /// <summary>
    /// For storing online room data locally.
    /// </summary>
    public struct room
    {
        /// <summary>
        /// Current number of players in room. Max: 6
        /// </summary>
        public int size;

        /// <summary>
        /// Unique identifier. 
        /// </summary>
        public string name;

        public room(int size, string name)
        {
            this.size = size;
            this.name = name;
        }
    }

    /// <summary>
    /// room[0] = Pillar, 1 = Void, 2 = Lair
    /// </summary>
    public List<List<room>> Rooms;

    void Start()
    {
        
        ID_to_SlotNum = new Dictionary<int, int>();
        ID_to_CharNum = new Dictionary<int, int>();
        ID_to_IsRdy = new Dictionary<int, bool>();
        
        Debug.Log("Setting Send Rate to: " + SendRate);
        PhotonNetwork.sendRate = SendRate;
        
        PhotonNetwork.sendRateOnSerialize = SendRate;
        

        version = M.Version; //Debug.isDebugBuild ? "test" : 
        isEastServer = PlayerPrefs.GetInt("Server", 0) == 1 ? true : false;
        //"Server" returns either 0=none, 1 = east, 2 = West
        //isEastServer won't be used if 0, and instead 'best' region is used.
  
        previousUpdateTime = Time.time;

        serverPlayerMax = 20;
        isConnectAllowed = false; //Enabled when server region given.

		Rooms = new List<List<room>> ();
        for(int x = 0; x < M.TotalUniqueArenas; x++)
        {
            Rooms.Add(new List<room>());
            Rooms[x].Clear();
        }
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
        }

        if (!PhotonNetwork.connectedAndReady && PhotonNetwork.connecting)
            printStatus();

        if (!PhotonNetwork.connected) 
            return;

        if (Time.time - previousUpdateTime > ServerUpdateLength)
        {
            previousUpdateTime = Time.time;
            setServerStats();
        }

        //Debug.Log("CHAR NUM: " + ID_to_CharNum[1]);

        #region Match Tracking
        if (readyTotal >= PhotonNetwork.playerList.Length && !startMatch)
        {
            ExitGames.Client.Photon.Hashtable tempRoomTable = PhotonNetwork.room.customProperties;
            tempRoomTable["GameStarted"] = true;
            PhotonNetwork.room.SetCustomProperties(tempRoomTable);
            startMatch = true;
            M.GameStarts(readyTotal);
        }
        #endregion
    }

    /// <summary>
    /// Shows character a player is hovering over.
    /// </summary>
    public void SetCharacter()
    {
        Debug.Log("Adding player property");
        ExitGames.Client.Photon.Hashtable playerProperties = PhotonNetwork.player.customProperties;
        //Update local player's chosen character online.
        if (playerProperties.ContainsKey("characterNum"))
        {
            playerProperties["characterNum"] = M.PlayerCharNum;
        }
        else
        {
            playerProperties.Add("characterNum", M.PlayerCharNum);
        }

        PhotonNetwork.player.SetCustomProperties(playerProperties);
        SetCharDisplay(PhotonTargets.All, M.PlayerCharNum);

    }

    /// <summary>
    /// Note: NEED GUI FOR IF ROOM IS FULL.
    /// </summary>
    public void JoinRoom()
    {
        string roomNameTemp = Rooms[MapUI.getTargetArena()][MapUI.getTargetRoom()].name;
        Debug.Log("Joining room named: " + roomNameTemp);
        PhotonNetwork.JoinRoom(roomNameTemp);
    }

    public void OnPhotonJoinRoomFailed()
    {
        MapUI.FullRoomWarning.SetActive(true);
    }

    public void CreateRoom()
    {
        int randInt = UnityEngine.Random.Range(0, 10000);
        string roomName = M.GetArenaName() + (Rooms[MapUI.getTargetArena()].Count + randInt);
        PhotonNetwork.JoinOrCreateRoom(roomName, new RoomOptions() { MaxPlayers = Convert.ToByte(M.Max_Players) }, null);
    }

    public void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster() was succesfull.");
    }

    public void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby() was a success.");
        //PhotonNetwork.JoinOrCreateRoom("Waiting", new RoomOptions() { MaxPlayers = Convert.ToByte(serverPlayerMax) }, null);

        inLobby = true;
    }

    public void OnLeftLobby()
    {
        Debug.Log("Left lobby.");
        inLobby = true;
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
        M.GoBack();
    }

    public void OnLeftRoom()
    {
        Debug.Log("Left Room.");
    }

    public void OnDisconnectedFromPhoton()
    {
        
    }

    public void OnJoinedRoom()
    {
        // All callbacks are listed in enum: PhotonNetworkingMessage.
        Debug.Log("On Joined Room: " + PhotonNetwork.room.name);
        inLobby = false;
        //As a new player, we must assign default player properties.
        //SetCharacter();

        //Get total number of players logged into room.
        int totalPlayersFound = PhotonNetwork.playerList.Length;
        PhotonNetwork.playerName = "Player " + totalPlayersFound;
        M.InRoomNumber = totalPlayersFound - 1;

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
            return;
        }
        
        //if we join in and game is ready to start, un-ready,
        //because a new player forces all players to re-ready.
        tempTable = PhotonNetwork.player.customProperties;
        if (!tempTable.ContainsKey("IsReady"))
            tempTable.Add("IsReady", false);
        else
            tempTable["IsReady"] = false;

        //Apply IsReady state. Begins false.
        PhotonNetwork.player.SetCustomProperties(tempTable);


        clearLocalTracking();
        //Add info from players already logged in, including self.
        int playerID;
        for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
        {
            playerID = PhotonNetwork.playerList[i].ID;
            storeLocalData(playerID, i);
        }

        //Tell everyone to reset their ready status.
        SetReadyStatus(PhotonTargets.MasterClient, ReadyCount.Reset);
        readyTotal = 0;
    }

    void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        //Add new player info tracking.
        storeRemoteData(player);
        Debug.Log("OnPhotonPlayerConnected: " + player);
        //Tell everyone to reset their ready status.
        SetReadyStatus(PhotonTargets.Others, ReadyCount.Reset);

    }

    void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        Debug.Log("OnPhotonPlayerDisconnected: " + player);
        //Tell everyone to reset their ready status.
        SetReadyStatus(PhotonTargets.Others, ReadyCount.Reset);
        
        unassignPlayerTracking(player);
        plrStateChange = true;
        rdyStateChange = true;
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


    private void setServerStats()
    {
        totalOnline = PhotonNetwork.countOfPlayers;
        latestRooms = PhotonNetwork.GetRoomList();

        //empty out old rooms list
        for(int x = 0; x < M.TotalUniqueArenas; x++)
        {
            Rooms[x].Clear();
        }

        string roomNameTemp;
        RoomInfo roomInfoTemp;
        int roomSizeTemp;
        for (int x = 0; x < latestRooms.Length; x++)
        {
            roomInfoTemp = latestRooms[x];
            roomNameTemp = roomInfoTemp.name;
            roomSizeTemp = roomInfoTemp.playerCount;

            Debug.Log("Room " + roomNameTemp + " has " + roomSizeTemp + " players.");
            for(int i = 0; i < M.TotalUniqueArenas; i++)
            {
                if (roomNameTemp.Contains(M.ArenaNames[i])){
                    Rooms[i].Add(new room(roomSizeTemp, roomNameTemp));
                }
            }
        }
        //Debug.Log("There are a total of " + totalOnline + " online.");
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
            return PhotonNetwork.room.playerCount;
        }
    }

    public int TotalOnline
    {
        get
        {
            return totalOnline;
        }
    }

    public bool GameStarted
    {
        get
        {
            return gameStarted;
        }
    }

    public int ReadyTotal
    {
        get
        {
            return readyTotal;
        }
    }

    public bool StartMatch
    {
        get
        {
            if (startMatch)
            {
                startMatch = false;
                return true;
            }
            else
                return false;
        }
    }

    public bool GetPlayerStateChange()
    {
        if(plrStateChange)
        {
            plrStateChange = false;
            return true;
        }
        return plrStateChange;
    }

    public bool GetCharStateChange()
    { 
        if (charStateChange)
        {
            charStateChange = false;
            return true;
        }
        return charStateChange;    
    }

    public bool GetRdyStateChange()
    {
        if (rdyStateChange)
        {
            rdyStateChange = false;
            return true;
        }
        return rdyStateChange;
    }

    public int GetCharNum(int logInID)
    {
        return ID_to_CharNum[logInID];
    }

    public int GetSlotNum(int logInID)
    {
       return ID_to_SlotNum[logInID];
    }

    public bool GetRdyStatus(int logInID)
    {
        return ID_to_IsRdy[logInID];
    }

    public void LeaveServer()
    {
        Debug.Log("Leaving server . . .");
        PhotonNetwork.Disconnect(); 
        inLobby = false;
    }

    public void LeaveRoom()
    {
        Debug.Log("Leaving Room.");
        unassignPlayerTracking(PhotonNetwork.player);
        PhotonNetwork.LeaveRoom();
    }

    public void SetReadyStatus(PhotonTargets tellWho, ReadyCount action)
    {
        M_PhotonView.RPC("ShowReadyStatus", tellWho, PhotonNetwork.player.ID, action);
    }

    public void SetCharDisplay(PhotonTargets tellWho, int newChar)
    {
        M_PhotonView.RPC("ShowNewChar", tellWho, PhotonNetwork.player.ID, newChar);
    }

    public void ReadyButton()
    {
        ExitGames.Client.Photon.Hashtable tempPlayerTable = PhotonNetwork.player.customProperties;
        tempPlayerTable["IsReady"] = true;
        PhotonNetwork.player.SetCustomProperties(tempPlayerTable);
        SetReadyStatus(PhotonTargets.All, ReadyCount.Add);
    }

    public void UnreadyButton()
    {
        ExitGames.Client.Photon.Hashtable tempPlayerTable = PhotonNetwork.player.customProperties;
        tempPlayerTable["IsReady"] = false;
        PhotonNetwork.player.SetCustomProperties(tempPlayerTable);
        SetReadyStatus(PhotonTargets.All, ReadyCount.Subtract);
    }

    [PunRPC]
    private void ShowReadyStatus(int playerNum, ReadyCount action)
    {

        readyTotal = action == ReadyCount.Add ? ++readyTotal : --readyTotal;
        readyTotal = action == ReadyCount.Reset ? 0 : readyTotal;

        ID_to_IsRdy[playerNum] = action == ReadyCount.Add;
        rdyStateChange = true;
    }

    [PunRPC]
    private void ShowNewChar(int playerID, int newChar)
    {
        //Update local player's chosen character locally.
        ID_to_CharNum[playerID] = newChar;
        charStateChange = true;
        if (Master.DEBUG_ON)
            Debug.Log(string.Format("Received PlayerID: {0} to Char: {1}", playerID, newChar));
    }

    private void printStatus()
    {
        Debug.Log("Connection Status: " + PhotonNetwork.connectionStateDetailed);
    }

    /// <summary>
    /// Helper function for OnJoinedRoom. Called while iterating through current list of players
    /// in the room.
    /// </summary>
    /// <param name="playerID">Unique Player ID</param>
    /// <param name="playerNum">i'th player in room</param>
    private void storeLocalData(int playerID, int playerNum)
    {
        int playerChar;
        if (PhotonNetwork.playerList[playerNum].customProperties.ContainsKey("characterNum"))
            playerChar = (int)PhotonNetwork.playerList[playerNum].customProperties["characterNum"];
        else
            playerChar = 0;

        //Add other players to info tracking.
        if (!ID_to_CharNum.ContainsKey(playerID))
            ID_to_CharNum.Add(playerID, playerChar);
        else
            ID_to_CharNum[playerID] = playerChar;

        if (!ID_to_SlotNum.ContainsKey(playerID))
            ID_to_SlotNum.Add(playerID, playerNum);
        else
            ID_to_SlotNum[playerID] = playerNum;

        if (!ID_to_IsRdy.ContainsKey(playerID))
            ID_to_IsRdy.Add(playerID, false);
        else
            ID_to_IsRdy[playerID] = false;

        plrStateChange = true;
        rdyStateChange = true;
        charStateChange = true;

        if (Master.DEBUG_ON)
            Debug.Log(string.Format("Assigning Local Data. ID: {0} Slot: {1}", playerID, playerNum));

    }

    /// <summary>
    /// Helper function called when a new player joins after you.
    /// </summary>
    /// <param name="player">New player that connected.</param>
    private void storeRemoteData(PhotonPlayer player)
    {
        int playerChar;
        int playerID = player.ID;
        if (player.customProperties.ContainsKey("characterNum"))
            playerChar = (int)player.customProperties["characterNum"];
        else
            playerChar = 0;

        //Add player to info tracking.
        if (!ID_to_CharNum.ContainsKey(playerID))
            ID_to_CharNum.Add(playerID, playerChar);
        else
            ID_to_CharNum[playerID] = playerChar;

        if (!ID_to_SlotNum.ContainsKey(playerID))
            ID_to_SlotNum.Add(playerID, PhotonNetwork.playerList.Length - 1);
        else
            ID_to_SlotNum[playerID] = PhotonNetwork.playerList.Length - 1;

        if (!ID_to_IsRdy.ContainsKey(playerID))
            ID_to_IsRdy.Add(playerID, false);
        else
            ID_to_IsRdy[playerID] = false;
        if (Master.DEBUG_ON)
            Debug.Log(string.Format("New Player Connected! ID: {0} Char: {1}", playerID, playerChar));
    }

    /// <summary>
    /// Helper function called when a player leaves.
    /// </summary>
    /// <param name="player">player that disconnected.</param>
    private void unassignPlayerTracking(PhotonPlayer player)
    {
        int playerID = player.ID;
        ID_to_CharNum.Remove(playerID);
        ID_to_IsRdy.Remove(playerID);
        ID_to_SlotNum.Remove(playerID);
    }

    private void clearLocalTracking( )
    {
        ID_to_CharNum.Clear();
        ID_to_IsRdy.Clear();
        ID_to_SlotNum.Clear();
    }

}
