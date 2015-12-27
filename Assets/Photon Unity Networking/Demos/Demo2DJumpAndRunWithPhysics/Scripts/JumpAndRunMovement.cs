using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JumpAndRunMovement : MonoBehaviour 
{
    public float Defense;
    public float Speed;
    public float SpeedAccel = 0.5f;
    public float SpeedDecel = 0.5f;
    private float SpeedTemp;
    public float JumpForce;
    public float JumpDecel = 0.5f;
    private float jumpForceTemp;
    public float GravityForce;

    public float MaxVelocityMag;

    public LayerMask mask = -1;

    Rigidbody2D m_Body;
    PhotonView m_PhotonView;
    private PhotonTransformView m_PhotonTransform;

    private bool m_IsGrounded;
    private int jumpsRemaining;
    private bool jumped;
    public int TotalJumpsAllowed;
    public Vector2 JumpOffset;

    public float GroundCheckEndPoint;

    private Vector2 position;
    private Vector2 velocity;

    public int jumpLag;
    private int totalJumpFrames;

    private GameObject[] AttackObjs;
    public int AttackLag;
    private int totalAttackFrames;
    private WaitForSeconds attackDisableDelay;

    private bool facingRight;

    public float PunchForceUp;
    public float PunchForceForward_Forward;
    public float PunchForceForward_Up;
    public float PunchForceDown;
    public float PunchForceDecel;
    private float punchForceUpTemp;
    private float punchForceForward_ForwardTemp;
    private float punchForceForward_UpTemp;
    private float punchForceDownTemp;
    private Dictionary<string, float> StrengthsList;

    //private bool PunchUp;
    //private bool PunchLeft;
    //private bool PunchRight;
    //private bool PunchDown;

    private bool cameraFollowAssigned;


    void Awake() 
    {
        StrengthsList = GameObject.FindGameObjectWithTag("Master").
            GetComponent<Master>().GetStrengthList();
        jumpForceTemp = 0f;
        SpeedTemp = 0f;
        cameraFollowAssigned = false;
        attackDisableDelay = new WaitForSeconds(0.15f);
        facingRight = true;
        position = new Vector2();
        m_Body = GetComponent<Rigidbody2D>();
        jumpsRemaining = TotalJumpsAllowed;
        m_PhotonView = GetComponent<PhotonView>();
        m_PhotonTransform = GetComponent<PhotonTransformView>();

        AttackObjs = new GameObject[3];
        AttackObjs[0] = transform.GetChild(3).gameObject;
        AttackObjs[1] = transform.GetChild(1).gameObject;
        AttackObjs[2] = transform.GetChild(2).gameObject;
    }

    void Update() 
    {
        m_PhotonTransform.SetSynchronizedValues(m_Body.velocity, 0f);
        if(!m_PhotonView.isMine)
            return;

        //Jump Detection Only, no physics handling.
        UpdateJumping();
        UpdateAttacks();
        if (!cameraFollowAssigned)
            AssignCameraFollow();
    }

    void FixedUpdate()
    {
        UpdateIsGrounded();
        UpdateFacingDirection();
        if(!m_PhotonView.isMine)
            return;
        UpdateJumpingPhysics();
        UpdateMovementPhysics();
        //limit max velocity.
        //Debug.Log("R Velocity: " + m_Body.velocity.magnitude);
        if (m_Body.velocity.magnitude >= MaxVelocityMag)
        {
        }

        velocity += Vector2.down * GravityForce;
        m_Body.velocity = velocity;
        velocity = Vector3.zero;
    }

    void UpdateFacingDirection()
    {
        if( !facingRight && m_Body.velocity.x > 0.2f )
        {
            facingRight = true;
            transform.localScale = new Vector3( 1, 1, 1 );
        }
        else if( facingRight && m_Body.velocity.x < -0.2f )
        {
            facingRight = false;
            transform.localScale = new Vector3( -1, 1, 1 );
        }
    }

    void UpdateJumping()
    {
        if( Input.GetButtonDown("Jump") == true 
            && jumpsRemaining > 0 && totalJumpFrames < 0)
        {
            jumped = true;
            jumpsRemaining -= 1;
            totalJumpFrames = jumpLag;
        }
        totalJumpFrames -= 1;
    }

    void UpdateJumpingPhysics()
    {
        if (jumped)
        {
            jumpForceTemp = JumpForce;
            jumped = false;
        } 
        velocity.y += jumpForceTemp;
        jumpForceTemp = Mathf.Lerp(jumpForceTemp, 0f, JumpDecel);
    }

    void UpdateMovementPhysics()
    {
        //Normally we'd have any Input Handling get called from Update,
        //But dropped inputs aren't a problem with 'getaxis' since it's
        //Continuous and not a single frame like GetButtonDown
        if(Input.GetAxis("Horizontal") > 0){
            SpeedTemp = Mathf.Lerp(SpeedTemp, Speed, SpeedAccel);
        }
        else if (Input.GetAxis("Horizontal") < 0)
        {
            SpeedTemp = Mathf.Lerp(SpeedTemp, -Speed, SpeedAccel);
        }
        else
        {
            SpeedTemp = Mathf.Lerp(SpeedTemp, 0f, SpeedDecel);
        }
        velocity.x += SpeedTemp;
    }

    void UpdateIsGrounded()
    {
        Set2DPosition();

        RaycastHit2D hit = 
            Physics2D.Raycast(position+JumpOffset, 
                              -Vector2.up, 
                              GroundCheckEndPoint, 
                              mask.value);

            //Debug.DrawLine(position, 
            //               position - Vector2.up*GroundCheckEndPoint, 
            //               Color.red, 
            //               0.02f);
        m_IsGrounded = hit.collider != null;
        //hit.collider.gameObject.layer
        if (m_IsGrounded)
        {
            Debug.Log ("Grounded on: " + (hit.collider.name));
            jumpsRemaining = TotalJumpsAllowed;
        }
    }

    private void Set2DPosition()
    {
        position.x = transform.position.x;
        position.y = transform.position.y;
    }

    void UpdateAttacks()
    {
        if(totalAttackFrames < 0 ){
            if (Input.GetButtonDown("Up"))
            {
                AttackObjs[0].SetActive(true);
                StartCoroutine(DisableDelay(AttackObjs[0]));
                totalAttackFrames = AttackLag;
                m_PhotonView.RPC("UpAttack", PhotonTargets.Others);
            }
            if (Input.GetButtonDown("Down"))
            {
                AttackObjs[2].SetActive(true);
                StartCoroutine(DisableDelay(AttackObjs[2]));
                totalAttackFrames = AttackLag;
                m_PhotonView.RPC("DownAttack", PhotonTargets.Others);
            }
            if (facingRight && Input.GetButtonDown("Right"))
            {
                AttackObjs[1].SetActive(true);
                StartCoroutine(DisableDelay(AttackObjs[1]));
                totalAttackFrames = AttackLag;
                m_PhotonView.RPC("ForwardAttack", PhotonTargets.Others);
            }
            if (!facingRight && Input.GetButtonDown("Left"))
            {
                AttackObjs[1].SetActive(true);
                StartCoroutine(DisableDelay(AttackObjs[1]));
                totalAttackFrames = AttackLag;
                m_PhotonView.RPC("ForwardAttack", PhotonTargets.Others);
            }
        }
        totalAttackFrames -= 1;
    }


    [PunRPC]
    void UpAttack()
    {
        AttackObjs[0].SetActive(true);
        StartCoroutine(DisableDelay(AttackObjs[0]));
    }

    [PunRPC]
    void ForwardAttack()
    {
        AttackObjs[1].SetActive(true);
        StartCoroutine(DisableDelay(AttackObjs[1]));
    }

    [PunRPC]
    void DownAttack()
    {
        AttackObjs[2].SetActive(true);
        StartCoroutine(DisableDelay(AttackObjs[2]));
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col == null)
            return;

        if (col != null)
        {
            Debug.Log("I collided with: " + col.transform.parent.name);
            //Debug.Log("And I am: " + gameObject.name);
        }


        //apply force . . .
        if (col.name.Contains("Punch"))
        {
            if (col.name == "PunchForward")
            {
                //velocity += Vector2.right * PunchForceForward_Forward;
                if (col.transform.parent.localScale.x > 0)
                {
                    Vector2 temp = Vector2.right * (PunchForceForward_Forward + StrengthsList[col.transform.parent.name + "(Clone)"] - Defense);
                    temp += Vector2.up * (PunchForceForward_Up + StrengthsList[col.transform.parent.name + "(Clone)"] - Defense);
                    StartCoroutine(
                        ApplyPunchForce(temp)
                    );
                }
                else
                {
                    Vector2 temp = Vector2.left * (PunchForceForward_Forward + StrengthsList[col.transform.parent.name+"(Clone)"] - Defense);
                    temp += Vector2.up * (PunchForceForward_Up + StrengthsList[col.transform.parent.name+"(Clone)"] - Defense);
                    StartCoroutine(
                        ApplyPunchForce(temp)
                    );
                }
            }
            else if (col.name == "PunchUp")
            {
                StartCoroutine(
                    ApplyPunchForce(
                        (Vector2.up * (PunchForceUp + StrengthsList[col.transform.parent.name+"(Clone)"] - Defense))
                    )
                );
            }
            else
            {
                StartCoroutine(
                    ApplyPunchForce(
                        (Vector2.down * (PunchForceDown + StrengthsList[col.transform.parent.name+"(Clone)"] - Defense))
                    )
                );
            }
        }
    }

    IEnumerator DisableDelay(GameObject dis)
    {
        yield return attackDisableDelay;
        dis.SetActive(false);
    }

    private void AssignCameraFollow()
    {
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<UnityStandardAssets._2D.Camera2DFollow>()
            .SetTarget(transform);  
        cameraFollowAssigned = true;
    }

    IEnumerator ApplyPunchForce(Vector2 punchForce)
    {
        Vector2 tempPunchForce = punchForce;
        while (tempPunchForce.magnitude > 0.01f)
        {
            velocity += tempPunchForce;
            tempPunchForce = Vector2.Lerp(tempPunchForce, Vector2.zero, PunchForceDecel);
            yield return null;
        }
    }
}
