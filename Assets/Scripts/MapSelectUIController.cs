using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MapSelectUIController : MonoBehaviour {

    public CallRoom RoomNameHelper;

    public Text Name1;
    public Text Name2;

    public Sprite[] AllSprites;
    public Sprite[] AllHDSprites;

    private Master m;
    public string[] MapNames;

    public Image LeftFrame;
    public Image MidFrame;
    public Image RightFrame;

    private int currentSelector;

	// Use this for initialization
	void Start () {
        m = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
        Reset();
    }

    public void Reset()
    {
        currentSelector = 1;
        LeftFrame.sprite = AllSprites[currentSelector - 1];
        MidFrame.sprite = AllHDSprites[currentSelector];
        RightFrame.sprite = AllSprites[currentSelector + 1];
        Name1.text = MapNames[currentSelector];
        Name2.text = MapNames[currentSelector];
        RoomNameHelper.RoomName = MapNames[currentSelector];
        RoomNameHelper.AssignNewRoom();
    }

    void OnEnable()
    {
    }

    // Update is called once per frame
    void Update () {
	}

    public void ShiftSelectionLeft()
    {
        if (currentSelector == 1)
            return;
        m.PlaySFX(5);
        currentSelector += -1;
        LeftFrame.sprite = AllSprites[currentSelector - 1];
        MidFrame.sprite = AllHDSprites[currentSelector];
        RightFrame.sprite = AllSprites[currentSelector + 1];
        Name1.text = MapNames[currentSelector];
        Name2.text = MapNames[currentSelector];
        RoomNameHelper.RoomName = MapNames[currentSelector];
        RoomNameHelper.AssignNewRoom();
    }

    public void ShiftSelectionRight()
    {
        if (currentSelector == 3)
            return;
        m.PlaySFX(5);   
        currentSelector += 1;
        LeftFrame.sprite  = AllSprites[currentSelector - 1];
        MidFrame.sprite   = AllHDSprites[currentSelector];
        RightFrame.sprite = AllSprites[currentSelector + 1];
        Name1.text = MapNames[currentSelector];
        Name2.text = MapNames[currentSelector];
        RoomNameHelper.RoomName = MapNames[currentSelector];
        RoomNameHelper.AssignNewRoom();
    }
}
