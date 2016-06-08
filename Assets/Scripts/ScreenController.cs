using UnityEngine;
using System.Collections;

public class ScreenController : MonoBehaviour
{

	private Camera cam;
	private LineRenderer line;
	public float LineZ; 
	private float tempZ;

    private float mouseX;
    private float mouseY;

    private float screenLength;
    private float rgnLength;
    private float screenHeight;

    private float leftRgnScaler; //Modifies how tall control are should be.
    private float leftRgnHeight;
    private float rightRgnScaler; // Same but on attack side.
    private float rightRgnHeight;
    private Vector2 rightRgnCenter;
    private Vector2 leftRgnCenter;
    private float leftRgnCtrHghtScaler;

    private Vector2 LeftScnPos; // Pos @ 0, 0 means no input
    private Vector2 RightScnPos; // Pos @ 0, 0 means no input

    private Touch[] allInputs;
    private int totalInputs;
    private int inputNum;
    private Touch tempTouch;

    private bool wasLeftActive;
    private bool wasRightActive;
    private bool isLeftActive;
    private bool isRightActive;
    private bool isLeftToggledOff;
    private bool isRightToggledOff;
    private bool isLeftToggledOn;
    private bool isRightToggledOn;

    private MobileController mCtrl;

    private float angle;
    private float distThresh; // Between 1 and 0;
    private float distLimit;
    private float distMax;

    private float tempSpeed;
    private float tempDist;

    private float bottomRightLimit;
    private float topRightLimit;
    private float topLeftLimit;
    private float bottomLeftLimit;

	private Vector2[] debugDisplayList;
	private float attackLineLng;


