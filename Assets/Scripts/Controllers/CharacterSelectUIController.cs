using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CharacterSelectUIController : MonoBehaviour {

    public Text Name1;
    public Text Name2;

    public Text Desc1;
    public Text Desc2;
    private string[] charDesc;

    public Sprite[] AllSprites;

    public Image StatFrame;
    public Sprite[] AllStatSprites;

    private NetworkManager N;
    private Master M;
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
        N = GameObject.FindGameObjectWithTag("Networking").GetComponent<NetworkManager>();
        M = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
        setDesc();
        currentSelector = PlayerPrefs.GetInt("currentSelector", 0);
        assign();
	}
    
    void OnEnable ()
    {
        assign();
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
        M.PlaySFX(5);

        currentSelector += shiftLeft ? -1 : 1;

        assign();  
    }

    private void assign()
    {
        bool isLeftActive = currentSelector > 0;
        bool isRightActive = currentSelector < AllSprites.Length - 1;

        chosenChar = currentSelector; //char num is 0 -> 5, currentSelector is 0 -> 6;
        LeftFrame_B.interactable = isLeftActive;
        LeftArrow.SetActive(isLeftActive);
        RightFrame_B.interactable = isRightActive;
        RightArrow.SetActive(isRightActive);
        //Reassign sprite images.
        LeftFrame_I.sprite = AllSprites[isLeftActive ? currentSelector - 1 : currentSelector];
        MidFrame_I.sprite = AllSprites[currentSelector];
        RightFrame_I.sprite = AllSprites[isRightActive ? currentSelector + 1 : currentSelector];
        //Reassign description info.
        Desc1.text = charDesc[currentSelector];
        Desc2.text = charDesc[currentSelector];

        //Reassign text names.
        Name1.text = CharNames[currentSelector];
        Name2.text = CharNames[currentSelector];
        //Reassign stat frame.
        StatFrame.sprite = AllStatSprites[currentSelector];
        //Tell Master (local) and Network (online) the character selection.

        Select.interactable = isRightActive;

        if (chosenChar >= M.CharactersUnlockedTotal)
        {
            _Audio.StopMusic();
            return;
        }

        M.AssignPlayerCharacter(chosenChar);
        N.SetCharacter();
        _Audio.Play(chosenChar + 20);
        _Audio.ChangeMusic();
        _Audio.Play(chosenChar + 11);

        if(chosenChar == 6)

        PlayerPrefs.SetInt("currentSelector", currentSelector);
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
        //Reassign description text.
        Desc1.text = charDesc[currentSelector];
        Desc2.text = charDesc[currentSelector];

        //Reassign text names.
        Name1.text = CharNames[currentSelector];
        Name2.text = CharNames[currentSelector];
        //Reassign stat frame.
        StatFrame.sprite = AllStatSprites[currentSelector];
        //Tell Master (local) and Network (online) the character selection.
        M.AssignPlayerCharacter(chosenChar);
        //n.SetCharacter();
    }

    private void setDesc()
    {
        charDesc = new string[7]{
            "A disgruntled student, Fang compensates his terrible grades with blingy fashion.  Except, he compensates his lack of fashion money... by smashing things to destress.",
            "Paramedic by day. Medicinal supplier by night. Dire contemplates what is actually \"good\" and \"evil\" in this world when his head isn't in the clouds. He also likes blackberries.",
            "After reading an old book, Lore became a wizard. The book was about architecture and the timing was coincidental. The magic is real, though, so now he fights stuff with the power of wizardry.",
            "The great and the good are separated by their willingness to sacrifice. I've given up my legs to smash them with my arms!",
            "One day, Sarchy falls asleep. Upon waking up, Sarchy becomes trapped in a familiar video game world! Stuck in 2D, Sarchy fights to be 3D once again. There's anime to watch!",
            "A cybernetic wolf created by an extremely lonely weaboo who only wanted to have a friend to talk to about the latest anime.",
            "???"
        };
    }
}
