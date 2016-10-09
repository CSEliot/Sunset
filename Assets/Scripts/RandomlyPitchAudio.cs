using UnityEngine;
using System.Collections;

public class RandomlyPitchAudio : MonoBehaviour {

    private AudioSource myAudioSource;
    float originalPitch;
    private float varyPitchBy;

	// Use this for initialization
	void Start () {
        varyPitchBy = 0.6f;
        myAudioSource = GetComponent<AudioSource>();
        originalPitch = myAudioSource.pitch;

        myAudioSource.volume = Master.GetSFXVolume();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void Awake()
    {
    }

    void OnEnable()
    {
        Start();
        myAudioSource.pitch = Random.Range(-varyPitchBy, varyPitchBy) + originalPitch;
        myAudioSource.Play();
    }
}
