using UnityEngine;
using System.Collections;

public class CamManager : MonoBehaviour
{
    Vector3 originalCameraPosition;

    public float DeathShakeMod = 0.05f;
    public float PunchShakeMod = 0.001f;
    public int PunchWaitTicks;
    public int OnDeathWaitTicks;

    private Transform clientTf;
    private UnityStandardAssets._2D.Camera2DFollow cam;
    private bool isClientFound;

    public Transform target;
    public float damping = 1;
    public float lookAheadFactor = 3;
    public float lookAheadReturnSpeed = 0.5f;
    public float lookAheadMoveThreshold = 0.1f;

    private float m_OffsetZ;
    private Vector3 m_LastTargetPosition;
    private Vector3 m_CurrentVelocity;
    private Vector3 m_LookAheadPos;

    private bool target_IsNew;

    private float x_offset;
    private float y_offset;
    private Vector3 offsetVector;
    private float maxDeathDistance;


    // Use this for initialization
    private void Start()
    {
        maxDeathDistance = 2000f;
        target_IsNew = false;
        if (target == null)
            return;
        m_LastTargetPosition = target.position;
        m_OffsetZ = (transform.position - target.position).z;
        transform.parent = null;
    }


    // Update is called once per frame
    private void Update()
    {
        if (target == null)
            return;
        
        // only update lookahead pos if accelerating or changed direction
        float xMoveDelta = (target.position - m_LastTargetPosition).x;

        bool updateLookAheadTarget = Mathf.Abs(xMoveDelta) > lookAheadMoveThreshold;

        if (updateLookAheadTarget)
        {
            m_LookAheadPos = lookAheadFactor * Vector3.right * Mathf.Sign(xMoveDelta);
        }
        else
        {
            m_LookAheadPos = Vector3.MoveTowards(m_LookAheadPos, Vector3.zero, Time.deltaTime * lookAheadReturnSpeed);
        }

        Vector3 aheadTargetPos = target.position + m_LookAheadPos + Vector3.forward * m_OffsetZ;
        Vector3 newPos = Vector3.SmoothDamp(transform.position, aheadTargetPos, ref m_CurrentVelocity, damping);

        offsetVector = new Vector3(x_offset, y_offset, 0f);
        transform.position = newPos + offsetVector;

        m_LastTargetPosition = target.position;
    }

    public static void SetTarget(Transform newTarget)
    {
        getRef()._SetTarget(newTarget);
    }

    public void _SetTarget(Transform newTarget)
    {
        target = newTarget;
        m_LastTargetPosition = target.position;
        m_OffsetZ = (transform.position - target.position).z;
        transform.parent = null;
        target_IsNew = true;
    }

    public bool HasNewTarget()
    {
        if (target_IsNew)
        {
            target_IsNew = false;
            return true;
        }
        return target_IsNew;
    }


    /// <summary>
    /// Amount of screenshake is variable on character distance to camera.
    /// Coded with assumption opponents are not dieing near you, and that
    /// distances greater than 1000 result in 0 screen shake and it's linear to
    /// that point.
    /// </summary>
    /// <param name="BodyVelocityMag">Moving force player is feeling.</param>
    /// <param name="playerLoc"> player location in WORLD space.</param>
    public static void DeathShake(bool isMyDeath)
    {
        getRef()._DeathShake(isMyDeath);
    }

    public void _DeathShake(bool isMyDeath)
    {
        float shakeAmt = DeathShakeMod * (isMyDeath ? 2f : 1f);
        StartCoroutine(cameraShake(OnDeathWaitTicks, shakeAmt));
    }


    //Called for Punches, only client 
    public static void PunchShake(float BodyVelocityMag)
    {
        getRef()._PunchShake(BodyVelocityMag);
    }

    public void _PunchShake(float BodyVelocityMag)
    {
        float shakeAmt = BodyVelocityMag * PunchShakeMod;
        StartCoroutine(cameraShake(PunchWaitTicks, shakeAmt));
    }

    private IEnumerator cameraShake(int ticks, float shakeAmt)
    {
        float x_totalAdded = 0f;
        float y_totalAdded = 0f;
        while(ticks > 0){
            ticks --;
            x_totalAdded = Random.Range(-1f, 1f) * shakeAmt;
            y_totalAdded = Random.Range(-1f, 1f) * shakeAmt;
            x_offset += x_totalAdded;
            y_offset += y_totalAdded;
            yield return null;
            x_offset -= x_totalAdded;
            y_offset -= y_totalAdded;
        }
    }

    void StopShaking()
    {
        CancelInvoke("CameraShake");
        transform.GetComponent<UnityStandardAssets._2D.Camera2DFollow>()
            .enabled = true;
    }

    private static CamManager getRef()
    {
        return GameObject.FindGameObjectWithTag("StageCamera")
            .GetComponent<CamManager>();
    }
}
