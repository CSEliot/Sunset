using UnityEngine;
using System.Collections;

public class ReadyUp : MonoBehaviour {

    public GameObject SelectorYes;
    public GameObject SelectorNo;

    private bool isReady;
    private bool readyUped;

    public PhotonView m_PhotonView;

    private Sprite headSprite;

    public GameObject[] PlayerSlots;


	// Use this for initialization
	void Start () {
        PlayerSlots = new GameObject[6];
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
        if (!readyUped && Input.GetButtonDown("Submit"))
        {
            m_PhotonView.RPC("ShowReady", PhotonTargets.All);
        }
	}

    [PunRPC]
    void ShowReady()
    {
        //Add
    }

    private void AddUser(Sprite headSprite)
    {

    }
}
