using UnityEngine;
using System.Collections;

public class ToggleParticles : MonoBehaviour {

    private ParticleSystem targetPart;
    private bool isEnabled;


    // Use this for initialization
    void Start () {
        isEnabled = false;
        targetPart = GetComponentInChildren<ParticleSystem>();
        targetPart.Play();
        TurnOff();
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void TurnOff()
    {
        targetPart.Stop();
    }

    public void TurnOn()
    {
        targetPart.Play();
    }
}
