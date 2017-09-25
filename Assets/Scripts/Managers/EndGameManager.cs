using UnityEngine;
using UnityEngine.UI;
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

    /// <summary>
    /// Assigned new players by a GameManager upon instantiation.
    /// </summary>
    public static Dictionary<int, GameObject> Players;
    private delegate void function_to_delay();

    public NetworkManager N;
    public Master M;
    public GameObject AnimRays;

    public GameObject[] RootPlayerSlots;

    public Image[] playerImages;
    public GameObject[] winnerGraphicImages;

    public Text[] textUsernames;
    public Text[] textUsernames_BG;

    public Text[] WinnerTexts;
    public Text[] WinnerTexts_BG;

    private List<int> BestKillers; //Only relevant to most recent game
    private List<int> BestSurvivors; //Only relevant to most recent game

    // Use this for initialization
    void Start () {

        Players = new Dictionary<int, GameObject>();

        tag = "EndGameManager";
        onScoreboard = false;

        slomoTimeSeconds = new WaitForSeconds(SlomoTime);
        autoKickSeconds = new WaitForSeconds(AutoKickTime);

        BestKillers = new List<int>();
        BestSurvivors = new List<int>();
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

    public static void CalculateBests(int[,] KillsMatrix)
    {
        getRef()._CalculateBests(KillsMatrix);
    }
    #endregion

    #region Public Functions
    public void LeaveScoreboard()
    {
        onScoreboard = false;
        GameObject.FindGameObjectWithTag("Master").GetComponent<Master>().NewGame();
        Scoreboard.SetActive(false);
    }
    #endregion

    #region Private Helper Functions
    private void _CalculateBests(int[,] killsMatrix)
    {

        int totalPlayers = killsMatrix.GetLength(0);
        int mostKills = 0;
        int tempKills = 0;
        int leastDeaths = SettingsManager._StartLives;
        int tempDeaths = 0;

        BestKillers.Clear();
        BestSurvivors.Clear();
        //CBUG.Do("X max: " + totalPlayers);
        //killed by
        for (int y = 0; y < totalPlayers; y++)
        {
            tempDeaths = 0;
            for (int x = 0; x < totalPlayers; x++)
            {
                    tempDeaths += killsMatrix[x, y];
                    CBUG.Do("Y Max: " + killsMatrix.GetLength(1));
                    CBUG.Do("" + x + " Killed " + y + "|" + killsMatrix[x, y]);
            }
            if (tempDeaths < leastDeaths)
            {
                BestSurvivors.Clear();
                leastDeaths = tempDeaths;
                BestSurvivors.Add(y);
            }
            else if (tempDeaths == leastDeaths)
            {
                BestSurvivors.Add(y);
            }
        }

        for (int x = 0; x < totalPlayers; x++)
        {
            tempKills = 0;
            for (int y = 0; y < totalPlayers; y++)
            {
                if (x != y)
                    tempKills += killsMatrix[x, y];
            }
            if (tempKills == mostKills)
            {
                BestKillers.Add(x);
            }
            else if (tempKills > mostKills)
            {
                BestKillers.Clear();
                mostKills = tempKills;
                BestKillers.Add(x);
            }
        }
    }
    private void _LaunchEndGame(int[,] KillsMatrix)
    {

        CalculateBests(KillsMatrix);

        for(int x = 0; x < 5; x++) {
            WinnerTexts[x].text = "";
            WinnerTexts_BG[x].text = "";
            textUsernames[x].text = "";
            textUsernames_BG[x].text = "";
            RootPlayerSlots[x].SetActive(false);
            winnerGraphicImages[x].SetActive(false);
        }
        
        int startSlot = Mathf.Clamp(4 - N.ReadyTotal, 0, 5);
        int endSlot = Mathf.Clamp(startSlot + N.ReadyTotal, 0, 5);
        int tempPlayerNum = 0;
        for (int x = startSlot; x < endSlot; x++) {
            RootPlayerSlots[x].SetActive(true);
            playerImages[x].sprite = M.AllCharacters[N.GetCharNum(NetIDs.GetNetID(tempPlayerNum))].GetComponentInChildren<Image>().sprite;
            if(BestKillers.Contains(tempPlayerNum) || BestSurvivors.Contains(tempPlayerNum)) {
                winnerGraphicImages[x].SetActive(true);
                if (BestKillers.Contains(tempPlayerNum)) {
                    WinnerTexts[x].text += "Most Kills";
                    WinnerTexts_BG[x].text += "Most Kills";
                }
                if(BestKillers.Contains(tempPlayerNum) && BestSurvivors.Contains(tempPlayerNum)) {
                    WinnerTexts[x].text += "\n";
                    WinnerTexts_BG[x].text += "\n";
                }
                if (BestSurvivors.Contains(tempPlayerNum)) {
                    WinnerTexts[x].text += "Most Lives";
                    WinnerTexts_BG[x].text += "Most Lives";
                }
            }
            tempPlayerNum++;
        }

        //if (manyBestSurvivor) {
        //    if (manyBestKiller) {
        //        GameHUDController.Won();
        //    } else if (NetID.Convert(PhotonNetwork.player.ID) == bestKiller) {
        //        GameHUDController.Won();
        //    } else {
        //        GameHUDController.Lost();
        //    }
        //} else if (NetID.Convert(PhotonNetwork.player.ID) == bestSurvivor) {
        //    CBUG.Do("I WIN! netID: " + PhotonNetwork.player.ID + " realID: " + NetID.Convert(PhotonNetwork.player.ID));
        //    GameHUDController.Won();
        //} else {
        //    GameHUDController.Lost();
        //}
        StartCoroutine(clearPlayers());
        StartCoroutine(slowTime());
        StartCoroutine(loadScoreboard());
        StartCoroutine(reloadArena());
        StartCoroutine(autoKick());
        //StartCoroutine(savePlayers());
    }

    //private IEnumerator savePlayers()
    //{
    //    yield return slomoTimeSeconds;
    //    int i = 0;
        
    //    foreach (GameObject player in Players.Values) {
    //        player.transform.SetParent(null);
    //        player.GetComponent<PlayerController2DOnline>().Freeze();
    //        i++;
    //    }
    //    yield return null;
    //    i = 0;
    //    foreach (GameObject player in Players.Values) {
    //        player.transform.position = RootPlayerSlots[i].transform.position;
    //        SceneManager.MoveGameObjectToScene(player, SceneManager.GetSceneAt(0));
    //        i++;
    //    }
    //    yield return null;
    //    foreach (GameObject player in Players.Values) {
    //        player.GetComponent<PlayerController2DOnline>().UnFreeze();
    //    }
    //}

    private IEnumerator loadScoreboard()
    {
        yield return slomoTimeSeconds;
        AnimRays.SetActive(false);
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
        //yield return null;
        //yield return null;
        //yield return null; //Stall by 3 frames to allow players to be saved in savePlayers()
        GameObject.FindGameObjectWithTag("Networking").GetComponent<NetworkManager>().NewGame();
        SceneManager.UnloadSceneAsync(GameObject.FindGameObjectWithTag("Master").GetComponent<Master>().CurrentMap);
        yield return new WaitForSeconds(0.25f); //0.25f = arbitrary delay to allow for unload.
        SceneManager.LoadScene(GameObject.FindGameObjectWithTag("Master").GetComponent<Master>().CurrentMap, LoadSceneMode.Additive);
        SceneManager.SetActiveScene(SceneManager.GetSceneAt(1));
        AnimRays.SetActive(true);
    }

    private IEnumerator clearPlayers()
    {
        yield return slomoTimeSeconds;
        foreach (GameObject plObj in Players.Values) {
            Destroy(plObj);
        }
        Players.Clear();
    }

    /// <summary>
    /// Kicks players who haven't left scoreboard screen.
    /// </summary>
    /// <returns></returns>
    private IEnumerator autoKick()
    {
        yield return slomoTimeSeconds;
        yield return autoKickSeconds;
        if (onScoreboard) {
            LeaveScoreboard();
            M.GoBack();
        }
    }
    #endregion
}
