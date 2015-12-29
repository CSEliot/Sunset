using UnityEngine;
using System.Collections;

public class RandomlyPitchAudio : MonoBehaviour {

    private AudioSource myAudioSource;
    float originalPitch;
    public float VaryPitchBy = 0.5f;

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void Awake()
    {
        myAudioSource = GetComponent<AudioSource>();
        originalPitch = myAudioSource.pitch;
    }

    void OnEnable()
    {
        myAudioSource.pitch = Random.Range(-VaryPitchBy, VaryPitchBy) + originalPitch;
        myAudioSource.Play();
    }
}
