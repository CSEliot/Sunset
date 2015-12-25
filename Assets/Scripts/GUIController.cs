using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GUIController : MonoBehaviour {


    public Sprite[] AllSprites;
    public Master m;

	// Use this for initialization
	void Start () {
        AllSprites = new Sprite[m.PlayableCharacters + 2];
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
