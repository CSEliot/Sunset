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

    public Button LeftFrame;
    public Button LeftArrow;
    public Image MidFrame;
	public Image[] MidTopFrames;
	public Image[] MidBottomFrames;
	public Button RightFrame;
    public Button RightArrow;
    public Button DownArrow;
	public Button UpArrow;
    public Button Join;

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
    private int maxArenas;
	private int latestRoomTotal;
    private int roomsAvailable;

    private float timeSinceLastUpdate;

    public GameObject LoadingUI;
    public float LoadingAnimSec;

    public GameObject FullRoomWarning;


    public void Awake()
    {
        m = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
        n = GameObject.FindGameObjectWithTag("Networking").GetComponent<ConnectAndJoinRandom>();
    }

    // Use this for initialization
    void Start () {
        Reset();
        maxArenas = m.TotalUniqueArenas;
        roomSizeText = " / 6\nMax Players";
    }

    void OnEnable()
    {
        Reset();
    }

    // Update is called once per frame
    void Update () {
         if(Time.time - timeSinceLastUpdate > n.ServerUpdateLength)
        {
            timeSinceLastUpdate = Time.time;
            SetTotalOnline(n.TotalOnline);
            setRoomsUI();
            StartCoroutine(PlayLoadingUI());   
        }
	}

    public void Reset()
    {
        currentHorizSelector = 0;
		currentVertSelector = 0;
		LeftFrame.interactable = false;
        LeftArrow.interactable = false;
        RightFrame.interactable = true;
        RightArrow.interactable = true;
        MidFrame.sprite = AllHDSprites[currentHorizSelector];
        RightFrame.image.sprite = AllSprites[currentHorizSelector + 1];
        Name1.text = MapNames[currentHorizSelector];
        Name2.text = MapNames[currentHorizSelector];
        RoomNameHelper.RoomName = MapNames[currentHorizSelector];
        RoomNameHelper.AssignNewRoom();
    }


    public void ShiftSelectionLeft()
    {
        if (currentHorizSelector == 0)
            return;
		currentHorizSelector--;
		m.PlaySFX(5);
		currentVertSelector = 0;
        setRoomsUI();
        if (currentHorizSelector == 0) {
			LeftFrame.interactable = false;
            LeftArrow.interactable = false;
        } else {
			LeftFrame.interactable = true;
            LeftArrow.interactable = true;
            LeftFrame.image.sprite = AllSprites[currentHorizSelector - 1];
		}
		MidFrame.sprite = AllHDSprites[currentHorizSelector];
        RightFrame.interactable = true;
        RightArrow.interactable = true;
        RightFrame.image.sprite = AllSprites[currentHorizSelector + 1];
        Name1.text = MapNames[currentHorizSelector];
        Name2.text = MapNames[currentHorizSelector];
        RoomNameHelper.RoomName = MapNames[currentHorizSelector];
        RoomNameHelper.AssignNewRoom();
    }

	public void ShiftSelectionUp(){

		if (currentVertSelector == n.Rooms [currentHorizSelector - 1].Count + 1)
			return;
		currentVertSelector++;
	}

	public void ShiftSelectionDown() {
		if (currentVertSelector == 0)
			return;
		currentVertSelector--;
	}

    public void ShiftSelectionRight()
    {
        if (currentHorizSelector == maxArenas-1)
            return;
		currentHorizSelector++;
        m.PlaySFX(5);   
		currentVertSelector = 0;
        setRoomsUI();
        LeftFrame.interactable = true;
        LeftArrow.interactable = true;
        LeftFrame.image.sprite = AllSprites[currentHorizSelector - 1];
        MidFrame.sprite   = AllHDSprites[currentHorizSelector];
		if (currentHorizSelector == maxArenas-1) {
			RightFrame.interactable = false;
            RightArrow.interactable = false;
        } else {
			RightFrame.interactable = true;
            RightArrow.interactable = true;
            RightFrame.image.sprite = AllSprites[currentHorizSelector + 1];
		}
        Name1.text = MapNames[currentHorizSelector];
        Name2.text = MapNames[currentHorizSelector];
        RoomNameHelper.RoomName = MapNames[currentHorizSelector];
        RoomNameHelper.AssignNewRoom();
    }

    public void SetTotalOnline(int totalOnline)
    {
        totalPlayerCountStr = "Players Online: " + (totalOnline);
        TotalPlayerCount.text = totalPlayerCountStr;
        TotalPlayerCountBG.text = totalPlayerCountStr;
    }

	/// <summary>
	/// Sets the rooms UI. Called on vertical or horizontal change.
	/// </summary>
    private void setRoomsUI()
    {

        roomsAvailable = n.Rooms [currentHorizSelector].Count;
        Join.interactable = roomsAvailable != 0;
		MidTopFrames [0].enabled = roomsAvailable > 1;
		MidTopFrames [1].enabled = roomsAvailable > 2;
		MidBottomFrames [0].enabled = currentVertSelector > 0;
		MidBottomFrames [1].enabled = currentVertSelector > 1;
        UpArrow.interactable = roomsAvailable > 1;
		DownArrow.interactable = currentVertSelector > 0;

		if (roomsAvailable != 0) {
			RoomPlayerCount.text = n.Rooms [currentHorizSelector] [currentVertSelector].size + roomSizeText;
			RoomPlayerCountBG.text = n.Rooms [currentHorizSelector] [currentVertSelector].size + roomSizeText;
        }
        else
        {
            RoomPlayerCount.text = "Create a game!";
            RoomPlayerCountBG.text = "Create a game!";
        }
    }

    public int getTargetRoom()
    {
        return currentVertSelector;
    }

    public int getTargetArena()
    {
        return currentHorizSelector;
    }
    
    private IEnumerator PlayLoadingUI()
    {
        LoadingUI.SetActive(true);
        Debug.Log("tUE");
        yield return new WaitForSeconds(LoadingAnimSec);
        Debug.Log("WED");
        LoadingUI.SetActive(false);
    }
}