    // Use this for initialization
    void Start()
    {

		cam = GameObject.FindGameObjectWithTag ("MainCamera").GetComponent<Camera> ();
		line = GetComponent<LineRenderer> ();

        mouseX = 0;
        mouseY = 0;
        screenLength = Screen.width;
        rgnLength = screenLength / 2;
        screenHeight = Screen.height;

        leftRgnScaler = 0.5f;
        rightRgnScaler = 0.5f;
        leftRgnHeight = screenHeight * leftRgnScaler;
        rightRgnHeight = screenHeight * rightRgnScaler;
        rightRgnCenter = new Vector2(screenLength * 0.75f, rightRgnHeight / 2f);
        leftRgnCtrHghtScaler = 0.40f;
        leftRgnCenter = new Vector2(screenLength * 0.25f, (leftRgnHeight * leftRgnCtrHghtScaler));

        LeftScnPos = Vector2.zero;
        RightScnPos = Vector2.zero;

        allInputs = Input.touches;
        totalInputs = Input.touchCount;
        inputNum = 0;

        wasLeftActive = false;
        wasRightActive = true;
        isLeftActive = false;
        isRightActive = false;
        isLeftToggledOff = false;
        isLeftToggledOn = false;
        isRightToggledOff = false;
        isRightToggledOn = false;

        mCtrl = GameObject.FindGameObjectWithTag("MobileController").GetComponent<MobileController>();

        bottomRightLimit = -45f;
        topRightLimit = 45f;
        topLeftLimit = 135f;
        bottomLeftLimit = -135f;

        distThresh = 0.6f;
        distMax =  leftRgnHeight - leftRgnCenter.y;
        distLimit = distMax * distThresh;


		attackLineLng = Mathf.Sqrt(Mathf.Pow(screenLength/4, 2) + Mathf.Pow(rightRgnHeight, 2))/2;
		debugDisplayList = new Vector2 [20];
		debugDisplayList [0] = new Vector2 (0, leftRgnHeight);
		debugDisplayList [1] = new Vector2 (rgnLength, leftRgnHeight);
		debugDisplayList [2] = new Vector2 (rgnLength, rightRgnHeight);
		debugDisplayList [3] = new Vector2 (rgnLength * 2f, rightRgnHeight);
		debugDisplayList [4] = new Vector2 (0, 0);
		debugDisplayList [5] = new Vector2 (screenLength, 0);
		debugDisplayList [6] = new Vector2 (Mathf.Cos ( topLeftLimit * (Mathf.PI / 180f) ) * attackLineLng + rightRgnCenter.x, 
		                                    Mathf.Sin ( topLeftLimit * (Mathf. PI / 180f) ) * attackLineLng + rightRgnCenter.y);
		debugDisplayList [7] = new Vector2 (Mathf.Cos ( topRightLimit * (Mathf.PI / 180f) ) * attackLineLng + rightRgnCenter.x, 
		                                    Mathf.Sin ( topRightLimit * (Mathf.PI / 180f) ) * attackLineLng + rightRgnCenter.y);
		debugDisplayList [8] = new Vector2 (Mathf.Cos ( bottomLeftLimit * (Mathf.PI / 180f) ) * attackLineLng + rightRgnCenter.x, 
		                                    Mathf.Sin ( bottomLeftLimit * (Mathf.PI / 180f) ) * attackLineLng + rightRgnCenter.y);
		debugDisplayList [9] = new Vector2 (Mathf.Cos (( bottomRightLimit * (Mathf.PI / 180f) )) * attackLineLng + rightRgnCenter.x, 
		                                    Mathf.Sin (( bottomRightLimit * (Mathf.PI / 180f) )) * attackLineLng + rightRgnCenter.y);
		debugDisplayList [10] = new Vector2 (rightRgnCenter.x, rightRgnCenter.y); 
		debugDisplayList [11] = new Vector2 (leftRgnCenter.x, leftRgnHeight);
		debugDisplayList [12] = new Vector2 (leftRgnCenter.x, 0);
		debugDisplayList [13] = new Vector2 (0, leftRgnCenter.y);
		debugDisplayList [14] = new Vector2 (rgnLength, leftRgnCenter.y);
		debugDisplayList [15] = new Vector2 (leftRgnCenter.x, leftRgnCenter.y);
		debugDisplayList [16] = new Vector2 (-distLimit + leftRgnCenter.x, distLimit + leftRgnCenter.y);
		debugDisplayList [17] = new Vector2 (distLimit + leftRgnCenter.x, distLimit + leftRgnCenter.y);
		debugDisplayList [18] = new Vector2 (-distLimit + leftRgnCenter.x, -distLimit + leftRgnCenter.y);
		debugDisplayList [19] = new Vector2 (distLimit + leftRgnCenter.x, -distLimit + leftRgnCenter.y);

		for (int x = 0; x < 20; x++) {
			line.SetPosition(x, cam.ScreenToWorldPoint(new Vector3(debugDisplayList[x].x, debugDisplayList[x].y, 0f)));
		}
    }

    // Update is called once per frame
    void Update()
    {
		if (tempZ != LineZ) {
			for (int x = 0; x < 20; x++) {
				line.SetPosition(x, cam.ScreenToWorldPoint(new Vector3(debugDisplayList[x].x, debugDisplayList[x].y, LineZ)));
			}
			tempZ = LineZ;
		}
		
        if (Application.isEditor)
            assignScreenActivityPCTEST();
        else
            assignScreenActivity();

        registerAttacks();
        registerMovement();

        
    }

    private void registerMovement()
    {
        if (!isLeftActive)
            return;

        //get distance first.
        tempDist = Vector2.Distance(leftRgnCenter, LeftScnPos);
        tempSpeed = tempDist > distLimit ? 1.0f : tempDist / distMax; //OR DISTMAX?!


        //now get location to apply distance.
        if (LeftScnPos.x - leftRgnCenter.x > 0)
        {
            mCtrl.SetAxisDown("MoveHorizontal", tempSpeed);
        }
        else
        {
            mCtrl.SetAxisDown("MoveHorizontal", -tempSpeed);
        }

        if (LeftScnPos.y - leftRgnCenter.y > 0)
        {
            mCtrl.SetButtonDown("Jump");
        }
        else
        {
            mCtrl.SetButtonUp("Jump");
            mCtrl.SetButtonDown("DownJump");
        }
    }

