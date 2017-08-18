using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerController2DOnline : PlayerController2D 
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

    public int SlotNum;

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
    private float lastHitTime;
    private float lastHitForgetLength;

    public AudioClip DeathNoise;
    private AudioSource myAudioSrc;

    private bool isFrozen;
    private bool controlsPaused;
    private float spawnPause;
    private WaitForSeconds spawnPauseWait;

    private Animator anim;
    private ParticleSystem mainPartSys;
    private ParticleSystem deathParts;

    public float BoxPunch;


    void Awake() 
    {

        foreach (ParticleSystem partSys in GetComponentsInChildren<ParticleSystem>())
        {
            if (partSys.gameObject.name == "Main Particle System")
            {
                mainPartSys = partSys;
            }
            if (partSys.gameObject.name == "Death Particle System")
            {
                deathParts = partSys;
            }
        }
        mainPartSys.Stop();
        deathParts.Stop();

        isFrozen = false;
        //DontDestroyOnLoad(gameObject);
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
        AttackObjs[0] = transform.Find("PunchUp").gameObject;
        AttackObjs[1] = transform.Find("PunchForward").gameObject;
        AttackObjs[2] = transform.Find("PunchDown").gameObject;

        if (GameObject.FindGameObjectWithTag("MobileController") != null)
            _MobileInput = GameObject.FindGameObjectWithTag("MobileController").GetComponent<MobileController>();

        spawnPause = 0.5f;
        spawnPauseWait = new WaitForSeconds(spawnPause);

        lastHitBy = -1;
        lastHitTime = Time.time;
        lastHitForgetLength = 5;//Seconds

        if (_PhotonView.isMine) {
            tag = "PlayerSelf";
            _PhotonView.RPC("SetSlotNum", PhotonTargets.All, NetIDs.PlayerNumber(PhotonNetwork.player.ID));
            CamManager.SetTarget(transform);
        }
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

        if (isFrozen)
            return;
        
        updateJumping();
        updateDownJumping();    
        updateAttacks();
        updateMovement();
        UpdateHurt();
    }

    void FixedUpdate()
    {
        updateFacingDirection();
        updateIsGrounded();
        if(!_PhotonView.isMine)
            return;
        if (wasGrounded && isGrounded)
        {
            canDownJump = true;
        }
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

        //Only recent hits count
        if(Time.time - lastHitTime > lastHitForgetLength) {
            lastHitBy = -1;
            lastHitTime = Time.time;
        }
    }

    private void UpdateHurt()
    {
        if(invincibilityCount>=0)
            invincibilityCount--;
        //if (invincibilityCount == 0)
        //    TurnPartsOff();

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
            //CBUG.Log("Jumped is true!");
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
            //CBUG.Log("Move axis: " + tempAxisTouch);
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
            //CBUG.Log("DownJumped");
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
            //CBUG.Log("Jumped is false!");
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
        isGrounded = (hit.collider != null);
        //hit.collider.gameObject.layer
        ///Can't regain jumpcounts before jump force is applied.
        if (isGrounded && !jumped)
        {
            jumpsRemaining = TotalJumpsAllowed;
            if (hit.transform.tag.Contains("Player"))
                return;
            //CBUG.Log("Grounded on: " + (hit.collider.name));
            transform.SetParent(hit.collider.transform);
        }
        else if (!isGrounded)
        {
            transform.SetParent(null);
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
        if (isDead)
            return;

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
    void SetSlotNum(int SlotNUm)
    {
        this.SlotNum = SlotNUm;
        CBUG.Do("Recording ID " + SlotNUm + " with Gamemaster.");
        CBUG.Do("Character is: " + gameObject.name);
        GameManager.AddPlayer(SlotNUm, gameObject);
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
    void HurtAnim(int hurtNum)
    {
        switch (hurtNum)
        {
            case 1:
                anim.SetTrigger("HurtSmall");
                break;
            case 2:
                anim.SetTrigger("HurtMedium");
                break;
            case 3:
                anim.SetTrigger("HurtBig");
                break;
            default:
                CBUG.Error("BAD ANIM NUMBER GIVEN");
                break;
        }
    }


    /// <summary>
    /// Calls GameManager's RecordDeath. Respawn Handling
    /// is done from there.
    /// </summary>
    /// <param name="killer"></param>
    /// <param name="killed"></param>
    [PunRPC]
    private void OnDeath(int killer, int killed)
    {
        if (isDead)
            return;

        isDead = true;
        deathParts.Play();
        //Hide Self till respawn (or stay dead, ghost)
        transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Image>().enabled = false;

        //Freeze and Clear motion
        _Rigibody2D.velocity = Vector2.zero;
        velocity = Vector2.zero;

        //Death Map: OnDeath > RecordDeath > HandleDeath >
        // doRespawnOrGhost
        GameManager.RecordDeath(killer, killed, false);
        GameManager.HandleDeath(killed, false);

        //TODO: Animate death
    }
    #endregion
    
    /// <summary>
    /// Nothing to do. You stay invisible and immobile
    /// </summary>
    public override void Ghost()
    {
        transform.tag = "PlayerGhost";
    }

    /// <summary>
    /// Respawning. Only Graphical UNLESS is our Player. Then we reposition ourselves.
    /// </summary>
    /// <param name="spawnPoint"></param>
    public override void Respawn(Vector3 spawnPoint)
    {
        if (_PhotonView.isMine) {
            transform.position = spawnPoint;
            GameHUDController.LoseALife();
            GameHUDController.ResetDamage();
            damage = 0;
        }
        deathParts.Stop();
        _Rigibody2D.isKinematic = false;
        StartCoroutine(spawnProtection()); 
    }

    private IEnumerator spawnProtection()
    {
        yield return spawnPauseWait;
        //TODO: Spawn animation
        transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Image>().enabled = true;

        isDead = false;
    }

    public void Freeze()
    {
        _Rigibody2D.isKinematic = true;
        isFrozen= true;
    }
    public void UnFreeze()
    {
        _Rigibody2D.isKinematic = false;
        isFrozen = false;
    }

    public void TurnPartsOn()
    {
        mainPartSys.Play();
    }

    public void TurnPartsOff()
    {
        mainPartSys.Stop();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        //if (col.name == "Physics Box(Clone)")
        //{
        //    CBUG.Log("PUNCH");
        //    if(transform.localScale.x > 0){
        //        col.GetComponent<Rigidbody2D>().AddForce(new Vector2(BoxPunch, BoxPunch), ForceMode2D.Impulse);
        //    }else{
        //        col.GetComponent<Rigidbody2D>().AddForce(new Vector2(-BoxPunch, BoxPunch), ForceMode2D.Impulse);
        //    }
        //}

        if (isDead || col == null || !_PhotonView.isMine)
            return;


        //apply force . . .
        else if (col.name.Contains("Punch"))
        {
            //Get name of puncher
            lastHitBy = col.GetComponentInParent<PlayerController2DOnline>().SlotNum;
            lastHitTime = Time.time;

            if (invincibilityCount > 0)
            {
                return;
            }
            else
            {
                invincibilityCount = InvicibilityFrames;
                //TurnPartsOn();
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
            GameHUDController.SetDamageTo(damage);
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
        bool isOutsideWall = !col.IsTouching(GetComponentInChildren<Collider2D>());
        if (col.tag == "DeathWall" && isOutsideWall)
        {
            myAudioSrc.Play();
            CamManager.DeathShake(CamManager.GetTarget().name == gameObject.name);

            if (!_PhotonView.isMine)
                return;

            _PhotonView.RPC("OnDeath", PhotonTargets.All, lastHitBy, SlotNum);
        }

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