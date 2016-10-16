using UnityEngine;
using System.Collections;

public class RandomlyPitchAudio : MonoBehaviour {

    private AudioSource myAudioSource;
    float originalPitch;
    private float pitchMin;
    private float pitchMax;

	// Use this for initialization
	void Awake () {
        pitchMin = 0.8f;
        pitchMax = 1.2f;
        myAudioSource = GetComponentInParent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    
    void OnEnable()
    {
        myAudioSource.pitch = Random.Range(pitchMin, pitchMax);
    }
}
