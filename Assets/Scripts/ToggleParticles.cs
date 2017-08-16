using UnityEngine;
using System.Collections;

public class ToggleParticles : MonoBehaviour {

    public ParticleSystem targetPart;
    private bool isEnabled;


    // Use this for initialization
    void Start () {
        isEnabled = false;
        if(GetComponentInChildren<ParticleSystem>() != null)
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
