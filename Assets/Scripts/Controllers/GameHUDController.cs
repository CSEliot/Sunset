using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles Visuals AND stats.
/// </summary>
public class GameHUDController : MonoBehaviour {

    public Text Lives_Text;
    public Text Damage_Text;
    public GameObject YouLoseText;
    public GameObject YouWinText;
    
    private int lives;
    private int damage;

	// Use this for initialization
	void Start () {
        lives = SettingsManager._StartLives;
        damage = 0;
        tag = "GameHUD";
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    #region Static Reference Functions
    private static GameHUDController getRef()
    {
        return GameObject.FindGameObjectWithTag("GameHUD").GetComponent<GameHUDController>();
    }

    public static void SetLives(int startingLives)
    {
        getRef()._SetLives(startingLives);
    }

    public static void Won()
    {
        getRef()._Won();
    }

    public static void Lost()
    {
        getRef()._Lost();
    }

    public static void IncreaseDamageBy(float increase)
    {
        getRef()._IncreaseDamageBy(increase);
    }

    public static void SetDamageTo(float amount)
    {
        getRef()._SetDamageTo(amount);
    }

    public static void ResetDamage()
    {
        getRef()._ResetDamage();
    }

    public static void LoseALife()
    {
        getRef()._LoseALife();
    }
    #endregion

    #region Private Helper Functions
    private void _SetLives(int startingLives)
    {
        Lives_Text.text = "" + startingLives;
        lives = startingLives;
    }

    private void _Won()
    {
        YouWinText.SetActive(true);
    }

    private void _Lost()
    {
        YouLoseText.SetActive(true);
    }

    private void _IncreaseDamageBy(float increase)
    {
        damage += (int)increase;
        Damage_Text.text = "" + damage + "%";
    }

    private void _SetDamageTo(float amount)
    {
        damage = (int)amount;
        Damage_Text.text = "" + damage + "%";
    }

    private void _ResetDamage()
    {
        damage = 0;
        Damage_Text.text = "000%";
    }

    private void _LoseALife()
    {
        lives--;
        Lives_Text.text = "" + lives;
    }
    #endregion

}
