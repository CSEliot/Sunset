using UnityEngine;
using System.Collections.Generic;

public class NetID : MonoBehaviour {

    public static Dictionary<int, int> RealIDs;


	// Use this for initialization
	void Start () {
        RealIDs = new Dictionary<int, int>();
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public static int Convert(int netID)
    {
        if (RealIDs.ContainsKey(netID)) {
            return RealIDs[netID];
        }

        for(int x = 0; x < PhotonNetwork.room.playerCount; x++) {
            if (!RealIDs.ContainsValue(x) && !RealIDs.ContainsKey(netID)) {
                RealIDs.Add(netID, x);
            }
        }
        return RealIDs[netID];
    }
    
}
