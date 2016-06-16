using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SwapPlayImage : MonoBehaviour {

    public Sprite[] SpriteSwaps;
    private int currentSprite;
    public Image img;
    private Master m;

	// Use this for initialization
	void Start () {
        m = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
        if (m.IsTestMode)
        {
            SetSprite(1);
        }
        else
        {
            SetSprite(0);
        }
        
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetSprite(int newNum)
    {
        img.sprite = SpriteSwaps[newNum];
    }
}
