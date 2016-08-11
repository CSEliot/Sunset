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

    public Button LeftFrame_B;
    public Button MidFrame_B;
    public Button RightFrame_B;
    public Image LeftFrame_I;
    public Image MidFrame_I;
    public Image RightFrame_I;
    public GameObject LeftArrow;
    public GameObject RightArrow;

    private int currentSelector;
    private int chosenChar;

    public Button Select;

	// Use this for initialization
	void Awake () {
        n = GameObject.FindGameObjectWithTag("Networking").GetComponent<ConnectAndJoinRandom>();
        m = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
	}
    
    void OnEnable ()
    {
        resetUI();
    }
	
	// Update is called once per frame
	void Update () {

	}

    public void ShiftSelection(bool shiftLeft)
    {
        if (shiftLeft && currentSelector == 0)
            return;
        if (!shiftLeft && currentSelector == AllSprites.Length - 1)
            return;
        m.PlaySFX(5);

        currentSelector += shiftLeft ? -1 : 1;

        bool isLeftActive = currentSelector > 0;
        bool isRightActive = currentSelector < AllSprites.Length - 1;

        chosenChar = currentSelector; //char num is 0 -> 5, currentSelector is 0 -> 6;
        LeftFrame_B.interactable = isLeftActive;
        LeftArrow.SetActive( isLeftActive );
        RightFrame_B.interactable = isRightActive;
        RightArrow.SetActive(isRightActive);
        //Reassign sprite images.
        LeftFrame_I.sprite = AllSprites[ isLeftActive ? currentSelector - 1 : currentSelector];
        MidFrame_I.sprite = AllSprites[currentSelector];
        RightFrame_I.sprite = AllSprites[ isRightActive ? currentSelector + 1: currentSelector];

        //Reassign text names.
        Name1.text = CharNames[currentSelector];
        Name2.text = CharNames[currentSelector];
        //Reassign stat frame.
        StatFrame.sprite = AllStatSprites[currentSelector];
        //Tell Master (local) and Network (online) the character selection.

        Select.interactable = isRightActive;
        
        m.AssignPlayerCharacter(chosenChar);
        n.SetCharacterInNet(); 
    }

    private void resetUI()
    {
        currentSelector = 0;
        chosenChar = currentSelector; //char num is 0 -> 5, currentSelector is 0 -> 6;
        LeftFrame_B.interactable = (currentSelector != 0);
        LeftArrow.SetActive(currentSelector != 0);
        RightFrame_B.interactable = (currentSelector != AllSprites.Length - 1);
        RightArrow.SetActive(currentSelector != AllSprites.Length - 1);
        //Reassign sprite images.
        LeftFrame_I.sprite = AllSprites[currentSelector == 0 ? currentSelector : currentSelector - 1];
        MidFrame_I.sprite = AllSprites[currentSelector];
        RightFrame_I.sprite = AllSprites[currentSelector + 1];

        //Reassign text names.
        Name1.text = CharNames[currentSelector];
        Name2.text = CharNames[currentSelector];
        //Reassign stat frame.
        StatFrame.sprite = AllStatSprites[currentSelector];
        //Tell Master (local) and Network (online) the character selection.
        m.AssignPlayerCharacter(chosenChar);
        n.SetCharacterInNet();
    }
}
