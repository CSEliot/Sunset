using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JumpAndRunMovementOffline : MonoBehaviour 
{
    public float Defense;
    public float Speed;
    public float SpeedAccel = 0.5f;
    public float AirSpeedDecel;
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
    private MobileController Input_M;

    private bool m_IsGrounded;
    private bool m_wasGrounded;
    private int jumpsRemaining;
    private bool jumped;
    private bool canDownJump;
    private bool downJumped;
    public int TotalJumpsAllowed;
    public Vector2 JumpOffset;
    float tempAxisKey;
    float tempAxisTouch;

    private float moveLeft;
    private float moveRight;

    public float GroundCheckEndPoint;

    private Vector2 position;
    private Vector2 velocity;
    
    public int jumpLag;
    private int totalJumpFrames;

    private GameObject[] AttackObjs;
    public int AttackLag;
    public float AttackLife;
    private int totalAttackFrames;
    private WaitForSeconds attackDisableDelay;
    public float InvicibilityFrames;
    private float invincibilityCount;

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
	private bool punchForceApplied;
    public float PunchDisablePerc;

    //private bool PunchUp;
    //private bool PunchLeft;
    //private bool PunchRight;
    //private bool PunchDown;

    private bool cameraFollowAssigned;
    private bool battleUIAssigned;
    private CamManager camShaker;

    private float damage;
    private bool isDead;

    private LifeMaster BattleUI;

    private Transform[] SpawnPoints;

    private Master m;

    private bool playersSpawned;
    
    private bool readyGUIFound;

    public AudioClip DeathNoise;
    private AudioSource myAudioSrc;

    private bool controlsPaused;

    private Animator anim;

    public float BoxPunch;

    public bool IsDummy;
    public int DummySpawn;

    void Awake() 
    {
        anim = GetComponentInChildren<Animator>();
        moveRight = 0;
        moveLeft = 0;
        controlsPaused = false;
        myAudioSrc = GetComponent<AudioSource>();
        myAudioSrc.clip = DeathNoise;
        readyGUIFound = false;
        playersSpawned = false;
        punching = false;
        camShaker = GameObject.FindGameObjectWithTag("MainCamera")
            .GetComponent<CamManager>();
        m = GameObject.FindGameObjectWithTag("Master")
            .GetComponent<Master>();

        isDead = false;
        battleUIAssigned = false;
        SpawnPoints = GameObject.FindGameObjectWithTag
            ("SpawnPoints").GetComponent<SpawnOfflineTest>().SpawnList;
        StrengthsList = GameObject.FindGameObjectWithTag("Master").
            GetComponent<Master>().GetStrengthList();

        Debug.Log("Str List: " + StrengthsList.ToStringFull());
        jumpForceTemp = 0f;
        SpeedTemp = 0f;
        cameraFollowAssigned = false;
        attackDisableDelay = new WaitForSeconds(AttackLife);
        facingRight = true;
        position = new Vector2();
        m_Body = GetComponent<Rigidbody2D>();
        jumpsRemaining = TotalJumpsAllowed;

        AttackObjs = new GameObject[3];
        AttackObjs[0] = transform.GetChild(3).gameObject;
        AttackObjs[1] = transform.GetChild(1).gameObject;
        AttackObjs[2] = transform.GetChild(2).gameObject;

        Input_M = GameObject.FindGameObjectWithTag("MobileController").GetComponent<MobileController>();
    }

    [PunRPC]
    void AssignPlayerTag(int ID)
    {
        transform.GetChild(0).tag = "" + ID;
    }

    void Update() 
    {
        //OFFLINE MODE <<FOR NOW>> ASSUMES SINGLEPLAYER ONLY

        if (!readyGUIFound)
        {
            readyGUIFound = true;
        }
        if (!cameraFollowAssigned)
            AssignCameraFollow(transform);
        //if (!battleUIAssigned){
        //    BattleUI = GameObject.FindGameObjectWithTag("BattleUI")
        //        .GetComponent<LifeMaster>();
        //    battleUIAssigned = true;
        //}
        

        //Jump Detection Only, no physics handling.
        if (controlsPaused)
        {
            moveLeft = 0;
            moveRight = 0;
            return;
        }

        updateSpecials();
        if (!IsDummy)
        {
            updateJumping();
            updateDownJumping();    
        }
        updateAttacks();
        if(!IsDummy)
            updateMovement();
        UpdateHurt();
    }

    void FixedUpdate()
    {
        updateFacingDirection();

        if (m_wasGrounded && m_IsGrounded)
        {
            canDownJump = true;
        }
        updateIsGrounded();
        updateJumpingPhysics();
        updateDownJumpingPhysics();
        updateMovementPhysics();
        ////limit max velocity.
        ////Debug.Log("R Velocity: " + m_Body.velocity.magnitude);
        ////if (m_Body.velocity.magnitude >= MaxVelocityMag)
        ////{
        ////}

        velocity += Vector2.down * GravityForce;
        if(!isDead)
            m_Body.velocity = velocity;
        velocity = Vector3.zero;
    }

    void LateUpdate()
    {
        if (playersSpawned)
        {
            if (GameObject.FindGameObjectsWithTag("PlayerSelf").Length <= 1)
            {
                if (!isDead)
                {
                    BattleUI.Won();
                    StartCoroutine(WinWait());
                }
                else
                {
                    StartCoroutine(LoseWait());
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

    private void UpdateHurt()
    {
        if(invincibilityCount>=0)
            invincibilityCount--;
    }

    IEnumerator WinWait()
    {
        yield return new WaitForSeconds(2f);
    }

    IEnumerator LoseWait()
    {
        yield return new WaitForSeconds(1.9f);
    }

    //private void 

    private void updateSpecials()
    {
        if (Input.GetButtonDown("Special") && invincibilityCount < 0)
        {
            PauseMvmnt();
        }
    }

    private void updateFacingDirection()
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

    private void updateJumping()
    {
        if ((Input.GetButtonDown("Jump") == true
            || Input_M.GetButtonDown("Jump"))
            && jumpsRemaining > 0 && totalJumpFrames < 0)
        {
            jumped = true;
            Debug.Log("Jumped is true!");
            jumpsRemaining -= 1;
            totalJumpFrames = jumpLag;
        }
        totalJumpFrames -= 1;
    }

    private void updateDownJumping()
    {
        if ((Input.GetButtonDown("DownJump") == true
            || Input_M.GetButtonDown("DownJump"))
            && canDownJump)
        {
            downJumped = true;
            canDownJump = false;
        }
    }

    private void updateMovement()
    {
        //tempAxis left n right, keyboar axis left n right, or no input
        tempAxisKey = Input.GetAxis("MoveHorizontal");
        tempAxisTouch = Input_M.GetAxis("MoveHorizontal");
        if ( tempAxisKey > 0 || tempAxisTouch > 0)
        {
            moveLeft = 0;
            moveRight = tempAxisTouch > tempAxisKey ? tempAxisTouch : tempAxisKey;
        }
        else if ( tempAxisKey < 0 || tempAxisTouch < 0)
        {
            moveLeft = tempAxisTouch < tempAxisKey ? tempAxisTouch : tempAxisKey;
            moveRight = 0;
        }
        else
        {
            moveLeft = 0;
            moveRight = 0; 
        }
    }

    private void updateDownJumpingPhysics()
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

    private void updateJumpingPhysics()
    {
        if (jumped)
        {
            jumpForceTemp = JumpForce;
            jumped = false;
            Debug.Log("Jumped is false!");
        } 
        velocity.y += jumpForceTemp;
        jumpForceTemp = Mathf.Lerp(jumpForceTemp, 0f, JumpDecel);
    }


    private void updateMovementPhysics()
    {
        //Normally we'd have any Input Handling get called from Update,
        //But dropped inputs aren't a problem with 'getaxis' since it's
        //Continuous and not a single frame like GetButtonDown
        if(moveRight != 0){
            SpeedTemp = Mathf.Lerp(SpeedTemp, Speed*moveRight, SpeedAccel);
        }
        else if (moveLeft != 0)
        {
            SpeedTemp = Mathf.Lerp(SpeedTemp, Speed*moveLeft, SpeedAccel);
        }
        else if(m_IsGrounded)
        {
            SpeedTemp = Mathf.Lerp(SpeedTemp, 0f, SpeedDecel);
        }
        else
        {
            SpeedTemp = Mathf.Lerp(SpeedTemp, 0f, AirSpeedDecel);
        }
		if(!punchForceApplied)
        	velocity.x += SpeedTemp;
    }

    private void updateIsGrounded()
    {
        set2DPosition();

        RaycastHit2D hit = 
            Physics2D.Raycast(position+JumpOffset, 
                              -Vector2.up, 
                              GroundCheckEndPoint, 
                              mask.value);

        //Debug.DrawLine(position+JumpOffset,
        //               position + (-Vector2.up * GroundCheckEndPoint),
        //               Color.red,
        //               0.01f);
        m_wasGrounded = m_IsGrounded;
        m_IsGrounded = hit.collider != null;
        //hit.collider.gameObject.layer
        ///Can't regain jumpcounts before jump force is applied.
        if (m_IsGrounded && !jumped)
        {
            //Debug.Log("Grounded on: " + (hit.collider.name));
            jumpsRemaining = TotalJumpsAllowed;
        }
        else if (m_IsGrounded)
        {
            //Debug.Log("Grounded on: " + (hit.collider.name));
        }
        else
        {
            //Debug.Log("No Raycast Contact.");
        }
        //if(!m_IsGrounded && down)
    }

    private void set2DPosition()
    {
        position.x = transform.position.x;
        position.y = transform.position.y;
    }

    private void updateAttacks()
    {
        if(totalAttackFrames < 0 ){
            if ((Input.GetButtonDown("Up") || Input_M.GetButtonDown("Up")) && !punching)
            {
                punching = true;
                AttackObjs[0].SetActive(true);
                StartCoroutine(DisableDelay(AttackObjs[0]));
                totalAttackFrames = AttackLag;
            }
            if ((Input.GetButtonDown("Down") || Input_M.GetButtonDown("Down")) && !punching)
            {
                punching = true;
                AttackObjs[2].SetActive(true);
                StartCoroutine(DisableDelay(AttackObjs[2]));
                totalAttackFrames = AttackLag;
            }
            if (facingRight && (Input.GetButtonDown("Right") || Input_M.GetButtonDown("Right")) && !punching)
            {
                punching = true;
                AttackObjs[1].SetActive(true);
                StartCoroutine(DisableDelay(AttackObjs[1]));
                totalAttackFrames = AttackLag;
            }
            if (!facingRight && (Input.GetButtonDown("Left") || Input_M.GetButtonDown("Left")) && !punching)
            {
                punching = true;
                AttackObjs[1].SetActive(true);
                StartCoroutine(DisableDelay(AttackObjs[1]));
                totalAttackFrames = AttackLag;
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

    [PunRPC]
    void SpecialActivate()
    {
        anim.SetBool("Activating", true);
    }

    [PunRPC]
    void HurtAnim(int hurtNum)
    {
        switch (hurtNum)
        {
            case 1:
                anim.SetBool("HurtSmall", true);
                break;
            case 2:
                anim.SetBool("HurtMedium", true);
                break;
            case 3:
                anim.SetBool("HurtBig", true);
                break;
            default:
                Debug.LogError("BAD ANIM NUMBER GIVEN");
                break;
        }
    }


    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.name == "Physics Box(Clone)")
        {
            Debug.Log("PUNCH");
            if(transform.localScale.x > 0){
                col.GetComponent<Rigidbody2D>().AddForce(new Vector2(BoxPunch, BoxPunch), ForceMode2D.Impulse);
            }else{
                col.GetComponent<Rigidbody2D>().AddForce(new Vector2(-BoxPunch, BoxPunch), ForceMode2D.Impulse);
            }
        }

        if (col == null)
            return;


        //apply force . . .
        else if (col.name.Contains("Punch"))
        {
            if (invincibilityCount > 0)
            {
                return;
            }
            else
            {
                invincibilityCount = InvicibilityFrames;
            }

            damage += PunchPercentAdd;

            if (damage < 30)
            {
                HurtAnim(1);
            }
            else if (damage < 60)
            {
                HurtAnim(2);
            }
            else
            {
                HurtAnim(3);
            }
            //BattleUI.SetDamageTo(damage);
            if (col.name.Contains("PunchForward"))
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
        camShaker.SetTarget(myTransform);  
        cameraFollowAssigned = true;
    }

    IEnumerator ApplyPunchForce(Vector2 punchForce)
    {
        Vector2 tempPunchForce = punchForce;
        bool isTempForceLow = false;
        camShaker.PunchShake(tempPunchForce.magnitude);
        while (tempPunchForce.magnitude > 0.01f)
        {
            velocity += tempPunchForce;
            tempPunchForce = Vector2.Lerp(tempPunchForce, Vector2.zero, PunchForceDecel);
            
            if (!isTempForceLow &&
                tempPunchForce.magnitude < punchForce.magnitude * PunchDisablePerc)
            {
                isTempForceLow = true;
                //if the force goes below 25%, let the character move again. 
	        	punchForceApplied = false;
            }
            else if(!isTempForceLow)
            {
			    punchForceApplied = true;
            }
            yield return null;
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.tag != "DeathWall")
            return;
        myAudioSrc.Play();
        camShaker.DeathShake(true);


        StartCoroutine(respawn());
        
    }	  	

    IEnumerator Ghost()
    {
        BattleUI.Lost();
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
        isDead = true;
        m_Body.velocity = Vector2.zero;
        velocity = Vector2.zero;
        m_Body.isKinematic = true;
        transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
        yield return new WaitForSeconds(3f);
        damage = 0;
        //BattleUI.ResetDamage();
        transform.position = SpawnPoints[0].position;
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
        if (!isDead)
        {
            BattleUI.Won();
        }
    }

    public void PauseMvmnt()
    {
        controlsPaused = true;
    }

    public void UnpauseMvmnt()
    {
        controlsPaused = false;
    }
}