    private void registerAttacks()
    {
 
        if (!isRightActive)
            return;

        angle = Mathf.Atan2(RightScnPos.y - rightRgnCenter.y, RightScnPos.x - rightRgnCenter.x);
        angle = angle * (180f / Mathf.PI);

        if (angle > bottomRightLimit && angle < topRightLimit)
        {
            mCtrl.SetButtonDown("Right");
        }
        else if (angle < topLeftLimit && angle > topRightLimit)
        {
            mCtrl.SetButtonDown("Up");
        }
        else if (angle < bottomLeftLimit || angle > topLeftLimit)
        {

            mCtrl.SetButtonDown("Left");
        }
        else if (angle > bottomLeftLimit && angle < bottomRightLimit)
        {
            mCtrl.SetButtonDown("Down");
        }
    }

    private void assignScreenActivity()
    {
        wasRightActive = IsRightActive;
        wasLeftActive = IsLeftActive;
        isLeftActive = false;
        isRightActive = false;

        allInputs = Input.touches;
        totalInputs = Input.touchCount;

        Debug.Log("total inputs is: " + totalInputs);

        //Update left region and right region activity.
        // Each region has it's own unique location.
        // Each region has 2 variables: If Touch down and 
        for (inputNum = 0; inputNum < totalInputs; inputNum++)
        {
            tempTouch = allInputs[inputNum];

            if (tempTouch.position.x < rgnLength)
            {
                if (tempTouch.position.y < leftRgnHeight)
                {
                    LeftScnPos = tempTouch.position;
                    isLeftActive = true;
                }
            }
            else
            {
                if (tempTouch.position.y < rightRgnHeight)
                {
                    RightScnPos = tempTouch.position;
                    isRightActive = true;
                }
            }
        }
        //Set boolean triggers
        isLeftToggledOn = !wasLeftActive && isLeftActive ? true : false;
        isLeftToggledOff = wasLeftActive && !isLeftActive ? true : false;
        isRightToggledOn = !wasRightActive && isRightActive ? true : false;
        isRightToggledOff = wasRightActive && !isRightActive ? true : false;
    }

    private void assignScreenActivityPCTEST()
    {
        wasRightActive = IsRightActive;
        wasLeftActive = IsLeftActive;
        isLeftActive = false;
        isRightActive = false;
        
        if (Input.mousePosition.x < rgnLength)
        {
            if (Input.mousePosition.y < leftRgnHeight)
            {
                LeftScnPos = Input.mousePosition;
                isLeftActive = true;
            }
        }
        else
        {
            if (Input.mousePosition.y < rightRgnHeight)
            {
                RightScnPos = Input.mousePosition;
                isRightActive = true;
            }
        }

        //Set boolean triggers
        isLeftToggledOn = !wasLeftActive && isLeftActive ? true : false;
        isLeftToggledOff = wasLeftActive && !isLeftActive ? true : false;
        isRightToggledOn = !wasRightActive && isRightActive ? true : false;
        isRightToggledOff = wasRightActive && !isRightActive ? true : false;
    }

    public bool IsLeftActive
    {
        get
        {
            return isLeftActive;
        }
    }

    public bool IsRightActive
    {
        get
        {
            return isRightActive;
        }
    }

    public bool IsLeftToggledOff
    {
        get
        {
            return isLeftToggledOff;
        }
    }

    public bool IsRightToggledOff
    {
        get
        {
            return isRightToggledOff;
        }
    }

    public bool IsLeftToggledOn
    {
        get
        {
            return isLeftToggledOn;
        }
    }

    public bool IsRightToggledOn
    {
        get
        {
            return isRightToggledOn;
        }
    }
}
