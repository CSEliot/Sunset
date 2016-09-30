using UnityEngine;
using UnityEngine.UI;

using System.Collections;

[RequireComponent (typeof (Text))]
public class AnimateText : MonoBehaviour {

    public float LoopTime;
    public string[] TextFrames;


    private Text TargetText;
    private float frameTime;
    private float lastFrametime;
    private int currentFrame;
    private int totalFrames;

	// Use this for initialization
	void Start () {
        TargetText = GetComponent<Text>();
        totalFrames = TextFrames.Length;
        frameTime = LoopTime / totalFrames;
        lastFrametime = Time.time;
        TargetText.text = TextFrames[0];
    }
	
	// Update is called once per frame
	void Update () {
	
        if(Time.time - lastFrametime > LoopTime) {
            currentFrame++;
            lastFrametime = Time.time;
            TargetText.text = TextFrames[currentFrame%totalFrames];
        }

	}
}
