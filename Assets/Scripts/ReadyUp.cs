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

    private Sprite headSprite;

    public Sprite Empty;

    public Image[] PlayerSlots;

    private int totalLoggedIn;

    private Dictionary<int, int> ID_to_ReadyNum;

	// Use this for initialization
	void Start () {
        ID_to_ReadyNum = new Dictionary<int, int>();
        totalLoggedIn = 0;
        m_PhotonView = GetComponent<PhotonView>();
        isReady = false;
        readyUped = false;
        headSprite = GameObject.FindGameObjectWithTag("SpawnPoints")
            .GetComponent<OnJoinedInstantiate>().GetImage();
	}
	
	// Update is called once per frame
	void Update () {
        
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
            m_PhotonView.RPC("ShowReady", PhotonTargets.All, headSprite);
            SelectorYes.SetActive(false);
        }
	}

    [PunRPC]
    void ShowReady(Sprite playerSprite)
    {
        PlayerSlots[totalLoggedIn].sprite = playerSprite;

        readyUped = true;
    }

    void OnJoinedRoom()
    {
        Debug.Log("GOGOGO");
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    { 
    }

    void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        Debug.Log("Player Connected!" + player.ID);
        ID_to_ReadyNum.Add(player.ID, totalLoggedIn);
        totalLoggedIn++;
    }

    void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        totalLoggedIn--;
    }


}
