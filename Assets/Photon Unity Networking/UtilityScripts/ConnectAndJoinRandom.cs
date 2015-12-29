using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// This script automatically connects to Photon (using the settings file),
/// tries to join a random room and creates one if none was found (which is ok).
/// </summary>
public class ConnectAndJoinRandom : Photon.MonoBehaviour
{
    /// <summary>Connect automatically? If false you can set this to true later on or call ConnectUsingSettings in your own scripts.</summary>
    public bool AutoConnect = true;

    public byte Version = 1;

    /// <summary>if we don't want to connect in Start(), we have to "remember" if we called ConnectUsingSettings()</summary>
    private bool ConnectInUpdate = true;

    public int SendRate;

    private Master m;

    public void Awake()
    {
        if (GameObject.FindGameObjectWithTag("Master") == null)
        {
            SceneManager.LoadScene("CharacterSelect");
        }
        else
        {
            Debug.Log("Setting Send Rate to: " + SendRate);
            PhotonNetwork.sendRate = SendRate;
            PhotonNetwork.sendRateOnSerialize = SendRate;
            m = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
        }
    }

    public virtual void Start()
    {
        PhotonNetwork.autoJoinLobby = false;    // we join randomly. always. no need to join a lobby to get the list of rooms.
    }

    public virtual void Update()
    {
        if (ConnectInUpdate && AutoConnect && !PhotonNetwork.connected)
        {
            Debug.Log("Update() was called by Unity. Scene is loaded. Let's connect to the Photon Master Server. Calling: PhotonNetwork.ConnectUsingSettings();");

            ConnectInUpdate = false;
            PhotonNetwork.ConnectUsingSettings(Version + "."+
                SceneManager.GetActiveScene().buildIndex);
        }
    }


    // below, we implement some callbacks of PUN
    // you can find PUN's callbacks in the class PunBehaviour or in enum PhotonNetworkingMessage

    
    public virtual void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster() was called by PUN. Now this client is connected and could join a room. Calling: PhotonNetwork.JoinRandomRoom();");
        ////Get room properties, containing things such as player info and selected character.
        
        Debug.Log("Adding player property");
        ExitGames.Client.Photon.Hashtable playerProperties = PhotonNetwork.player.customProperties;
        ////Track each player's chosen character.
        if (playerProperties.ContainsKey("ChosenCharNum"))
        {
            playerProperties["ChosenCharNum"] = m.Client_CharNum;
        }
        else { 
            playerProperties.Add("ChosenCharNum", m.Client_CharNum);
        }

        PhotonNetwork.player.SetCustomProperties(playerProperties);
        PhotonNetwork.JoinRandomRoom();        
    }

    public virtual void OnJoinedLobby()
    {
        Debug.Log("OnJoinedLobby(). This client is connected and does get a room-list, which gets stored as PhotonNetwork.GetRoomList(). This script now calls: PhotonNetwork.JoinRandomRoom();");
        PhotonNetwork.JoinRandomRoom();
    }

    public virtual void OnPhotonRandomJoinFailed()
    {
        Debug.Log("OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one. Calling: PhotonNetwork.CreateRoom(null, new RoomOptions() {maxPlayers = 4}, null);");
        PhotonNetwork.CreateRoom(null, new RoomOptions() { maxPlayers = Convert.ToByte(m.Max_Players) }, null);
    }

    // the following methods are implemented to give you some context. re-implement them as needed.

    public virtual void OnFailedToConnectToPhoton(DisconnectCause cause)
    {
        Debug.LogError("Cause: " + cause);
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
    }
}
