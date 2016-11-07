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
    private bool gameEnded;
    private float startTime;
    private float respawnTime;
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

    public bool IsLocalGame;
    public int LocalPlayers;

    //private enum GameState
    //{
    //    Waiting,
    //    Playing,
    //    Spectating
    //}

    // Use this for initialization
    void Start () {
        gameStarted = false;
        gameEnded = false;
        SettingsManager.GetGameInfo(ref startingLives,
                                    ref gameLength,
                                    ref respawnTime);
        respawnWait = new WaitForSeconds(respawnTime);
        Players = new Dictionary<int, GameObject>();
        if (IsLocalGame) {
            startingLives = int.MaxValue;
            N = new NetworkManager();
            N.ReadyTotal = LocalPlayers;
            startLocal();
            foreach(GameObject O in GameObject.FindGameObjectsWithTag("PlayerSelf")) {
                Players.Add(O.GetComponent<PlayerController2DOffline>().ID, O);
            }
            return;
        }
	    NetworkManager.SetGameStateMan(out N, this);
	}
	
	// Update is called once per frame
	void Update () {

        if (!gameStarted || IsLocalGame || gameEnded)
            return;

        if(startingPlayers - 1 == totalGhosts || 
            Time.time > startTime + gameLength) {
            //END GAME SEQUENCE
            endGame();
        }

        if (CBUG.DEBUG_ON && ((int)(Time.time))%20 == 0) {
            CBUG.Do("Time Remaining: " + (int)(startTime + gameLength - Time.time) );
        }

	}

    #region Public Static Functions

    public static bool GameStarted
    {
        get {
            return getRef().gameStarted;
        }
    }

    public static bool GameEnded
    {
        get {
            return getRef().gameEnded;
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

    public static void GameStart(int myID, string charName)
    {
        getRef()._GameStart(myID, charName);   
    }

    public static void HandleDeath(int killed)
    {
        getRef()._HandleDeath(killed);
    }

    public static void AddPlayer(int ID, GameObject Player)
    {
        Players.Add(ID, Player);
        EndGameManager.Players.Add(ID, Player);
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
        gameLength = SettingsManager._GameLength;

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
            playerLives[x] = SettingsManager._StartLives;
        }
        GameHUDController.SetLives(SettingsManager._StartLives);
    }
    
    private void startLocal()
    { 
        gameStarted = true;
        SettingsManager.SetGameInfo(startingLives, gameLength);
        startTime = Time.time;

        startingPlayers = N.ReadyTotal;
        gameLength = SettingsManager._GameLength;

        killsMatrix = new int[N.ReadyTotal, N.ReadyTotal];
        for (int x = 0; x < N.ReadyTotal; x++) {
            for (int y = 0; y < N.ReadyTotal; y++) {
                killsMatrix[x, y] = 0;
            }
        }

        SpawnPositions = new Transform[N.ReadyTotal];
        for (int x = 0; x < N.ReadyTotal; x++) {
            SpawnPositions[x] = GameObject.FindGameObjectWithTag("SpawnPoints").transform.GetChild(x);
        }

        playerLives = new int[N.ReadyTotal];
        for (int x = 0; x < N.ReadyTotal; x++) {
            playerLives[x] = SettingsManager._StartLives;
        }
    }

    private void endGame()
    {
        gameEnded = true;
        EndGameManager.LaunchEndGame(killsMatrix, Players);
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
        if(IsLocalGame)
            StartCoroutine(doRespawnOrGhost<PlayerController2DOffline>(killed));
        else
            StartCoroutine(doRespawnOrGhost<PlayerController2DOnline>(killed));
    }

    private IEnumerator doRespawnOrGhost<PlayerController>(int deadPlayerNum) where PlayerController : PlayerController2D
    {
        if (playerLives[deadPlayerNum] == -1) {
            totalGhosts++;
            Players[deadPlayerNum].GetComponent<PlayerController>().Ghost();
            //ONLY OUR PLAYER SHOPULD SPECTATE MODE
            if (deadPlayerNum == NetID.ConvertToSlot(PhotonNetwork.player.ID)){
                WaitUIController.ActivateSpectatingMode();
            }
            yield return null;
        } else {
            yield return respawnWait;
            //Player spawn position is controlled by Game Manager.
            //But we only wanna reposition OUR player's position.
            //BUT ALSO
            Players[deadPlayerNum].GetComponent<PlayerController>().Respawn(
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
