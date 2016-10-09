using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Manages State(Waiting, Playing, Spectating),
/// Spawning and Kills
/// TODO: Rename map stuff to Stage stuff
/// </summary>
public class GameManager : MonoBehaviour {

    private float startTime;
    public float GameLength; // In Seconds
    public float RespawnTime;
    private WaitForSeconds respawnWait;
    private int[] playerLives;
    private NetworkManager N;
    private Master M;
    private int startingPlayers;
    private int startingLives;
    private int totalGhosts;
    public static Dictionary<int, GameObject> Players;
    public static int[,] KillsMatrix; //x -> y // X killed Y

    private Transform[] SpawnPositions;
    //public int TotalSpawns;

    public WaitUIController waitScreen;



    private enum GameState
    {
        Waiting,
        Playing,
        Spectating
    }

	// Use this for initialization
	void Start () {
        respawnWait = new WaitForSeconds(RespawnTime);
        KillsMatrix = new int[Master.MaxRoomSize,Master.MaxRoomSize];
        Players = new Dictionary<int, GameObject>();
	    NetworkManager.SetGameStateMan(out N, this);

        SpawnPositions = new Transform[Master.MaxRoomSize];

        for(int x = 0; x < Master.MaxRoomSize; x++) {
            SpawnPositions[x] = GameObject.FindGameObjectWithTag("SpawnPoints").transform.GetChild(x);
        }
        startingLives = SettingsManager.StartLives;
	}
	
	// Update is called once per frame
	void Update () {
	
        if(startingPlayers - 1 == totalGhosts || 
            Time.time > startTime + GameLength) {
            //END GAME SEQUENCE
        }

	}

    /// <summary>
    /// Records death and
    /// </summary>
    /// <param name="killer"></param>
    /// <param name="killed"></param>
    public static void RecordDeath(int killer, int killed)
    {
        getRef()._RecordDeath(killer, killed);
    }


    public void Respawn(int playerNum)
    {

    }

    public bool IsGhost(int playerNum)
    {
        return (playerLives[playerNum] <= 0);
    }

    private void checkForWinner()
    {
        if (totalGhosts >= startingPlayers - 1) {
            //END GAME STUFF
        }
    }


    public static void GameStart(int myID, string charName)
    {
        getRef()._GameStart(myID, charName);   
    }

    public void Spectate()
    {

    }

    #region Helper Functions
    private void _GameStart(int myID, string charName)
    {
        CBUG.Do("Player " + myID + " Spawning " + charName);
        Vector3 spawnPos;
        spawnPos = SpawnPositions[myID].position;

        PhotonNetwork.Instantiate(charName, spawnPos, Quaternion.identity, 0);
    }

    private void _RecordDeath(int killer, int killed)
    {
        playerLives[killed]--;
        KillsMatrix[killer, killed]++;
        StartCoroutine(onDeath(killed));
    }

    private IEnumerator onDeath(int deadPlayerNum)
    {
        yield return respawnWait;
        if (playerLives[deadPlayerNum] == -1) {
            totalGhosts++;
            Players[deadPlayerNum].GetComponent<PlayerController2D>().Ghost();
        }else {
            Players[deadPlayerNum].GetComponent<PlayerController2D>().Respawn();
        }
    }

    private static GameManager getRef()
    {
        return GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
    }
    #endregion
}
/*        if (!cameraFollowAssigned)
            assignCameraFollow(transform);
        if (!battleUIAssigned)
        {
            GameUI = GameObject.FindGameObjectWithTag("GameUI")
                .GetComponent<GameHUD>();
            battleUIAssigned = true;
        }
        if (!matchHUDAssigned)
        {
            stageReadyWaitGUI = GameObject.FindGameObjectWithTag ("GameUI")
                .GetComponent<GameHUD>().MatchHudComp;
            matchHUDAssigned = true;
        }

        private void assignCameraFollow(Transform myTransform)
    {
        if (myTransform == null)
        {
            return;
        }
        camShaker.SetTarget(myTransform);  
        cameraFollowAssigned = true;
    }

            if (playersSpawned)
        {
            if (GameObject.FindGameObjectsWithTag("PlayerSelf").Length <= 1)
            {
                if (!isDead)
                {
                    GameUI.Won();
                }
            }
        }
        else
        {
            if (GameObject.FindGameObjectsWithTag("PlayerSelf").Length > 1)
            {
                playersSpawned  = true;
            }
        }

    
        //stageReadyWaitGUI.gameObject.SetActive(true);
        //stageReadyWaitGUI.ActivateSpectating();
        GameUI.Lost();
*/
