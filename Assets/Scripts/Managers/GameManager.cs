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
    private float secondsPlayed;
    private float respawnTime;
    private float gameLength;
    private WaitForSeconds respawnWait;
    private int[] playerLives;
    private NetworkManager N;
    private Master M;
    private int startingPlayers;
    private int startingLives;
    private int totalGhosts;
    public static Dictionary<int, GameObject> PlayableCharacters;
    public int[,] killsMatrix; //x -> y // X killed Y
    
    enum k
    {
        x,
        y
    };

    k z;
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
        secondsPlayed = 0;
        gameStarted = false;
        gameEnded = false;
        SettingsManager.GetGameInfo(ref startingLives,
                                    ref gameLength,
                                    ref respawnTime);
        respawnWait = new WaitForSeconds(respawnTime);
        PlayableCharacters = new Dictionary<int, GameObject>();
        if (IsLocalGame) {
            startingLives = int.MaxValue;
            N = new NetworkManager();
            N.ReadyTotal = LocalPlayers;
            startLocal();
            foreach(GameObject PbC in GameObject.FindGameObjectsWithTag("PlayerSelf")) {
                PlayableCharacters.Add(PbC.GetComponent<PlayerController2DOffline>().ID, PbC);
            }
            return;
        }
	    NetworkManager.SetGameStateMan(out N, this);
	}

    // Update is called once per frame
    void Update () {

        if (!gameStarted || IsLocalGame || gameEnded)
            return;

        if(startingPlayers - 1 <= totalGhosts || 
            Time.time > startTime + gameLength) {
            //END GAME SEQUENCE
            endGame();
        }

        if(secondsPlayed + startTime < Time.time)
        {
            secondsPlayed++;
            GameHUDController.SetClock("" + (gameLength - secondsPlayed));//"" + (secondsPlayed / 60) + ":" + (secondsPlayed - (secondsPlayed / 60) * 60));
        }

        if (CBUG.DEBUG_ON && ((int)(Time.time))%20 == 0) {
            //CBUG.Do("Time Remaining: " + (int)(startTime + gameLength - Time.time) );
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

    public static void GameStart(int myID, string charName)
    {
        getRef()._GameStart(myID, charName);   
    }

    public static void HandleDeath(int Killer, int Killed, bool IsDisconnect)
    {
        getRef()._HandleDeath(Killer, Killed, IsDisconnect);
    }

    public static void AddPlayer(int ID, GameObject Player)
    {
        PlayableCharacters.Add(ID, Player);
        EndGameManager.Players.Add(ID, Player);
    }

    /// <summary>
    /// For SinglePlayer Game use. Tells all other PCs they're not selected.
    /// </summary>
    public static void UnselectControlled(GameObject NewSelect)
    {
        foreach(GameObject PC in PlayableCharacters.Values)
        {
            PC.GetComponent<PlayerController2DOffline>().IsPlayerControlled = false;
        }
        NewSelect.GetComponent<PlayerController2DOffline>().IsPlayerControlled = true;
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
        secondsPlayed = 0;
        GameHUDController.SetClock("" + (gameLength - secondsPlayed));// (secondsPlayed / 60) + ":" + (secondsPlayed - (secondsPlayed / 60) * 60));")

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
        EndGameManager.LaunchEndGame(killsMatrix, PlayableCharacters);
    }

    private void recordDeath(int killer, int killed, bool isDisconnect)
    {
        /*
         * Players know when they've killed someone, but they think they
         * suicided when someone kills them.
         */
        //Not sure if above is still issue.

        if (isDisconnect)
        {
            killer = -1;
            CBUG.Do("Player " + killed + " disconnected midgame!");
            playerLives[killed] = 0;
        }
        else
        {
            //Note: Killer=killed means player suicided.
            killsMatrix[killer, killed]++;
            if(killer != killed) {
                CBUG.Do("Player " + (killer) + " knocked out Player " + (killed));
            } else {
                //killsMatrix[killer, killed]++;
                CBUG.Do("Player " + (killed) + " Suicided!");
            }
            playerLives[killed]--;
        }
        EndGameManager.CalculateBests(killsMatrix);
    }

    private void _HandleDeath(int killer, int killed, bool isDisconnect)
    {
        Dictionary<int, GameObject> t = PlayableCharacters;
        if(IsLocalGame)
            StartCoroutine(doRespawnOrGhost<PlayerController2DOffline>(killed, isDisconnect));
        else
            StartCoroutine(doRespawnOrGhost<PlayerController2DOnline>(killed, isDisconnect));
        recordDeath(killer, killed, isDisconnect);
    }

    private IEnumerator doRespawnOrGhost<PlayerController>(int deadPlayerNum, bool isDisconnect) where PlayerController : PlayerController2D
    {
        if (playerLives[deadPlayerNum] <= 0 || isDisconnect) {
            totalGhosts++;
            PlayableCharacters[deadPlayerNum].GetComponent<PlayerController>().Ghost(); 
            //ONLY OUR PLAYER SHOPULD SPECTATE MODE
            if (deadPlayerNum == NetIDs.PlayerNumber(PhotonNetwork.player.ID)){
                WaitGUIController.ActivateSpectatingMode();
            }
            yield return null;
        } else {
            yield return respawnWait;
            //Player spawn position is controlled by Game Manager.
            //But we only wanna reposition OUR player's position.
            //BUT ALSO
            PlayableCharacters[deadPlayerNum].GetComponent<PlayerController>().Respawn(
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
