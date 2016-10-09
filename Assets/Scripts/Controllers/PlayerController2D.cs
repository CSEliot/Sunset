using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController2D : MonoBehaviour 
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

    public int ID;

    private Rigidbody2D _Rigibody2D;
    private PhotonView _PhotonView;
    private PhotonTransformView _PhotonTransform;
    private MobileController _MobileInput;

    private bool isGrounded;
    private bool wasGrounded;
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
    
    private float damage;
    private bool isDead;
    private int lastHitBy;

    private GameHUDController GameUI;

    public AudioClip DeathNoise;
    private AudioSource myAudioSrc;

    private bool controlsPaused;

    private Animator anim;

    public float BoxPunch;

    void Awake() 
    {
        anim = GetComponentInChildren<Animator>();
        moveRight = 0;
        moveLeft = 0;
        controlsPaused = false;
        myAudioSrc = GetComponent<AudioSource>();
        myAudioSrc.clip = DeathNoise;
        punching = false;

        isDead = false;
        StrengthsList = GameObject.FindGameObjectWithTag("Master").
            GetComponent<Master>().GetStrengthList();

        CBUG.Log("Str List: " + StrengthsList.ToStringFull());
        jumpForceTemp = 0f;
        SpeedTemp = 0f;
        attackDisableDelay = new WaitForSeconds(AttackLife);
        facingRight = true;
        position = new Vector2();
        _Rigibody2D = GetComponent<Rigidbody2D>();
        jumpsRemaining = TotalJumpsAllowed;
        _PhotonView = GetComponent<PhotonView>();
        _PhotonTransform = GetComponent<PhotonTransformView>();

        AttackObjs = new GameObject[3];
        AttackObjs[0] = transform.GetChild(3).gameObject;
        AttackObjs[1] = transform.GetChild(1).gameObject;
        AttackObjs[2] = transform.GetChild(2).gameObject;

        _MobileInput = GameObject.FindGameObjectWithTag("MobileController").GetComponent<MobileController>();

        if(_PhotonView.isMine)
            _PhotonView.RPC("SetID", PhotonTargets.All, PhotonNetwork.player.ID);
    }
 

    void Update() 
    {
        _PhotonTransform.SetSynchronizedValues(_Rigibody2D.velocity, 0f);
        if(!_PhotonView.isMine)
            return;

        //Jump Detection Only, no physics handling.
        if (controlsPaused)
        {
            moveLeft = 0;
            moveRight = 0;
            return;
        }

        updateSpecials();
        updateJumping();
        updateDownJumping();    
        updateAttacks();
        updateMovement();
        UpdateHurt();
    }

    void FixedUpdate()
    {
        updateFacingDirection();
        if(!_PhotonView.isMine)
            return;
        if (wasGrounded && isGrounded)
        {
            canDownJump = true;
        }
        updateIsGrounded();
        updateJumpingPhysics();
        updateDownJumpingPhysics();
        updateMovementPhysics();
        ////limit max velocity.
        ////CBUG.Log("R Velocity: " + m_Body.velocity.magnitude);
        ////if (m_Body.velocity.magnitude >= MaxVelocityMag)
        ////{
        ////}

        velocity += Vector2.down * GravityForce;
        if(!isDead)
            _Rigibody2D.velocity = velocity;
        velocity = Vector3.zero;
    }

    void LateUpdate()
    {
        if (!_PhotonView.isMine)
            return;
    }

    private void UpdateHurt()
    {
        if(invincibilityCount>=0)
            invincibilityCount--;
    }

    private void updateSpecials()
    {
        if (Input.GetButtonDown("Special") && invincibilityCount < 0)
        {
            _PhotonView.RPC("SpecialActivate", PhotonTargets.All);
            PauseMvmnt();
        }
    }

    private void updateFacingDirection()
    {
        if( !facingRight && _Rigibody2D.velocity.x > 0.2f )
        {
            facingRight = true;
            transform.localScale = new Vector3( 1, 1, 1 );
        }
        else if( facingRight && _Rigibody2D.velocity.x < -0.2f )
        {
            facingRight = false;
            transform.localScale = new Vector3( -1, 1, 1 );
        }
    }

    private void updateJumping()
    {
        if ((Input.GetButtonDown("Jump") == true
            || _MobileInput.GetButtonDown("Jump"))
            && jumpsRemaining > 0 && totalJumpFrames < 0)
        {
            jumped = true;
            CBUG.Log("Jumped is true!");
            jumpsRemaining -= 1;
            totalJumpFrames = jumpLag;
        }
        totalJumpFrames -= 1;
    }

    private void updateDownJumping()
    {
        if ((Input.GetButtonDown("DownJump") == true
            || _MobileInput.GetButtonDown("DownJump"))
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
        tempAxisTouch = _MobileInput.GetAxis("MoveHorizontal");
        if ( tempAxisKey > 0 || tempAxisTouch > 0)
        {
            CBUG.Log("Move axis: " + tempAxisTouch);
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
            CBUG.Log("DownJumped");
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
            CBUG.Log("Jumped is false!");
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
        else if(isGrounded)
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
        wasGrounded = isGrounded;
        isGrounded = hit.collider != null;
        //hit.collider.gameObject.layer
        ///Can't regain jumpcounts before jump force is applied.
        if (isGrounded && !jumped)
        {
            //CBUG.Log("Grounded on: " + (hit.collider.name));
            jumpsRemaining = TotalJumpsAllowed;
        }
        else if (isGrounded)
        {
            //CBUG.Log("Grounded on: " + (hit.collider.name));
        }
        else
        {
            //CBUG.Log("No Raycast Contact.");
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
            if ((Input.GetButtonDown("Up") || _MobileInput.GetButtonDown("Up")) && !punching)
            {
                punching = true;
                AttackObjs[0].SetActive(true);
                StartCoroutine(disableDelay(AttackObjs[0]));
                totalAttackFrames = AttackLag;
                _PhotonView.RPC("UpAttack", PhotonTargets.Others);
            }
            if ((Input.GetButtonDown("Down") || _MobileInput.GetButtonDown("Down")) && !punching)
            {
                punching = true;
                AttackObjs[2].SetActive(true);
                StartCoroutine(disableDelay(AttackObjs[2]));
                totalAttackFrames = AttackLag;
                _PhotonView.RPC("DownAttack", PhotonTargets.Others);
            }
            if (facingRight && (Input.GetButtonDown("Right") || _MobileInput.GetButtonDown("Right")) && !punching)
            {
                punching = true;
                AttackObjs[1].SetActive(true);
                StartCoroutine(disableDelay(AttackObjs[1]));
                totalAttackFrames = AttackLag;
                _PhotonView.RPC("ForwardAttack", PhotonTargets.Others);
            }
            if (!facingRight && (Input.GetButtonDown("Left") || _MobileInput.GetButtonDown("Left")) && !punching)
            {
                punching = true;
                AttackObjs[1].SetActive(true);
                StartCoroutine(disableDelay(AttackObjs[1]));
                totalAttackFrames = AttackLag;
                _PhotonView.RPC("ForwardAttack", PhotonTargets.Others);
            }
        }
        totalAttackFrames -= 1;
    }

    #region RPC Functions.
    //TODO: Determine allowed scope of RPC functions.
    [PunRPC]
    void SetID(int ID)
    {
        this.ID = ID;
        CBUG.Do("Recording ID " + ID + "with Gamemaster.");
        CBUG.Do("Character is: " + gameObject.name);
        GameManager.Players.Add(ID, gameObject);
    }

    [PunRPC]
    void UpAttack()
    {
        AttackObjs[0].SetActive(true);
        StartCoroutine(disableDelay(AttackObjs[0]));
    }

    [PunRPC]
    void ForwardAttack()
    {
        AttackObjs[1].SetActive(true);
        StartCoroutine(disableDelay(AttackObjs[1]));
    }

    [PunRPC]
    void DownAttack()
    {
        AttackObjs[2].SetActive(true);
        StartCoroutine(disableDelay(AttackObjs[2]));
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
                CBUG.Error("BAD ANIM NUMBER GIVEN");
                break;
        }
    }


    [PunRPC]
    private void OnDeath(int killer, int killed)
    {
        GameManager.RecordDeath(killer, killed);
        transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
        transform.tag = "PlayerGhost";
    }

    [PunRPC]
    void OnRespawn()
    {
        transform.tag = "PlayerSelf";
        transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
    }
    #endregion


    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.name == "Physics Box(Clone)")
        {
            CBUG.Log("PUNCH");
            if(transform.localScale.x > 0){
                col.GetComponent<Rigidbody2D>().AddForce(new Vector2(BoxPunch, BoxPunch), ForceMode2D.Impulse);
            }else{
                col.GetComponent<Rigidbody2D>().AddForce(new Vector2(-BoxPunch, BoxPunch), ForceMode2D.Impulse);
            }
        }

        if (col == null || !_PhotonView.isMine)
            return;


        //apply force . . .
        else if (col.name.Contains("Punch"))
        {
            //Get name of puncher
            lastHitBy = col.GetComponentInParent<PlayerController2D>().ID;
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
                _PhotonView.RPC("HurtAnim", PhotonTargets.All, 1);
            }
            else if (damage < 60)
            {
                _PhotonView.RPC("HurtAnim", PhotonTargets.All, 2);
            }
            else
            {
                _PhotonView.RPC("HurtAnim", PhotonTargets.All, 3);
            }
            GameUI.SetDamageTo(damage);
            if (col.name == "PunchForward")
            {
                //velocity += Vector2.right * PunchForceForward_Forward;
                if (col.transform.parent.localScale.x > 0)
                {
                    Vector2 temp = Vector2.right * (PunchForceForward_Forward + StrengthsList[col.transform.parent.name] - Defense);
                    temp += Vector2.up * (PunchForceForward_Up + StrengthsList[col.transform.parent.name] - Defense);
                    StartCoroutine(
                        applyPunchForce(temp * (damage/100f))
                    );
                }
                else
                {
                    Vector2 temp = Vector2.left * (PunchForceForward_Forward + StrengthsList[col.transform.parent.name] - Defense);
                    temp += Vector2.up * (PunchForceForward_Up + StrengthsList[col.transform.parent.name] - Defense);
                    StartCoroutine(
                        applyPunchForce(temp * (damage / 100f))
                    );
                }
            }
            else if (col.name == "PunchUp")
            {
                StartCoroutine(
                    applyPunchForce(
                        (Vector2.up * (PunchForceUp + StrengthsList[col.transform.parent.name] - Defense) * (damage / 100f))
                    )
                );
            }
            else
            {
                if (!isGrounded)
                {
                    StartCoroutine(
                        applyPunchForce(
                            (Vector2.down * (PunchForceDown + StrengthsList[col.transform.parent.name] - Defense) * (damage / 100f))
                        )
                    );
                }
                else
                {
                    StartCoroutine(
                        applyPunchForce(
                            (Vector2.up * (PunchForceDown + StrengthsList[col.transform.parent.name] - Defense) * (damage / 200f))
                        )
                    );
                }
            }
        }
    }

    private IEnumerator disableDelay(GameObject dis)
    {
        yield return attackDisableDelay;
        dis.SetActive(false);
        punching = false;
    }

    private IEnumerator applyPunchForce(Vector2 punchForce)
    {
        Vector2 tempPunchForce = punchForce;
        bool isTempForceLow = false;
        CamManager.PunchShake(tempPunchForce.magnitude);
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
        CamManager.DeathShake(_PhotonView.isMine);

        if (!_PhotonView.isMine)
            return;
        isDead = true;

        GameManager.RecordDeath(lastHitBy, ID);
        _PhotonView.RPC("OnDeath", PhotonTargets.Others, lastHitBy, ID);
        
        //Freeze and Clear motion
        _Rigibody2D.velocity = Vector2.zero;
        velocity = Vector2.zero;
    }	  	

    public void Ghost()
    {
        transform.tag = "PlayerGhost";
    }

    public void Respawn()
    {
        damage = 0;
        GameUI.ResetDamage();
        //transform.position = SpawnPoints[Random.Range(0, 6)].position;
        _Rigibody2D.isKinematic = false; 
        transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
        isDead = false;
    }

    public bool GetIsDead()
    {
        return isDead;
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
//public void CheckWon()
//{
//    if (m_PhotonView.isMine && !isDead)
//    {
//        BattleUI.Won();
//    }
//}