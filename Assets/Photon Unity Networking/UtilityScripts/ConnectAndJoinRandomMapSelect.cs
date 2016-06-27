using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// This script automatically connects to Photon (using the settings file),
/// tries to join a random room and creates one if none was found (which is ok).
/// </summary>
public class ConnectAndJoinRandomMapSelect : Photon.MonoBehaviour
{
    /// <summary>Connect automatically? If false you can set this to true later on or call ConnectUsingSettings in your own scripts.</summary>
    public bool AutoConnect = true;

    private string version;

    /// <summary>if we don't want to connect in Start(), we have to "remember" if we called ConnectUsingSettings()</summary>
    private bool ConnectInUpdate = true;

    public int SendRate;

    private Master m;

    private bool isScannable = false;
    private int totalFrames = 0;
    private RoomInfo[] latestRooms;
    public Text RoomPlayerCount;
    public Text RoomPlayerCountBG;

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
            version = m.Version;
        }
    }

    public virtual void Start()
    {
        PhotonNetwork.autoJoinLobby = false;    // we join randomly. always. no need to join a lobby to get the list of rooms.
    }

    public virtual void Update()
    {
        if (ConnectInUpdate && AutoConnect && !PhotonNetwork.connected 
            && m.GetServerIsEast())
        {
            Debug.Log("Update() was called by Unity. Scene is loaded. Let's connect to the Photon Master Server. Calling: PhotonNetwork.ConnectUsingSettings();");

            ConnectInUpdate = false;
            PhotonNetwork.ConnectUsingSettings(version);
            PhotonNetwork.sendRate = SendRate;
            PhotonNetwork.sendRateOnSerialize = SendRate;
        }else 
        if(ConnectInUpdate && AutoConnect && !PhotonNetwork.connected 
           && !m.GetServerIsEast())
        {
            Debug.Log("Update() was called by Unity. Scene is loaded. Let's connect to the Photon Master Server. Calling: PhotonNetwork.ConnectToMaster(); This is WEST COAST Server!");

            ConnectInUpdate = false;
            PhotonNetwork.ConnectToMaster("52.9.58.118", 5055,
                "d4e9e94d-de9a-44ec-9668-5f1898d4e76c", 
                version);
            PhotonNetwork.sendRate = SendRate;
            PhotonNetwork.sendRateOnSerialize = SendRate;
        }

        totalFrames++;
        if(isScannable && totalFrames % 60 == 0)
        {
            latestRooms = PhotonNetwork.GetRoomList();
            for (int x = 0; x < latestRooms.Length; x++) {
                Debug.Log("Room name: " + latestRooms[x].name);
                if(m.GetRoomName() == latestRooms[x].name)
                {
                    RoomPlayerCount.text = latestRooms[x].playerCount + " / 6 Max Players";
                    RoomPlayerCountBG.text = latestRooms[x].playerCount + " / 6 Max Players";
                }
            } 
        }
    }


    // below, we implement some callbacks of PUN
    // you can find PUN's callbacks in the class PunBehaviour or in enum PhotonNetworkingMessage

    
    public virtual void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster() was called by PUN. Now this client is connected and could join a room. Scanning rooms . . .");

        isScannable = true;
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
            SceneManager.LoadScene("GameScreen");
        }
        else
        {
            Debug.LogError("Cause: " + cause);
        }
    }

    void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        Debug.Log("OnPhotonPlayerDisconnected: " + player);
    }
}
