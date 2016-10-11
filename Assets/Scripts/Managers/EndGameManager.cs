using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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

	// Use this for initialization
	void Start () {
        tag = "EndGameManager";
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
            CBUG.Do("RootObjName: " + SceneManager.GetSceneAt(0).GetRootGameObjects()[x]);
        }
        CBUG.Do("FRIEND NOT FOUND!");
        return null;
    }

    public static void LaunchEndGame(int[,] KillsMatrix)
    {
        getRef()._LaunchEndGame(KillsMatrix);
    }
    #endregion

    #region Private Helper Functions
    private void _LaunchEndGame(int [,] KillsMatrix)
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

        CBUG.Do("X max: " + totalPlayers);
        for(int x = 0; x < totalPlayers; x++) {
            for(int y = 0; y < totalPlayers; y++) {
                CBUG.Do("Y Max: " + KillsMatrix.GetLength(1));
                CBUG.Do("" + x + " Killed " + y  + "|" + KillsMatrix[x, y]);
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
        CBUG.Do("Player " + bestKiller + " Won Most Kills at: " + mostKills);
        CBUG.Do("Player " + bestSurvivor + "Survived the Longest at: " + leastDeaths);
        
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
    }

    private IEnumerator slowTime()
    {
        yield return new WaitForSeconds(0.5f);
        Time.timeScale = 0.5f;
        Time.fixedDeltaTime = 1f / 30f;
        yield return new WaitForSeconds(2f);
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 1f / 60f;
        leaveGame();
    }

    private void leaveGame()
    {
        GameObject.FindGameObjectWithTag("Networking").GetComponent<NetworkManager>().NewGame();
        GameObject.FindGameObjectWithTag("Master").GetComponent<Master>().NewGame();
        SceneManager.UnloadScene(GameObject.FindGameObjectWithTag("Master").GetComponent<Master>().CurrentMap);
        StartCoroutine(reload());
    }

    private IEnumerator reload()
    {
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene(GameObject.FindGameObjectWithTag("Master").GetComponent<Master>().CurrentMap, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneAt(1));
    }
    #endregion
}
