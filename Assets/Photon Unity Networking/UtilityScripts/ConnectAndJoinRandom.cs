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

    public Master m;
    private MatchHUD matchHUD;
    public MapSelectUIController mapUI;
    //private gameManager gameMan;

    private bool inLobby = false;
    private RoomInfo[] latestRooms;
    private int totalOnline;

    private int clientRoomSize;
    
    private int serverPlayerMax;
    private bool isEastServer;

    private float previousUpdateTime;
    public float ServerUpdateLength;
    
    private Dictionary<int, int> ID_to_SlotNum;
    private Dictionary<int, int> ID_to_CharNum;

    private TypedLobby inMatchLobby;

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
        
        Debug.Log("Setting Send Rate to: " + SendRate);
        PhotonNetwork.sendRate = SendRate;
        
        PhotonNetwork.sendRateOnSerialize = SendRate;
        

        version = m.Version; //Debug.isDebugBuild ? "test" : 
        isEastServer = PlayerPrefs.GetInt("Server", 0) == 1 ? true : false;
        //"Server" returns either 0=none, 1 = east, 2 = West
        //isEastServer won't be used if 0, and instead 'best' region is used.
  
        previousUpdateTime = Time.time;

        serverPlayerMax = 20;
        isConnectAllowed = false; //Enabled when server region given.

		Rooms = new List<List<room>> ();
        for(int x = 0; x < m.TotalUniqueArenas; x++)
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

        //Update self info tracking.
        if(!ID_to_CharNum.ContainsKey(PhotonNetwork.player.ID))
            ID_to_CharNum.Add(PhotonNetwork.player.ID, m.PlayerCharNum);
        else
            ID_to_CharNum[PhotonNetwork.player.ID] = m.PlayerCharNum;
    }

    /// <summary>
    /// Note: NEED GUI FOR IF ROOM IS FULL.
    /// </summary>
    public void JoinRoom()
    {
        PhotonNetwork.JoinOrCreateRoom(m.GetRoomName()+mapUI.getTargetRoom(), new RoomOptions() { MaxPlayers = Convert.ToByte(m.Max_Players) }, null);
    }

    public void CreateRoom()
    {
        PhotonNetwork.JoinOrCreateRoom(m.GetRoomName()+(Rooms[mapUI.getTargetArena()].Count), new RoomOptions() { MaxPlayers = Convert.ToByte(m.Max_Players) }, null);
    }

    public void assignMatchHUD()
    {
        matchHUD = GameObject.FindGameObjectWithTag("MatchHUD").GetComponent<MatchHUD>();
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
        m.GoBack();
    }

    public void OnLeftRoom()
    {
        Debug.Log("Left Room.");
    }

    public void OnJoinedRoom()
    {
        // All callbacks are listed in enum: PhotonNetworkingMessage.
        Debug.Log("On Joined Room: " + PhotonNetwork.room.name);
        if (PhotonNetwork.room.name == "Waiting")
        {
            PhotonNetwork.LeaveRoom();
            return;
        }
        inLobby = false;

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

        //Apply IsReady state. Begins false.
        PhotonNetwork.player.SetCustomProperties(tempTable);

        //Add info from players already logged in, including self.
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
            }else
            {
                ID_to_CharNum.Add(PhotonNetwork.player.ID, m.PlayerCharNum);
                ID_to_SlotNum.Add(PhotonNetwork.player.ID, PhotonNetwork.playerList.Length - 1);
            }
            clientRoomSize++;
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
        
        clientRoomSize++;

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
        clientRoomSize--;

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

    

    private void setServerStats()
    {
        totalOnline = PhotonNetwork.countOfPlayers;
        latestRooms = PhotonNetwork.GetRoomList();

        //empty out old rooms list
        for(int x = 0; x < m.TotalUniqueArenas; x++)
        {
            Rooms[x].Clear();
        }

        string roomNameTemp;
        RoomInfo roomInfoTemp;
        int roomSizeTemp;
        int[] totalRoomTypesTemp;
        totalRoomTypesTemp = new int[m.TotalUniqueArenas];
        for (int x = 0; x < latestRooms.Length; x++)
        {
            roomInfoTemp = latestRooms[x];
            roomNameTemp = roomInfoTemp.name;
            roomSizeTemp = roomInfoTemp.playerCount;

            Debug.Log("Room " + roomNameTemp + " has " + roomSizeTemp + " players.");
            for(int i = 0; i < m.TotalUniqueArenas; i++)
            {
                if (roomNameTemp.Contains(m.ArenaNames[i])){
                    totalRoomTypesTemp[i]++;
                    Rooms[i].Add(new room(roomSizeTemp, roomNameTemp + totalRoomTypesTemp[i]));
                }
            }
        }
        Debug.Log("There are a total of " + totalOnline + " online.");
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
            return clientRoomSize;
        }
    }

    public int TotalOnline
    {
        get
        {
            return totalOnline;
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
        inLobby = false;
    }

    public void LeaveRoom()
    {
        Debug.Log("Leaving Room.");
        PhotonNetwork.LeaveRoom();
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
