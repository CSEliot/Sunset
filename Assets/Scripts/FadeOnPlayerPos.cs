using UnityEngine;
using System.Collections;

public class FadeOnPlayerPos : MonoBehaviour {

    private _2dxFX_Offset fadeObj;
    public float MaxDistance;

    private bool gotPlayerRef;

    private GameObject[] pObjs;
    //private Vector3 pLocation; //for local tracking location

    private Vector3 myLocation;
    private float tempAlpha;

    private int totalPlayers;

    private delegate void fade();

	// Use this for initialization
	void Start () {

        fadeObj = GetComponent<_2dxFX_Offset>();
        gotPlayerRef = false;
        myLocation = new Vector3(transform.position.x, transform.position.y, transform.position.z);
	}
	
	// Update is called once per frame
	void Update () {
	
        if(!gotPlayerRef && GameObject.FindGameObjectWithTag("PlayerSelf") != null)
        {
            pObjs = GameObject.FindGameObjectsWithTag("PlayerSelf");
            totalPlayers = pObjs.Length;
            gotPlayerRef = true;
        }

        if (gotPlayerRef)
        {
            tempAlpha = 0;
            fadeObj._Alpha = 0f;
            for(int i = 0; i < totalPlayers; i++) {
                tempAlpha = (MaxDistance * MaxDistance) / (myLocation - pObjs[i].transform.position).sqrMagnitude;
                tempAlpha = Mathf.Clamp(tempAlpha, 0f, 1f);
                if (fadeObj._Alpha < tempAlpha)
                    fadeObj._Alpha = tempAlpha;
            }
        }

	}
}
