using UnityEngine;
using System.Collections;

public class ScreenController : MonoBehaviour
{

    private float mouseX;
    private float mouseY;

    private float screenLength;
    private float rgnLength;
    private float screenHeight;

    public float LeftRgnScaler; //Modifies how tall control are should be.
    private float leftRgnHeight;
    public float RightRgnScaler; // Same but on attack side.
    private float rightRgnHeight;
    private Vector2 rightRgnCenter;
    private Vector2 leftRgnCenter;

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
    [Tooltip("Must be between 1.0 and 0.0 !")]
    [Range(1.0f, 0.0f)]
    public float distThresh; // Between 1 and 0;
    private float distLimit;
    private float distMax;

    private float tempSpeed;
    private float tempDist;

    private float bottomRightLimit;
    private float topRightLimit;
    private float topLeftLimit;
    private float bottomLeftLimit;

    // Use this for initialization
    void Start()
    {
        mouseX = 0;
        mouseY = 0;
        screenLength = Screen.width;
        rgnLength = screenLength / 2;
        screenHeight = Screen.height;

        leftRgnHeight = screenHeight * LeftRgnScaler;
        rightRgnHeight = screenHeight * RightRgnScaler;
        rightRgnCenter = new Vector2(screenLength * 0.75f, rightRgnHeight / 2f);
        leftRgnCenter = new Vector2(screenLength * 0.25f, leftRgnHeight / 2f);

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

        distMax =  leftRgnHeight - leftRgnCenter.y;
        distLimit = distMax * distThresh;

        //CHANGES TO DO: Jump 75% of height, left and right bigger gaps, all maps, and fix infinite jump
    }

    // Update is called once per frame
    void Update()
    {
        if (Application.isEditor)
            assignScreenActivityPCTEST();
        else
            assignScreenActivity();

        registerAttacks();

        if (!isLeftActive)
            return;

        //get distance first.
        tempDist = Vector2.Distance(leftRgnCenter, LeftScnPos);
        tempSpeed = tempDist > distLimit ? 1.0f : tempDist / distMax; //OR DISTMAX?!
        

        //now get location to apply distance.
        if(LeftScnPos.x - leftRgnCenter.x > 0)
        {
            mCtrl.SetAxisDown("MoveHorizontal", tempSpeed);
        }
        else
        {
            mCtrl.SetAxisDown("MoveHorizontal", -tempSpeed);
        }

        if(LeftScnPos.y - leftRgnCenter.y > 0)
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
