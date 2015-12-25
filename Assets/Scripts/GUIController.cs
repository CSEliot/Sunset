using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GUIController : MonoBehaviour {


    public Text Name1;
    public Text Name2;

    public Sprite[] AllSprites;
    private Master m;
    public string[] CharNames;

    public Image LeftFrame;
    public Image MidFrame;
    public Image RightFrame;

    private int currentSelector;

	// Use this for initialization
	void Start () {
        currentSelector = 1;
        m = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown("Left"))
        {
            ShiftSelectionLeft();
        }
        if (Input.GetButtonDown("Right"))
        {
            ShiftSelectionRight();
        }
	}

    private void ShiftSelectionLeft()
    {
        if (currentSelector == 1)
            return;
        currentSelector += -1;
        LeftFrame.sprite = AllSprites[currentSelector - 1];
        MidFrame.sprite = AllSprites[currentSelector];
        RightFrame.sprite = AllSprites[currentSelector + 1];
        Name1.text = CharNames[currentSelector - 1];
        Name2.text = CharNames[currentSelector - 1];
        UpdateStats();
        m.AssignClientCharacter(currentSelector - 1);   
    }

    private void ShiftSelectionRight()
    {
        if (currentSelector == 7)
            return;
        currentSelector += 1;
        LeftFrame.sprite  = AllSprites[currentSelector - 1];
        MidFrame.sprite   = AllSprites[currentSelector];
        RightFrame.sprite = AllSprites[currentSelector + 1];
        Name1.text = CharNames[currentSelector - 1];
        Name2.text = CharNames[currentSelector - 1];
        UpdateStats();
        m.AssignClientCharacter(currentSelector-1);
    }

    private void UpdateStats()
    {
        
    }
}
