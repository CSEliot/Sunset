using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
/// <summary>
/// Handles from end game up to starting a new game. Rewards winners and 
/// punishes losers. Handles GUI for each.
/// </summary>
public class EndGameManager : MonoBehaviour {

    //Is a Canvas in MainMenu
    /*
     * Immediately coveres the screen
     * Launching function should hand it player with most lives and most kills
     * Tell network to change room state to NOT PLAYER or whatever
     * Unload stage
     * reload stage
     * Reset character select
     * tell N we reset character select
     * Navigation should be done w/ Master
     * 
     */

    public GameObject MainMenuCamera;
    public GameObject Scoreboard;
    private bool onScoreboard;
    
    public int SlomoTime;
    private WaitForSeconds slomoTimeSeconds;
    public float AutoKickTime;
    private WaitForSeconds autoKickSeconds;

    private Dictionary<int, GameObject> Players;
    private delegate void function_to_delay();

    public Master M;

    // Use this for initialization
    void Start () {
        tag = "EndGameManager";
        onScoreboard = false;

        slomoTimeSeconds = new WaitForSeconds(SlomoTime);
        autoKickSeconds = new WaitForSeconds(AutoKickTime);
	}
	
	// Update is called once per frame
	void Update () {
    }
    #region Public Static Methods
    private static EndGameManager getRef()
    {
        for (int x = 0; x < SceneManager.GetSceneAt(0).GetRootGameObjects().Length; x++) {
            if(SceneManager.GetSceneAt(0).GetRootGameObjects()[x].name == "EndGameCanvas") {
                return SceneManager.GetSceneAt(0).GetRootGameObjects()[x].GetComponent<EndGameManager>();
            }
            //CBUG.Do("RootObjName: " + SceneManager.GetSceneAt(0).GetRootGameObjects()[x]);
        }
        CBUG.Error("FRIEND NOT FOUND!");
        return null;
    }

    public static void LaunchEndGame(int[,] KillsMatrix, Dictionary<int, GameObject> Players)
    {
        getRef()._LaunchEndGame(KillsMatrix);
    }

    public static bool GameEnded {
        get {
            return getRef().onScoreboard;
        }
    }
    #endregion

    #region Public Functions
    public void LeaveScoreboard()
    {
        onScoreboard = false;
        GameObject.FindGameObjectWithTag("Networking").GetComponent<NetworkManager>().NewGame();
        GameObject.FindGameObjectWithTag("Master").GetComponent<Master>().NewGame();
        Scoreboard.SetActive(false);
    }
    #endregion

    #region Private Helper Functions
    private void _LaunchEndGame(int[,] KillsMatrix)
    {
        int totalPlayers = KillsMatrix.GetLength(0);
        int mostKills = 0;
        int tempKills = 0;
        int bestKiller = -1;
        bool manyBestKiller = false;
        int leastDeaths = 1000;
        int tempDeaths = 0;
        int bestSurvivor = -1;
        bool manyBestSurvivor = false;

        //CBUG.Do("X max: " + totalPlayers);
        for(int x = 0; x < totalPlayers; x++) {
            for(int y = 0; y < totalPlayers; y++) {
                //CBUG.Do("Y Max: " + KillsMatrix.GetLength(1));
                //CBUG.Do("" + x + " Killed " + y  + "|" + KillsMatrix[x, y]);
                tempKills += KillsMatrix[x,y];
            }
            if(tempKills == leastDeaths) {
                manyBestKiller = true;
            }
            if (tempKills > mostKills) {
                mostKills = tempKills;
                bestKiller = x;
                manyBestKiller = false;
            }
        }

        for (int y = 0; y < totalPlayers; y++) {
            for (int x = 0; x < totalPlayers; x++) {
                tempDeaths += KillsMatrix[x, y];
            }
            if(tempDeaths == leastDeaths) {
                manyBestSurvivor = true;
            }
            if (tempDeaths < leastDeaths) {
                leastDeaths = tempDeaths;
                bestSurvivor = y;
                manyBestSurvivor = false;
            }
        }

        if(bestKiller != -1) {
            CBUG.Do("Player " + bestKiller + " Won Most Kills at: " + mostKills);
        }else {
            CBUG.Do("No Best Killer!");
        }
        if(bestSurvivor != -1) {
            CBUG.Do("Player " + bestSurvivor + "Survived the Longest at: " + leastDeaths);
        }else {
            CBUG.Do("No best survivor! Everyone wins!");
        }
        
        if (manyBestSurvivor) {
            if (manyBestKiller) {
                GameHUDController.Won();
            } else if (NetID.Convert(PhotonNetwork.player.ID) == bestKiller) {
                GameHUDController.Won();
            } else {
                GameHUDController.Lost();
            }
        } else if (NetID.Convert(PhotonNetwork.player.ID) == bestSurvivor) {
            CBUG.Do("I WIN! netID: " + PhotonNetwork.player.ID + " realID: " + NetID.Convert(PhotonNetwork.player.ID));
            GameHUDController.Won();
        } else {
            GameHUDController.Lost();
        }
        StartCoroutine(slowTime());
        StartCoroutine(loadScoreboard());
        StartCoroutine(reloadArena());
        StartCoroutine(autoKick());
    }

    private IEnumerator loadScoreboard()
    {
        yield return slomoTimeSeconds;
        onScoreboard = true;
        Scoreboard.SetActive(true);
        MainMenuCamera.SetActive(true);
    }

    private IEnumerator slowTime()
    {
        Time.timeScale = 0.5f;
        Time.fixedDeltaTime = 1f / 30f;
        yield return slomoTimeSeconds;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 1f / 60f;
    }

    private IEnumerator delayFunction(function_to_delay f, float delayBy)
    {
        yield return new WaitForSeconds(delayBy);
        f();
    }

    private IEnumerator reloadArena()
    {
        yield return slomoTimeSeconds;
        SceneManager.UnloadScene(GameObject.FindGameObjectWithTag("Master").GetComponent<Master>().CurrentMap);
        yield return new WaitForSeconds(Time.deltaTime);
        SceneManager.LoadScene(GameObject.FindGameObjectWithTag("Master").GetComponent<Master>().CurrentMap, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneAt(1));
    }

    /// <summary>
    /// Kicks players who haven't left scoreboard screen.
    /// </summary>
    /// <returns></returns>
    private IEnumerator autoKick()
    {
        yield return autoKickSeconds;
        if (onScoreboard) {
            LeaveScoreboard();
            M.GoBack();
        }
    }
    #endregion
}
