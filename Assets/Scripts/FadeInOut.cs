using UnityEngine;
using UnityEngine.UI;
using System.Collections;


/// <summary>
/// Specifically for fading TEXT in and out. 
/// </summary>
[RequireComponent( typeof(Text))]
public class FadeInOut : MonoBehaviour {

    [Range(0.0f, 1.0f)]
    public float FadeSpeed = 0.5f;

    private Text targetText;
    private float alpha;
    

	// Use this for initialization
	void Start () {

        targetText = GetComponent<Text>();

	}
	
	// Update is called once per frame
	void Update () {

        alpha = Mathf.Sin(Time.time * FadeSpeed);

        targetText.color = new Color(
            targetText.color.r,
            targetText.color.g,
            targetText.color.b,
            alpha
            );

	}
}
