using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SwapPlayImage : MonoBehaviour {

    public Sprite[] SpriteSwaps;
    private int currentSprite;
    public Image img;
    private Master m;
    private int practiceMapNum = 1;
    private int currentLevelNum;

	// Use this for initialization
	void OnEnable () {
        m = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
	}
	
	// Update is called once per frame
	void Update () {
        currentLevelNum = m.CurrentLevel;
        if (currentLevelNum == practiceMapNum)
        {
            SetSprite(1);
        }
        else
        {
            SetSprite(0);
        }
    }

    public void SetSprite(int newNum)
    {
        img.sprite = SpriteSwaps[newNum];
    }
}
