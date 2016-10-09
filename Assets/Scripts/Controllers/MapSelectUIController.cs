using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MapSelectUIController : MonoBehaviour {

    public CallRoom RoomNameHelper;

    public Text Name1;
    public Text Name2;

    public Sprite[] AllSprites;
    public Sprite[] AllHDSprites;

    private Master M;
    private NetworkManager N;
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
    public GameObject JoinMapButton;

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
    private int maxStages;
	private int latestRoomTotal;
    private int roomsAvailable;
    private bool isServerWoke; //Woke Status = Server can tell us total connected > 0.

    private float timeSinceLastUpdate;

    public GameObject LoadingUI;
    public float LoadingAnimSec;

    public GameObject FullRoomWarning;
    

    public void Awake()
    {
        M = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
        N = GameObject.FindGameObjectWithTag("Networking").GetComponent<NetworkManager>();
    }

    // Use this for initialization
    void Start () {
        Reset();
        maxStages = M.TotalUniqueStages;
        roomSizeText = " / 6\nMax Players";
    }

    void OnEnable()
    {
        isServerWoke = false;
        SetTotalOnline(N.TotalOnline);
        setRoomsUI();
        StartCoroutine(PlayLoadingUI());
    }

    // Update is called once per frame
    void Update () {

        if(!isServerWoke && N.TotalOnline > 0) {
            M.ToggleConnectLoadScreen(false);
            isServerWoke = true;
        }

         if(Time.time - timeSinceLastUpdate > N.ServerUpdateLength)
        {
            timeSinceLastUpdate = Time.time;
            SetTotalOnline(N.TotalOnline);
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
		M.PlaySFX(5);
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

        MidTopFrames[0].sprite = AllHDSprites[currentHorizSelector];
        MidTopFrames[1].sprite = AllHDSprites[currentHorizSelector];
        MidFrame.sprite = AllHDSprites[currentHorizSelector];
        MidBottomFrames[0].sprite = AllHDSprites[currentHorizSelector];
        MidBottomFrames[1].sprite = AllHDSprites[currentHorizSelector];

        RightFrame.interactable = true;
        RightArrow.interactable = true;
        RightFrame.image.sprite = AllSprites[currentHorizSelector + 1];
        Name1.text = MapNames[currentHorizSelector];
        Name2.text = MapNames[currentHorizSelector];
        RoomNameHelper.RoomName = MapNames[currentHorizSelector];
        RoomNameHelper.AssignNewRoom();
    }

	public void ShiftSelectionUp(){

		if (currentVertSelector == N.Rooms [currentHorizSelector].Count - 1 )
			return;
        M.PlaySFX(7);
		currentVertSelector++;
        setRoomsUI();
	}

	public void ShiftSelectionDown() {
		if (currentVertSelector == 0)
			return;
        M.PlaySFX(8);
        currentVertSelector--;
        setRoomsUI();
    }

    public void ShiftSelectionRight()
    {
        if (currentHorizSelector == maxStages-1)
            return;
		currentHorizSelector++;
        M.PlaySFX(5);   
		currentVertSelector = 0;
        setRoomsUI();
        LeftFrame.interactable = true;
        LeftArrow.interactable = true;
        LeftFrame.image.sprite = AllSprites[currentHorizSelector - 1];

        MidTopFrames[0].sprite = AllHDSprites[currentHorizSelector];
        MidTopFrames[1].sprite = AllHDSprites[currentHorizSelector];
        MidFrame.sprite = AllHDSprites[currentHorizSelector];
        MidBottomFrames[0].sprite = AllHDSprites[currentHorizSelector];
        MidBottomFrames[1].sprite = AllHDSprites[currentHorizSelector];

        if (currentHorizSelector == maxStages-1) {
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

        roomsAvailable = N.Rooms [currentHorizSelector].Count;
        Join.interactable = roomsAvailable != 0;
        JoinMapButton.SetActive(roomsAvailable != 0);
		MidTopFrames [0].enabled = roomsAvailable > 1 && currentVertSelector < N.Rooms[currentHorizSelector].Count - 1;
		MidTopFrames [1].enabled = roomsAvailable > 2 && currentVertSelector < N.Rooms[currentHorizSelector].Count - 2;
		MidBottomFrames [0].enabled = currentVertSelector > 0;
		MidBottomFrames [1].enabled = currentVertSelector > 1;
        UpArrow.interactable = roomsAvailable > 1 && currentVertSelector != N.Rooms[currentHorizSelector].Count - 1; ;
		DownArrow.interactable = currentVertSelector > 0;

		if (roomsAvailable != 0) {
            RoomPlayerCount.text = N.Rooms[currentHorizSelector][currentVertSelector].size + "/6 Players\n" +
                N.Rooms[currentHorizSelector][currentVertSelector].name;//roomSizeText;

            RoomPlayerCountBG.text = N.Rooms[currentHorizSelector][currentVertSelector].size + "/6 Players\n" +
                N.Rooms[currentHorizSelector][currentVertSelector].name;//roomSizeText;
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

    public int getTargetStage()
    {
        return currentHorizSelector;
    }
    
    private IEnumerator PlayLoadingUI()
    {
        LoadingUI.SetActive(true);
        yield return new WaitForSeconds(LoadingAnimSec);
        LoadingUI.SetActive(false);
    }
}
