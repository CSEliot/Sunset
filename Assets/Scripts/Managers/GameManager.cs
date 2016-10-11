using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Manages State(Waiting, Playing, Spectating),
/// Spawning and Kills
/// TODO: Rename map stuff to Stage stuff
/// </summary>
public class GameManager : MonoBehaviour {

    private bool gameStarted;
    private float startTime;
    public float RespawnTime;
    private float gameLength;
    private WaitForSeconds respawnWait;
    private int[] playerLives;
    private NetworkManager N;
    private Master M;
    private int startingPlayers;
    private int startingLives;
    private int totalGhosts;
    public static Dictionary<int, GameObject> Players;
    public int[,] killsMatrix; //x -> y // X killed Y
    
    private Transform[] SpawnPositions;

    //public int TotalSpawns;

    //private enum GameState
    //{
    //    Waiting,
    //    Playing,
    //    Spectating
    //}

    // Use this for initialization
    void Start () {
        gameStarted = false;
        startingLives = 2;
        gameLength = 3f*60f; //Seconds
        respawnWait = new WaitForSeconds(RespawnTime);
        Players = new Dictionary<int, GameObject>();
	    NetworkManager.SetGameStateMan(out N, this);
	}
	
	// Update is called once per frame
	void Update () {

        if (!gameStarted)
            return;

        if(startingPlayers - 1 == totalGhosts || 
            Time.time > startTime + gameLength) {
            //END GAME SEQUENCE
            endGame();
        }

        if (CBUG.DEBUG_ON && ((int)(Time.time))%10 == 0) {
            CBUG.Do("Time Remaining: " + (int)(startTime + gameLength - Time.time) );
        }

	}

    #region Public Static Functions
    /// <summary>
    /// Records death and
    /// </summary>
    /// <param name="killer"></param>
    /// <param name="killed"></param>
    public static void RecordDeath(int killer, int killed)
    {
        getRef()._RecordDeath(killer, killed);
    }

    public static void GameStart(int myID, string charName)
    {
        getRef()._GameStart(myID, charName);   
    }

    public static void HandleDeath(int killed)
    {
        getRef()._HandleDeath(killed);
    }
    #endregion

    public bool IsGhost(int playerNum)
    {
        return (playerLives[playerNum] < 0);
    }

    private void checkForWinner()
    {
        if (totalGhosts >= startingPlayers - 1) {
            //END GAME STUFF
        }
    }

    #region Helper Functions
    private void _GameStart(int myID, string charName)
    {
        gameStarted = true;
        SettingsManager.SetGameInfo(startingLives, gameLength);
        startTime = Time.time;

        startingPlayers = N.ReadyTotal;
        gameLength = SettingsManager.GameLength;

        killsMatrix = new int[N.ReadyTotal, N.ReadyTotal];
        for(int x = 0; x < N.ReadyTotal; x++) {
            for(int y = 0; y < N.ReadyTotal; y++) {
                killsMatrix[x, y] = 0;
            }
        }

        SpawnPositions = new Transform[N.ReadyTotal];
        for (int x = 0; x < N.ReadyTotal; x++) {
            SpawnPositions[x] = GameObject.FindGameObjectWithTag("SpawnPoints").transform.GetChild(x);
        }

        CBUG.Do("Player " + myID + " Spawning " + charName);
        Vector3 spawnPos;
        spawnPos = SpawnPositions[myID].position;

        PhotonNetwork.Instantiate(charName, spawnPos, Quaternion.identity, 0);

        playerLives = new int[N.ReadyTotal];
        for (int x = 0; x < N.ReadyTotal; x++) {
            playerLives[x] = SettingsManager.StartLives;
        }
        GameHUDController.SetLives(SettingsManager.StartLives);
    }

    private void endGame()
    {
        CBUG.Do("GAME ENDS BRUH ");
        gameStarted = false;

        EndGameManager.LaunchEndGame(killsMatrix);
        CBUG.Do("IT OVER");
    }

    private void _RecordDeath(int killer, int killed)
    {
        /*
         * Players know when they've killed someone, but they think they
         * suicided when someone kills them.
         */

        //Note: Killer=0 means player suicided.
        if(killer != -1) {
            killsMatrix[killer, killed]++;
            CBUG.Do("Player " + (killer+1) + 
                    " knocked out Player " + (killed+1));
        } else {
            CBUG.Do("Player " + (killed+1) + " Suicided!");
        }
        playerLives[killed]--;
    }

    private void _HandleDeath(int killed)
    {
        StartCoroutine(doRespawnOrGhost(killed));
    }

    private IEnumerator doRespawnOrGhost(int deadPlayerNum)
    {
        yield return respawnWait;
        if (playerLives[deadPlayerNum] == -1) {
            totalGhosts++;
            Players[deadPlayerNum].GetComponent<PlayerController2D>().Ghost();
            //ONLY OUR PLAYER SHOPULD SPECTATE MODE
            WaitUIController.ActivateSpectatingMode();
        } else {
            //Player spawn position is controlled by Game Manager.
            //But we only wanna reposition OUR player's position.
            //BUT ALSO
            Players[deadPlayerNum].GetComponent<PlayerController2D>().Respawn(
                SpawnPositions[Random.Range(0, SpawnPositions.Length - 1)].position
            );
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
