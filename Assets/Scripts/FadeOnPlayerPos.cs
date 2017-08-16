using UnityEngine;
using System.Collections;

public class FadeOnPlayerPos : MonoBehaviour
{

    private _2dxFX_Offset fadeObj;
    private float maxDistance;


    public enum followComponent
    {
        x,
        y,
        z,
        xy,
        xz,
        zy,
        xyz
    }
    /// <summary>
    /// Must be assigned manually in-editor.
    /// </summary>
    public followComponent CurrentFollowing;


    private bool gotPlayerRef;

    private GameObject[] pObjs;
    //private Vector3 pLocation; //for local tracking location

    private Vector3 myLocation;
    private float tempAlpha;

    private int totalPlayers;

    private delegate void fade();

    private bool fadeOnSingleComponent2D;
    private bool fadeOnSingleComponent1D;


    // Use this for initialization
    void Start()
    {
        fadeObj = GetComponent<_2dxFX_Offset>();
        gotPlayerRef = false;
        myLocation = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        maxDistance = 35f;

        if ((int)CurrentFollowing < 3)
        {
            fadeOnSingleComponent1D = true;
            fadeOnSingleComponent2D = false;
        }
        else if ((int)CurrentFollowing < 6)
        {
            fadeOnSingleComponent1D = false;
            fadeOnSingleComponent2D = true;
        }
        else
        {
            fadeOnSingleComponent1D = false;
            fadeOnSingleComponent2D = false;
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (!gotPlayerRef && GameObject.FindGameObjectWithTag("PlayerSelf") != null)
        {
            pObjs = GameObject.FindGameObjectsWithTag("PlayerSelf");
            totalPlayers = pObjs.Length;
            gotPlayerRef = true;
        }

        if (gotPlayerRef)
        {
            tempAlpha = 0;
            fadeObj._Alpha = 0f;
            if (fadeOnSingleComponent1D)
            {
                for (int i = 0; i < totalPlayers; i++)
                {
                    float currentDistance = Mathf.Abs(
                            myLocation[(int)CurrentFollowing] -
                            pObjs[i].transform.position[(int)CurrentFollowing]
                        );
                    tempAlpha = (maxDistance) / currentDistance;
                    tempAlpha = Mathf.Clamp(tempAlpha, 0f, 1f);
                    if (fadeObj._Alpha < tempAlpha)
                        fadeObj._Alpha = tempAlpha;
                }
            }
            else if (fadeOnSingleComponent2D)
            {
                int firstComponent = 0;
                if (CurrentFollowing == followComponent.zy)
                    firstComponent = 2;
                int secondComponent = 1;
                if (CurrentFollowing == followComponent.xz)
                    secondComponent = 2;

                for (int i = 0; i < totalPlayers; i++)
                {
                    Vector3 temp2DV = new Vector3(
                        pObjs[i].transform.position[firstComponent],
                        pObjs[i].transform.position[secondComponent],
                        0f);

                    tempAlpha = (maxDistance * maxDistance) / (myLocation - temp2DV).sqrMagnitude;
                    tempAlpha = Mathf.Clamp(tempAlpha, 0f, 1f);
                    if (fadeObj._Alpha < tempAlpha)
                        fadeObj._Alpha = tempAlpha;
                }
                for (int i = 0; i < totalPlayers; i++)
                {
                    tempAlpha = (maxDistance * maxDistance) / (myLocation - pObjs[i].transform.position).sqrMagnitude;
                    tempAlpha = Mathf.Clamp(tempAlpha, 0f, 1f);
                    if (fadeObj._Alpha < tempAlpha)
                        fadeObj._Alpha = tempAlpha;
                }
            }

        }
    }
}
