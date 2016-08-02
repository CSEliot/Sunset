using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CharacterSelectUIController : MonoBehaviour {


    public Text Name1;
    public Text Name2;

    public Sprite[] AllSprites;

    public Image StatFrame;
    public Sprite[] AllStatSprites;

    private ConnectAndJoinRandom n;
    private Master m;
    public string[] CharNames;

    public Image LeftFrame;
    public Image MidFrame;
    public Image RightFrame;

    private int currentSelector;
    private int chosenChar;

	// Use this for initialization
	void Awake () {
        currentSelector = 1;

        n = GameObject.FindGameObjectWithTag("Networking").GetComponent<ConnectAndJoinRandom>();
        m = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
        PhotonNetwork.Disconnect();
	}
	
	// Update is called once per frame
	void Update () {
	}

    public void ShiftSelection(bool shiftLeft)
    {
        if (shiftLeft && currentSelector == 1)
            return;
        if (!shiftLeft && currentSelector == 6)
            return;
        m.PlaySFX(5);

        currentSelector += shiftLeft ? -1 : 1;

        chosenChar = currentSelector - 1; //char num is 0 - 5, currentSelector is 1 - 6;

        //Reassign sprite images.
        LeftFrame.sprite = AllSprites[currentSelector - 1];
        MidFrame.sprite = AllSprites[currentSelector];
        RightFrame.sprite = AllSprites[currentSelector + 1];
        //Reassign text names.
        Name1.text = CharNames[currentSelector - 1];
        Name2.text = CharNames[currentSelector - 1];
        //Reassign stat frame.
        StatFrame.sprite = AllStatSprites[currentSelector - 1];
        //Tell Master (local) and Network (online) the character selection.
        m.AssignPlayerCharacter(chosenChar);
        n.SetCharacterInNet(chosenChar); 
    }
}
