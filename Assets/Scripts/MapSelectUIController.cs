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
    private ConnectAndJoinRandom n;
    public string[] MapNames;

    public Image LeftFrame;
    public Image MidFrame;
    public Image RightFrame;

    private int currentSelector;

    private string roomSizeText;
    public Text RoomPlayerCount;
    public Text RoomPlayerCountBG;
    public Text TotalPlayerCount;
    public Text TotalPlayerCountBG;
    private string totalPlayerCountStr;
    private string totalRoomCountStr;

    private int roomNumber;
    private int maxRooms;

    // Use this for initialization
    void Start () {
        m = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
        n = GameObject.FindGameObjectWithTag("Networking").GetComponent<ConnectAndJoinRandom>();

        Reset();

        roomNumber = 0;
        maxRooms = m.TotalUniqueArenas;
        roomSizeText = " / 6\nMax Players";
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
        if (currentSelector == maxRooms)
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

    public void SetTotalOnline(int totalOnline)
    {
        totalPlayerCountStr = "Players Online\n" + (totalOnline);
        TotalPlayerCount.text = totalPlayerCountStr;
        TotalPlayerCountBG.text = totalPlayerCountStr;
    }

    private void setRoomsString()
    {
        if(n)
    }


}
