using UnityEngine;
using System.Collections;

public class SettingsManager : MonoBehaviour {

    private static bool infoUpdated;

    [Header("Default Settings")]
    public int StartLives;
    public float GameLength;
    public float RespawnTime;
    [Range(2, 6)]
    public int MinimumPlayers;
    [Range(2, 6)]
    public int MaximumPlayers;

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    #region Static Functions
    public static void GetGameInfo(ref int StartLives, ref float GameLength,
                                   ref float RespawnTime)
    {
        StartLives = GetRef().StartLives;
        GameLength = GetRef().GameLength;
        RespawnTime = GetRef().RespawnTime;
    }

    public static void SetGameInfo(int startLives = 2, float gameLength = 180f,
                                   float respawnTime = 2f, int minPlayers = 2,
                                   int maxPlayers = 6)
    {
        GetRef().setGameSettings(startLives, gameLength, respawnTime,
            minPlayers, maxPlayers);
    }

    public static SettingsManager GetRef()
    {
        return GameObject.FindGameObjectWithTag("Settings").GetComponent<SettingsManager>();
    }

    public static int _StartLives {
        get{
            return GetRef().StartLives;
        }
    }

    public static float _GameLength {
        get {
            return GetRef().GameLength;
        }
    }

    public static float _RespawnTime{
        get {
            return GetRef().RespawnTime;
        }
    }

    public static int _MinimumPlayers{
        get {
            return GetRef().MinimumPlayers;
        }
    }

    public static int _MaximumPlayers {
        get {
            return GetRef().MaximumPlayers;
        }
    }
    #endregion

    #region Private Functions
    private void setGameSettings(int startLives, float gameLength, float respawnTime,
                                 int minPlayers, int maxPlayers)
    {
        StartLives = startLives;
        GameLength = gameLength;
        RespawnTime = respawnTime;
        MinimumPlayers = minPlayers;
        MaximumPlayers = maxPlayers;
    }
    #endregion
}
