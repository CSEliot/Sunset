using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ReadyUp : MonoBehaviour {

    public GameObject SelectorYes;
    public GameObject SelectorNo;

    private bool isReady;
    private bool readyUped;

    private PhotonView m_PhotonView;

    private Sprite headSprite;

    public Image[] PlayerSlots;


	// Use this for initialization
	void Start () {
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
        }
        if (!readyUped && Input.GetButtonDown("Right") && isReady)
        {
            SelectorYes.SetActive(false);
            SelectorNo.SetActive(true);
        }
        if (!readyUped && Input.GetButtonDown("Submit") && isReady)
        {
            m_PhotonView.RPC("ShowReady", PhotonTargets.All);
        }
	}

    [PunRPC]
    void ShowReady()
    {
        
    }

    private void AddUser(Sprite headSprite)
    {

    }
}
