using UnityEngine;
using System.Collections;

public class CamShakeSimple : MonoBehaviour
{

    Vector3 originalCameraPosition;

    private float shakeAmt;
    public float ShakeMod;
    public float ShakeTime;

	private Transform clientTf;
	private UnityStandardAssets._2D.Camera2DFollow cam;
	private bool isClientFound;

	void Start(){
		isClientFound = false;	
		cam = GetComponent<UnityStandardAssets._2D.Camera2DFollow> ();
	}

	void Update(){
		if (!isClientFound && cam.target != null) {
			clientTf = cam.target;
			isClientFound = true;
		}
	}


    public void BeginDeathShake(float BodyVelocityMag, bool isClient)
    {
        float timeDiv = isClient ? 1 : 3;
		BodyVelocityMag = isClient ? BodyVelocityMag : BodyVelocityMag * 1/timeDiv;
        gameObject.GetComponent<UnityStandardAssets._2D.Camera2DFollow>().enabled = false;
        shakeAmt = BodyVelocityMag * ShakeMod;
        InvokeRepeating("CameraShake", 0, .01f);
        Invoke("StopShaking", ShakeTime/timeDiv);
    }

	//Called for Punches
    public void BeginPunchShake(float BodyVelocityMag, float time)
    {
        gameObject.GetComponent<UnityStandardAssets._2D.Camera2DFollow>() .enabled = false;
        shakeAmt = BodyVelocityMag * ShakeMod;
        InvokeRepeating("CameraShake", 0, .01f);
        Invoke("StopShaking", time / 3f);
    }

    void CameraShake()
    {
        if (shakeAmt > 0)
        {
            float quakeAmt = Random.value * shakeAmt * 2 - shakeAmt;
			Vector3 pp = cam.target.position;
            pp.y += quakeAmt;
            quakeAmt = Random.value * shakeAmt * 2 - shakeAmt;
            pp.x += quakeAmt;
            transform.position = pp;
        }
    }

    void StopShaking()
    {
        CancelInvoke("CameraShake");
        transform.GetComponent<UnityStandardAssets._2D.Camera2DFollow>()
            .enabled = true;
    }

}