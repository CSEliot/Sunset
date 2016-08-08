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
	public Image[] MidTopFrames;
	public Image[] MidBottomFrames;
	public Image RightFrame;
	public Image DownArrow;
	public Image UpArrow;

    private int currentHorizSelector;
	private int currentVertSelector;

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
        currentHorizSelector = 1;
		currentVertSelector = 0;
        LeftFrame.sprite = AllSprites[currentHorizSelector - 1];
        MidFrame.sprite = AllHDSprites[currentHorizSelector];
        RightFrame.sprite = AllSprites[currentHorizSelector + 1];
        Name1.text = MapNames[currentHorizSelector];
        Name2.text = MapNames[currentHorizSelector];
        RoomNameHelper.RoomName = MapNames[currentHorizSelector];
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
        if (currentHorizSelector == 1)
            return;
        m.PlaySFX(5);
        currentHorizSelector += -1;
		currentVertSelector = 0;
		setRoomsString ();
        LeftFrame.sprite = AllSprites[currentHorizSelector - 1];
		MidTopFrames[0].sprite = AllHDSprites[currentHorizSelector];
		MidTopFrames[1].sprite = AllHDSprites[currentHorizSelector];
		MidFrame.sprite = AllHDSprites[currentHorizSelector];
		MidBottomFrames[0].sprite = AllHDSprites[currentHorizSelector];
		MidBottomFrames[1].sprite = AllHDSprites[currentHorizSelector];
        RightFrame.sprite = AllSprites[currentHorizSelector + 1];
        Name1.text = MapNames[currentHorizSelector];
        Name2.text = MapNames[currentHorizSelector];
        RoomNameHelper.RoomName = MapNames[currentHorizSelector];
        RoomNameHelper.AssignNewRoom();
    }

	public void ShiftSelectionUp(){

	}

	public void ShiftSelectionDown() {
	
	}

    public void ShiftSelectionRight()
    {
        if (currentHorizSelector == maxRooms)
            return;
        m.PlaySFX(5);   
        currentHorizSelector += 1;
		currentVertSelector = 0;
		setRoomsString ();
        LeftFrame.sprite  = AllSprites[currentHorizSelector - 1];
        MidFrame.sprite   = AllHDSprites[currentHorizSelector];
        RightFrame.sprite = AllSprites[currentHorizSelector + 1];
        Name1.text = MapNames[currentHorizSelector];
        Name2.text = MapNames[currentHorizSelector];
        RoomNameHelper.RoomName = MapNames[currentHorizSelector];
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
		int tempRoomsAvailable = n.Rooms [currentHorizSelector - 1].Count;

		MidTopFrames [0].enabled = tempRoomsAvailable > 1;
		MidTopFrames [1].enabled = tempRoomsAvailable > 2;
		MidBottomFrames [0].enabled = currentVertSelector > 0;
		MidBottomFrames [1].enabled = currentVertSelector > 1;
		UpArrow.enabled = tempRoomsAvailable > 1;
		DownArrow.enabled = currentVertSelector > 0;

		if (tempRoomsAvailable != 0) {
			RoomPlayerCount.text = n.Rooms [currentHorizSelector] [currentVertSelector].size + roomSizeText;
			RoomPlayerCountBG.text = n.Rooms [currentHorizSelector] [currentVertSelector].size + roomSizeText;
		}
    }


}
