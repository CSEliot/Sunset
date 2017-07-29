using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tracks player Server IDs to their Player Number within a room.
/// </summary>
public class NetIDs : MonoBehaviour {

    /// <summary>
    /// An index is a room slot.
    /// </summary>
    private static List<int> roomIDsList;
    
	// Use this for initialization
	void Start () {
        if(roomIDsList == null)
            roomIDsList = new List<int>();
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    /// <summary>
    /// Photon's room number is not guaranteed to be consistent so we track
    /// a player's place in the room to their server ID Number.
    /// 
    /// Returns player's Room Number and creates one if it doesn't exist.
    /// </summary>
    /// <param name="NetID"></param>
    /// <returns>Player's Room ID</returns>
    public static int PlayerNumber(int NetID)
    {
        if(roomIDsList == null)
            roomIDsList = new List<int>();

        
        for (int x = 0; x < roomIDsList.Count; x++)
        {
            if (NetID == roomIDsList[x])
                return x;
        }
        //if (roomIDsList.Contains(NetID)) {
        //    //CBUG.Do("ID KEY FOUND! ID: " + netID);
        //    return roomIDsList[NetID];
        //}

        CBUG.Do("Adding new player. Server ID: " + NetID + ". Is Player " + (roomIDsList.Count));
        roomIDsList.Add(NetID);
        return roomIDsList.Count - 1;   
    }

    public static int GetNetID(int PlayerNumber)
    {
        if (roomIDsList == null)
            roomIDsList = new List<int>();

        return roomIDsList[PlayerNumber];
    }

    
    public static void Remove(int netID)
    {
        if (roomIDsList == null)
            roomIDsList = new List<int>();

        bool removedSuccessfully = roomIDsList.Remove(netID);
        if (!removedSuccessfully)
            CBUG.SrsError("Attempted to remove a NetID that doesn't exist!");
    }

    public static void Clear()
    {
        if (roomIDsList == null)
            roomIDsList = new List<int>();

        //If roomIDsList exists, Keyword: first time null
        //Sometimes a function will reference an object
        //That hasn't been loaded in yet.
        //And doing a null check is fine because
        //on a first time load.
        if (roomIDsList != null)
            roomIDsList.Clear();
    }
}
