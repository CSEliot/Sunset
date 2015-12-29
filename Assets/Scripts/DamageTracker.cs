using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Handles Visuals AND stats.
/// </summary>
public class DamageTracker : MonoBehaviour {

    public Text Lives_Text;
    public Text Damage_Text;
    public GameObject YouLoseText;
    public GameObject YouWinText;

    public int StartingLives;
    private int lives;
    private int damage;

	// Use this for initialization
	void Start () {
        lives = StartingLives;
        damage = 0;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void IncreaseDamageBy(float increase)
    {
        damage += (int)increase;
        Damage_Text.text = ""+ damage + "%";
    }

    public void SetDamageTo(float amount)
    {
        damage = (int)amount;
        Damage_Text.text = "" + damage + "%";
    }

    public int GetDamage()
    {
        return damage;
    }

    public int GetLives()
    {
        return lives;
    }

    public void ResetLives()
    {
        lives = StartingLives;
        Lives_Text.text = "" + lives; 
    }

    public void ResetDamage()
    {
        damage = 0;
        Damage_Text.text = "000%";
    }
    public void LoseALife()
    {
        lives--;
        Lives_Text.text = "" + lives; 
    }

    public void Lost()
    {
        YouLoseText.SetActive(true);
    }

    public void Won()
    {
        YouWinText.SetActive(true);
    }
}
