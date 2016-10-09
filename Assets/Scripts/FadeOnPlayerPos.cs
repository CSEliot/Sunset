using UnityEngine;
using System.Collections;

public class FadeOnPlayerPos : MonoBehaviour {

    private _2dxFX_Offset fadeObj;
    private NetworkManager N;


    public float MaxDistance;

    private bool gotPlayerRef;

    private Transform pTransform;
    //private Vector3 pLocation; //for local tracking location

    private Vector3 myLocation;

	// Use this for initialization
	void Start () {

        fadeObj = GetComponent<_2dxFX_Offset>();
        N = GameObject.FindGameObjectWithTag("Networking").GetComponent<NetworkManager>();
        gotPlayerRef = false;
        pTransform = null;
        myLocation = new Vector3(transform.position.x, transform.position.y, transform.position.z);
	}
	
	// Update is called once per frame
	void Update () {
	
        if(!gotPlayerRef && N.GameStarted)
        {
            pTransform = GameObject.FindGameObjectWithTag("PlayerSelf").GetComponent<Transform>();
            if (pTransform != null)
            {
                gotPlayerRef = true;
            }
        }

        if (gotPlayerRef)
        {
            fadeObj._Alpha = (MaxDistance * MaxDistance) / (myLocation - pTransform.position).sqrMagnitude; 
        }

	}
}
