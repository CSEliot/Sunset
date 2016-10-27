using UnityEngine;
using System.Collections.Generic;

public class NetID : MonoBehaviour {

    /// <summary>
    /// NetID -> RoomID (slotNum)
    /// </summary>
    public static Dictionary<int, int> FromNetToSlot;
    public static Dictionary<int, int> FromSlotToNet;

	// Use this for initialization
	void Start () {
        FromNetToSlot = new Dictionary<int, int>();
        FromSlotToNet = new Dictionary<int, int>();
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    /// <summary>
    /// From netID to roomID for properly handling slots.
    /// </summary>
    /// <param name="netID"></param>
    /// <returns>Player's Room ID</returns>
    public static int ConvertToSlot(int netID)
    {
        if (FromNetToSlot.ContainsKey(netID)) {
            //CBUG.Do("ID KEY FOUND! ID: " + netID);
            return FromNetToSlot[netID];
        }

        CBUG.Do("Adding new player ID: " + netID);
        for(int x = 0; x < PhotonNetwork.room.playerCount; x++) {
            if (!FromNetToSlot.ContainsValue(x) && !FromNetToSlot.ContainsKey(netID)) {
                FromNetToSlot.Add(netID, x);
                FromSlotToNet.Add(x, netID);
            }
        }
        return FromNetToSlot[netID];
    }
    
    public static void Remove(int netID)
    {
        FromSlotToNet.Clear();
        int slotNumToRemove = FromNetToSlot[netID];
        FromNetToSlot.Remove(netID);
        foreach(int id in FromNetToSlot.Keys) {
            if(FromNetToSlot[id] > slotNumToRemove ) {
                FromNetToSlot[id] = FromNetToSlot[id] - 1;
                FromNetToSlot.Add(FromNetToSlot[id], id);
            }
        }
    }
}
