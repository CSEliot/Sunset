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
    public float DownJumpForce;
    public float JumpDecel = 0.5f;
    public float DownJumpDecel = 0.5f;
    private float jumpForceTemp;
    private float downJumpForceTemp;
    public float GravityForce;

    public float MaxVelocityMag;

    public LayerMask mask = -1;

    Rigidbody2D m_Body;
    PhotonView m_PhotonView;
    private PhotonTransformView m_PhotonTransform;

    private bool m_IsGrounded;
    private int jumpsRemaining;
    private bool jumped;
    private bool canDownJump;
    private bool downJumped;
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

    private bool punching;
    public int PunchPercentAdd;
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
    private bool battleUIAssigned;
    private CamShakeSimple camShaker;

    private float damage;
    private bool isDead;

    private DamageTracker BattleUI;

    private Transform[] SpawnPoints;

    private Master m;

    private bool playersSpawned;

    private ReadyUp readyGUI;
    private bool readyGUIDisabled;

    void Awake() 
    {
        readyGUIDisabled = false;
        playersSpawned = false;
        punching = false;
        camShaker = GameObject.FindGameObjectWithTag("MainCamera")
            .GetComponent<CamShakeSimple>();
        m = GameObject.FindGameObjectWithTag("Master")
            .GetComponent<Master>();

        isDead = false;
        battleUIAssigned = false;
        SpawnPoints = GameObject.FindGameObjectWithTag
            ("SpawnPoints").GetComponent<OnJoinedInstantiate>().SpawnPosition;
        StrengthsList = GameObject.FindGameObjectWithTag("Master").
            GetComponent<Master>().GetStrengthList();

        Debug.Log("Str List: " + StrengthsList.ToStringFull());
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

        if (!readyGUIDisabled)
        {
            readyGUI = GameObject.FindGameObjectWithTag("ReadyOBJ")
                .GetComponent<ReadyUp>();
            readyGUI.transform.gameObject.SetActive(false);
        }
        if (!cameraFollowAssigned)
            AssignCameraFollow(transform);
        if (!battleUIAssigned){
            BattleUI = GameObject.FindGameObjectWithTag("BattleUI")
                .GetComponent<DamageTracker>();
            battleUIAssigned = true;
        }
        

        //Jump Detection Only, no physics handling.
        UpdateJumping();
        UpdateDownJumping();    
        UpdateAttacks();
    }

    void FixedUpdate()
    {
        UpdateIsGrounded();
        UpdateFacingDirection();
        if(!m_PhotonView.isMine)
            return;
        UpdateJumpingPhysics();
        UpdateDownJumpingPhysics();
        UpdateMovementPhysics();
        //limit max velocity.
        //Debug.Log("R Velocity: " + m_Body.velocity.magnitude);
        if (m_Body.velocity.magnitude >= MaxVelocityMag)
        {
        }

        velocity += Vector2.down * GravityForce;
        if(!isDead)
            m_Body.velocity = velocity;
        velocity = Vector3.zero;
    }

    void LateUpdate()
    {
        if (!m_PhotonView.isMine)
            return;
        if (playersSpawned)
        {
            if (GameObject.FindGameObjectsWithTag("PlayerSelf").Length <= 1)
            {
                if (!isDead)
                {
                    BattleUI.Won();
                    readyGUI.EndGame();
                }
            }
        }
        else
        {
            if (GameObject.FindGameObjectsWithTag("PlayerSelf").Length > 1)
            {
                playersSpawned  = true;
            }
        }
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
            canDownJump = true;
        }
        totalJumpFrames -= 1;
    }

    void UpdateDownJumping()
    {
        if (Input.GetButtonDown("DownJump") == true
            && canDownJump)
        {
            downJumped = true;
            canDownJump = false;
        }
    }

    void UpdateDownJumpingPhysics()
    {
        if (downJumped)
        {
            Debug.Log("DownJumped");
            jumpForceTemp = DownJumpForce;
            downJumped = false;
        }
        velocity.y -= downJumpForceTemp;
        downJumpForceTemp = Mathf.Lerp(downJumpForceTemp, 0f, DownJumpDecel);
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

        Debug.DrawLine(position+JumpOffset,
                       position + (-Vector2.up * GroundCheckEndPoint),
                       Color.red,
                       0.01f);

        m_IsGrounded = hit.collider != null;
        //hit.collider.gameObject.layer
        if (m_IsGrounded && !jumped)
        {
            //Debug.Log ("Grounded on: " + (hit.collider.name));
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
            if (Input.GetButtonDown("Up") && !punching)
            {
                punching = true;
                AttackObjs[0].SetActive(true);
                StartCoroutine(DisableDelay(AttackObjs[0]));
                totalAttackFrames = AttackLag;
                m_PhotonView.RPC("UpAttack", PhotonTargets.Others);
            }
            if (Input.GetButtonDown("Down") && !punching)
            {
                punching = true;
                AttackObjs[2].SetActive(true);
                StartCoroutine(DisableDelay(AttackObjs[2]));
                totalAttackFrames = AttackLag;
                m_PhotonView.RPC("DownAttack", PhotonTargets.Others);
            }
            if (facingRight && Input.GetButtonDown("Right") && !punching)
            {
                punching = true;
                AttackObjs[1].SetActive(true);
                StartCoroutine(DisableDelay(AttackObjs[1]));
                totalAttackFrames = AttackLag;
                m_PhotonView.RPC("ForwardAttack", PhotonTargets.Others);
            }
            if (!facingRight && Input.GetButtonDown("Left") && !punching)
            {
                punching = true;
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
        if (col == null || !m_PhotonView.isMine)
            return;

        if (col != null)
        {
            Debug.Log("I collided with: " + col.transform.parent.name);
            //Debug.Log("And I am: " + gameObject.name);
        }


        //apply force . . .
        if (col.name.Contains("Punch"))
        {
            damage += PunchPercentAdd;
            BattleUI.SetDamageTo(damage);
            if (col.name == "PunchForward")
            {
                //velocity += Vector2.right * PunchForceForward_Forward;
                if (col.transform.parent.localScale.x > 0)
                {
                    Vector2 temp = Vector2.right * (PunchForceForward_Forward + StrengthsList[col.transform.parent.name] - Defense);
                    temp += Vector2.up * (PunchForceForward_Up + StrengthsList[col.transform.parent.name] - Defense);
                    StartCoroutine(
                        ApplyPunchForce(temp * (damage/100f))
                    );
                }
                else
                {
                    Vector2 temp = Vector2.left * (PunchForceForward_Forward + StrengthsList[col.transform.parent.name] - Defense);
                    temp += Vector2.up * (PunchForceForward_Up + StrengthsList[col.transform.parent.name] - Defense);
                    StartCoroutine(
                        ApplyPunchForce(temp * (damage / 100f))
                    );
                }
            }
            else if (col.name == "PunchUp")
            {
                StartCoroutine(
                    ApplyPunchForce(
                        (Vector2.up * (PunchForceUp + StrengthsList[col.transform.parent.name] - Defense) * (damage / 100f))
                    )
                );
            }
            else
            {
                if (!m_IsGrounded)
                {
                    StartCoroutine(
                        ApplyPunchForce(
                            (Vector2.down * (PunchForceDown + StrengthsList[col.transform.parent.name] - Defense) * (damage / 100f))
                        )
                    );
                }
                else
                {
                    StartCoroutine(
                        ApplyPunchForce(
                            (Vector2.up * (PunchForceDown + StrengthsList[col.transform.parent.name] - Defense) * (damage / 200f))
                        )
                    );
                }
            }
        }
    }

    IEnumerator DisableDelay(GameObject dis)
    {
        yield return attackDisableDelay;
        dis.SetActive(false);
        punching = false;
    }

    private void AssignCameraFollow(Transform myTransform)
    {
        if (myTransform == null)
        {
            return;
        }
        GameObject.FindGameObjectWithTag("MainCamera").GetComponent<UnityStandardAssets._2D.Camera2DFollow>()
            .SetTarget(myTransform);  
        cameraFollowAssigned = true;
    }

    IEnumerator ApplyPunchForce(Vector2 punchForce)
    {
        Vector2 tempPunchForce = punchForce;
        camShaker.BeginShake(punchForce.magnitude, m_PhotonView.isMine, 1);
        while (tempPunchForce.magnitude > 0.01f)
        {
            velocity += tempPunchForce;
            tempPunchForce = Vector2.Lerp(tempPunchForce, Vector2.zero, PunchForceDecel);
            yield return null;
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.tag != "DeathWall")
            return;
        camShaker.BeginShake(m_Body.velocity.magnitude, m_PhotonView.isMine);

        if(!m_PhotonView.isMine)
            return;

        if (BattleUI.GetLives() > 0){
            BattleUI.LoseALife();
            StartCoroutine(respawn());
        }
        else
        {
            StartCoroutine(Ghost());
        }
    }

    IEnumerator Ghost()
    {
        BattleUI.Lost();
        m_PhotonView.RPC("OnGhost", PhotonTargets.Others);
        isDead = true;
        m_Body.velocity = Vector2.zero;
        velocity = Vector2.zero;
        m_Body.isKinematic = true;
        transform.tag = "PlayerGhost";
        transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
        yield return new WaitForSeconds(3f);
        if (GameObject.FindGameObjectWithTag("PlayerSelf") != null)
            AssignCameraFollow(GameObject.FindGameObjectWithTag("PlayerSelf").transform);
    }

    IEnumerator respawn()
    {
        m_PhotonView.RPC("OnDeath", PhotonTargets.Others);
        isDead = true;
        m_Body.velocity = Vector2.zero;
        velocity = Vector2.zero;
        m_Body.isKinematic = true;
        transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
        yield return new WaitForSeconds(3f);
        damage = 0;
        BattleUI.ResetDamage();
        transform.position = SpawnPoints[Random.Range(0, 6)].position;
        m_Body.isKinematic = false; 
        transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
        isDead = false;
    }

    [PunRPC]
    IEnumerator OnDeath()
    {
        transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
        yield return new WaitForSeconds(3f);
        transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
    }

    [PunRPC]
    void OnGhost()
    {
        transform.tag = "PlayerGhost";
        transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
    }

    public bool GetIsDead()
    {
        return isDead;
    }

    public void CheckWon()
    {
        if (m_PhotonView.isMine && !isDead)
        {
            BattleUI.Won();
        }
    }
}
