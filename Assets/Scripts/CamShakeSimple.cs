using UnityEngine;
using System.Collections;

public class CamShakeSimple : MonoBehaviour
{

    Vector3 originalCameraPosition;

    private float shakeAmt;
    public float ShakeMod;
    public float ShakeTime;

    public void BeginShake(float BodyVelocityMag, bool isClient)
    {
        float timeDiv = isClient ? 3 : 1;
        BodyVelocityMag = isClient ? BodyVelocityMag : BodyVelocityMag * 0.9f;
        gameObject.GetComponent<UnityStandardAssets._2D.Camera2DFollow>().enabled = false;
        originalCameraPosition = transform.position;
        shakeAmt = BodyVelocityMag * ShakeMod;
        InvokeRepeating("CameraShake", 0, .01f);
        Invoke("StopShaking", ShakeTime/timeDiv);
    }

    public void BeginShake(float BodyVelocityMag, bool isClient, float time)
    {
        float timeDiv = isClient ? 3 : 1;
        BodyVelocityMag = isClient ? BodyVelocityMag : BodyVelocityMag * 0.9f;
        gameObject.GetComponent<UnityStandardAssets._2D.Camera2DFollow>().enabled = false;
        originalCameraPosition = transform.position;
        shakeAmt = BodyVelocityMag * ShakeMod;
        InvokeRepeating("CameraShake", 0, .01f);
        Invoke("StopShaking", time / timeDiv);
    }

    void CameraShake()
    {
        if (shakeAmt > 0)
        {
            float quakeAmt = Random.value * shakeAmt * 2 - shakeAmt;
            Vector3 pp = transform.position;
            pp.y += quakeAmt; // can also add to x and/or z
            quakeAmt = Random.value * shakeAmt * 2 - shakeAmt;
            pp.x += quakeAmt;
            transform.position = pp;
        }
    }

    void StopShaking()
    {
        CancelInvoke("CameraShake");
        transform.position = originalCameraPosition;
        transform.GetComponent<UnityStandardAssets._2D.Camera2DFollow>().enabled = true;
    }

}