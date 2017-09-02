using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerController2DOffline : PlayerController2D
{
    public float Defense;
    public float Speed;
    public float SpeedAccel = 0.5f;
    public float AirSpeedDecel;
    public float SpeedDecel = 0.5f;
    private float speedTemp;
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
    private GameInputController gameInput;

    private bool isGrounded;
    private bool wasGrounded;
    private int jumpsRemaining;
    private bool jumped;
    private bool canDownJump;
    private bool downJumped;
    public int TotalJumpsAllowed;
    public Vector2 JumpOffset;
    float tempAxisKey;
    float tempAxis;

    private float moveLeft;
    private float moveRight;

    public float GroundCheckEndPoint;

    private Vector2 position;
    private Vector2 velocity;

    public int jumpLag;
    private int totalJumpFrames;

    private GameObject[] AttackObjs;
    public int AttackLag;
    private float attackLife = 0.2f;
    private int totalAttackFrames;
    private WaitForSeconds attackDisableDelay;
    public float InvicibilityFrames;
    private float invincibilityCount;


    private bool facingRight;
    private bool canTurnAround;

    //private bool punching;
    public int PunchPercentAdd;
    /// <summary>
    /// Eventually to be used so that less players = more damage per player.
    /// </summary>
    private float damageTweek;
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

    private enum attackType { Up, Down, Left, Right, None };
    /// <summary>
    /// # of Frames of holding down before a charge is initiated.
    /// </summary>
    private float startingPunchTime;
    private attackType chargingAttack;
    public float MaxNormalPunchTime;
    public float MaxChargePunchTime;
    /// <summary>
    /// How long after fully charged can we hold it for? 
    /// </summary>
    public float MaxChargePunchHold;
    private float chargeFistSizeMultiplier;
    private float preChargeFistSizeMultiplier;
    private float normalFistSizeMultiplier;
    public float SpeedWhileChargingModifier;
    public float JumpSpeedWhileChargingModifier;
    private float movSpeedChargeModifier;
    private float jumpSpeedChargeModifier;
    private float chargePercentage;
    public float ExponentialDamageMod;
    public AudioClip[] ChargeSFX;

    private float damage;
    private bool isDead;

    private int lastHitBy;
    private float lastHitTime;
    private float lastHitForgetLength;
    
    public AudioClip DeathNoise;
    private Color deathColor;
    private Color lifeColor;
    public AudioClip PunchNoise;
    private AudioSource myAudioSrc;
    private int currentSFX;


    private bool controlsPaused;
    private float spawnPause;
    private WaitForSeconds spawnPauseWait;

    private Animator anim;
    private ParticleSystem mainPartSys;
    private ParticleSystem deathParts;
    private Image bodyImg;

    public float BoxPunch;

    public Vector2 LeftRaytraceOffset;
    public Vector2 RightRaytraceOffset;

    public bool IsPlayerControlled;

    void Awake()
    {
        currentSFX = -1;
        IsPlayerControlled = false;
        anim = GetComponent<Animator>();

        foreach(ParticleSystem partSys in GetComponentsInChildren<ParticleSystem>())
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

        foreach (Image img in GetComponentsInChildren<Image>())
        {
            if (img.gameObject.name == "BodyRender")
            {
                bodyImg = img;
            }
        }

        moveRight = 0;
        moveLeft = 0;
        controlsPaused = false;
        myAudioSrc = GetComponent<AudioSource>();
        myAudioSrc.clip = DeathNoise;
        //punching = false;

        deathColor = Color.clear;
        lifeColor = Color.white;
        isDead = false;
        StrengthsList = GameObject.FindGameObjectWithTag("Master").
            GetComponent<Master>().GetStrengthList();

        jumpForceTemp = 0f;
        speedTemp = 0f;
        attackDisableDelay = new WaitForSeconds(attackLife);
        facingRight = true;
        canTurnAround = true;

        position = new Vector2();
        _Rigibody2D = GetComponent<Rigidbody2D>();
        jumpsRemaining = TotalJumpsAllowed;

        AttackObjs = new GameObject[3];
        AttackObjs[0] = transform.Find("PunchUp").gameObject;
        AttackObjs[1] = transform.Find("PunchForward").gameObject;
        AttackObjs[2] = transform.Find("PunchDown").gameObject;
        
        gameInput = GameObject.FindGameObjectWithTag("MobileController").GetComponent<GameInputController>();

        spawnPause = 0.5f;
        spawnPauseWait = new WaitForSeconds(spawnPause);

        lastHitBy = -1;
        lastHitTime = Time.time;
        lastHitForgetLength = 5;//Seconds

        chargingAttack = attackType.None;
        startingPunchTime = 0;
    }

    void Start()
    {
        damageTweek = 0.5f;
        movSpeedChargeModifier = 1.0f;
        jumpSpeedChargeModifier = 1.0f;
        chargeFistSizeMultiplier = 2.0f;
        preChargeFistSizeMultiplier = 0.58f;
        normalFistSizeMultiplier = 0.75f;
        float temp = normalFistSizeMultiplier;
        AttackObjs[0].transform.localScale.Set(temp, temp, 1f);
        AttackObjs[1].transform.localScale.Set(temp, temp, 1f);
        AttackObjs[2].transform.localScale.Set(temp, temp, 1f);

        //chargeReleased = true;
        //anim.SetTrigger("ChargeReleased");
    }

    void Update()
    {
        UpdateHurt();
        if (CamManager.GetTarget().gameObject.name != gameObject.name)
            return;

        //CBUG.Log("OOOH I AM" + name);
        //Jump Detection Only, no physics handling.
        if (controlsPaused) {
            moveLeft = 0;
            moveRight = 0;
            return;
        }

        updateSpecials();
        updateJumping();
        updateDownJumping();
        updateAttacks();
        updateMovement();
    }

    void FixedUpdate()
    {
        updateFacingDirection();
        updateIsGrounded();

        if (wasGrounded && isGrounded) {
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
        if (!isDead)
            _Rigibody2D.velocity = velocity;
        velocity = Vector3.zero;
    }

    void LateUpdate()
    {
        //Only recent hits count
        if (Time.time - lastHitTime > lastHitForgetLength) {
            lastHitBy = -1;
            lastHitTime = Time.time;
        }
    }

    private void UpdateHurt()
    {
        if (invincibilityCount >= 0)
            invincibilityCount--;
    }

    private void updateSpecials()
    {
        //if (Input.GetButtonDown("Special") && invincibilityCount < 0) {
        //    _PhotonView.RPC("SpecialActivate", PhotonTargets.All);
        //    PauseMvmnt();
        //}
    }

    private void updateFacingDirection()
    {
        if (chargingAttack != attackType.None || !canTurnAround)
            return;

        if (!facingRight && _Rigibody2D.velocity.x > 0.2f) {
            facingRight = true;
            transform.localScale = new Vector3(1, 1, 1);
        } else if (facingRight && _Rigibody2D.velocity.x < -0.2f) {
            facingRight = false;
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void setFacingDirection(bool toFaceRight)
    {
        if (toFaceRight)
        {
            facingRight = true;
            transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            facingRight = false;
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void updateJumping()
    {

        //bool K = ();

        //CBUG.Do(Input.GetAxisRaw("Jump") + "");

        if (  (gameInput.GetButton("Jump") || gameInput.GetAxis("Jump") > 0f)
            && jumpsRemaining > 0 
            && totalJumpFrames < 0)
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
        if (  (gameInput.GetButton("DownJump")|| gameInput.GetAxis("DownJump") < 0f)
            && canDownJump)
        {
            downJumped = true;
            canDownJump = false;
        }
    }

    private void updateMovement()
    {
        //tempAxis left n right, keyboar axis left n right, or no input
        tempAxis = gameInput.GetAxis("MoveHorizontal");
        if (tempAxis > 0) {
            moveLeft = 0;
            moveRight = tempAxis;
        } else if (tempAxis < 0) {
            moveLeft = tempAxis;
            moveRight = 0;
        } else {
            moveLeft = 0;
            moveRight = 0;
        }
    }

    private void updateDownJumpingPhysics()
    {
        if (downJumped) {
            //CBUG.Log("DownJumped");
            jumpForceTemp = DownJumpForce;
            downJumped = false;
        }
        velocity.y -= downJumpForceTemp;
        downJumpForceTemp = Mathf.Lerp(downJumpForceTemp, 0f, DownJumpDecel);
    }

    private void updateJumpingPhysics()
    {
        if (jumped) {
            jumpForceTemp = JumpForce * (!isGrounded ? 1.0f : jumpSpeedChargeModifier);
            jumped = false;
        }
        velocity.y += jumpForceTemp;
        jumpForceTemp = Mathf.Lerp(jumpForceTemp, 0f, JumpDecel);
    }


    private void updateMovementPhysics()
    {
        //Normally we'd have any Input Handling get called from Update,
        //But dropped inputs aren't a problem with 'getaxis' since it's
        //Continuous and not a single frame like GetButtonDown
        if (moveRight != 0) {
            speedTemp = Mathf.Lerp(speedTemp, Speed * moveRight, SpeedAccel);
        } else if (moveLeft != 0) {
            speedTemp = Mathf.Lerp(speedTemp, Speed * moveLeft, SpeedAccel);
        } else if (isGrounded) {
            speedTemp = Mathf.Lerp(speedTemp, 0f, SpeedDecel);
        } else {
            speedTemp = Mathf.Lerp(speedTemp, 0f, AirSpeedDecel);
        }
        if (!punchForceApplied)
            velocity.x += speedTemp * (!isGrounded ? 1.0f : movSpeedChargeModifier);
    }

    private void updateIsGrounded()
    {
        set2DPosition();

        RaycastHit2D hitLeft =
            Physics2D.Raycast(position + JumpOffset + LeftRaytraceOffset,
                              -Vector2.up,
                              GroundCheckEndPoint,
                              mask.value);

        Debug.DrawLine(position + JumpOffset + LeftRaytraceOffset,
                       (position + JumpOffset + LeftRaytraceOffset) + (-Vector2.up * GroundCheckEndPoint),
                       Color.red,
                       0.01f);

        RaycastHit2D hitRight =
            Physics2D.Raycast(position + JumpOffset + RightRaytraceOffset,
                              -Vector2.up,
                              GroundCheckEndPoint,
                              mask.value);

        Debug.DrawLine(position + JumpOffset + RightRaytraceOffset,
                       (position + JumpOffset + RightRaytraceOffset) + (-Vector2.up * GroundCheckEndPoint),
                       Color.red,
                       0.01f);

        wasGrounded = isGrounded;
        bool isLeftGrounded = hitLeft.collider != null;
        bool isRightGrounded = hitRight.collider != null;
        isGrounded = (isLeftGrounded || isRightGrounded);

        //hit.collider.gameObject.layer
        ///Can't regain jumpcounts before jump force is applied.
        if (isRightGrounded && !jumped) {
            //CBUG.Log("Grounded on: " + (hit.collider.name));
            jumpsRemaining = TotalJumpsAllowed;
            transform.SetParent(hitRight.collider.transform);
        } else if (isLeftGrounded && !jumped) {
            //CBUG.Log("Grounded on: " + (hit.collider.name));
            jumpsRemaining = TotalJumpsAllowed;
            transform.SetParent(hitLeft.collider.transform);
        } else if (!isGrounded) {
            transform.SetParent(null);
            isGrounded = false;
            //CBUG.Log("Grounded on: " + (hit.collider.name));
        } else {
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

        // If ability to attack is on cooldown
        if (totalAttackFrames < 0) {

            //Get Attack Input for this frame
            if (chargingAttack == attackType.None)
            {
                //startingPunchTime will always be the time of the 
                //previous frame before attacking
                startingPunchTime = Time.time;
                movSpeedChargeModifier = 1.0f;
                jumpSpeedChargeModifier = 1.0f;
                if (gameInput.GetButton("Right"))
                {
                    chargingAttack = attackType.Right;
                    anim.SetTrigger("IsHoriz");
                    setFacingDirection(chargingAttack == attackType.Right);
                }
                else if (gameInput.GetButton("Left"))
                {
                    anim.SetTrigger("IsHoriz");
                    chargingAttack = attackType.Left;
                    setFacingDirection(chargingAttack == attackType.Right);
                }
                else if (gameInput.GetButton("Up"))
                {
                    chargingAttack = attackType.Up;
                    anim.SetTrigger("IsVert");
                    anim.SetTrigger("IsUp");
                }
                else if (gameInput.GetButton("Down"))
                {
                    chargingAttack = attackType.Down;
                    anim.SetTrigger("IsVert");
                    anim.SetTrigger("IsDown");
                }
            }

            // Begin Per frame while charging check
            if (chargingAttack != attackType.None)
            {
                // Cancel charging animation if you charge for too long.
                if (Time.time - startingPunchTime > (MaxChargePunchTime + MaxChargePunchHold))
                {
                    chargingAttack = attackType.None;
                    anim.SetBool("IsCharging", false);
                    anim.SetTrigger("ChargeFailed");
                }
                else if (Time.time - startingPunchTime > MaxNormalPunchTime)
                {
                    movSpeedChargeModifier = SpeedWhileChargingModifier;
                    jumpSpeedChargeModifier = JumpSpeedWhileChargingModifier;
                    anim.SetBool("IsCharging", true);
                }
            }
            //End Per Frame WHILE Charging Check

            //Begin Charge Attack Release check
            if (chargingAttack != attackType.None)
            {
                string attackName = "";
                switch (chargingAttack)
                {
                    case attackType.Right:
                        if (!gameInput.GetButton("Right"))
                        {
                            chargingAttack = attackType.None;
                            attackName = "NormalAttackForward";
                        }
                        break;
                    case attackType.Left:
                        if (!gameInput.GetButton("Left"))
                        {
                            chargingAttack = attackType.None;
                            attackName = "NormalAttackForward";
                        }
                        break;
                    case attackType.Up:
                        if (!gameInput.GetButton("Up"))
                        {
                            chargingAttack = attackType.None;
                            attackName = "NormalAttackUp";
                        }
                        break;
                    case attackType.Down:
                        if (!gameInput.GetButton("Down"))
                        {
                            chargingAttack = attackType.None;
                            attackName = "NormalAttackDown";
                        }
                        break;
                }//end charge attack release check

                //If it's now none after we find out the holding button was released.
                if (chargingAttack == attackType.None)
                {
                    if (Time.time - startingPunchTime < MaxNormalPunchTime)
                    {
                        PitchAudio.Rand(myAudioSrc);
                        myAudioSrc.PlayOneShot(PunchNoise);
                        StartCoroutine(stopAnimationWithDelay(attackName));
                        anim.SetBool(attackName, true);
                        totalAttackFrames = AttackLag;
                        chargePercentage = 0f;
                    }
                    else if (Time.time - startingPunchTime < (MaxChargePunchTime + MaxChargePunchHold))
                    {
                        float totalEffectiveChargeTime = Mathf.Clamp(Time.time - startingPunchTime, 
                                                                    0f,
                                                                    MaxChargePunchTime);
                        //launch charging animation
                        chargePercentage = Mathf.Pow(totalEffectiveChargeTime, ExponentialDamageMod) / MaxChargePunchTime;
                        anim.SetTrigger("ChargeReleased"); //TODO: SET ANIMATION NAMES FOR UP AND DOWN
                    }
                        //CBUG.Do("TOtal held down time: " + (Time.time - startingPunchTime));
                    anim.SetBool("IsCharging", false);
                }
            }   
            //if (gameInput.GetButton("Up")) {
            //    //AttackObjs[0].SetActive(true);
            //    myAudioSrc.PlayOneShot(PunchNoise);
            //    StartCoroutine(stopAnimationWithDelay("NormalAttackUp"));
            //    anim.SetBool("NormalAttackUp", true);
            //    totalAttackFrames = AttackLag;
            //    //_PhotonView.RPC("UpAttack", PhotonTargets.Others);
            //}
            //if (gameInput.GetButton("Down")) {
            //    //AttackObjs[2].SetActive(true);
            //    myAudioSrc.PlayOneShot(PunchNoise);
            //    StartCoroutine(stopAnimationWithDelay("NormalAttackDown"));
            //    anim.SetBool("NormalAttackDown", true);
            //    totalAttackFrames = AttackLag;
            //    //_PhotonView.RPC("DownAttack", PhotonTargets.Others);
            //}

            //if (!facingRight && (Input.GetButtonDown("Left") || gameInput.GetButton("Left"))) {
            //    //AttackObjs[1].SetActive(true);
            //    myAudioSrc.PlayOneShot(PunchNoise);
            //    StartCoroutine(stopAnimationWithDelay("NormalAttackForward"));
            //    anim.SetBool("NormalAttackForward", true);
            //    totalAttackFrames = AttackLag;
            //    //_PhotonView.RPC("ForwardAttack", PhotonTargets.Others);
            //}
        }
        totalAttackFrames -= 1;
    }

    #region RPC Functions.
    //TODO: Determine allowed scope of RPC functions.
    [PunRPC]
    void SetID(int ID)
    {
        this.ID = ID;
        CBUG.Do("Recording ID " + ID + " with Gamemaster.");
        CBUG.Do("Character is: " + gameObject.name);
        GameManager.AddPlayer(ID, gameObject);
    }

    [PunRPC]
    void SpecialActivate()
    {
        //anim.SetBool("Activating", true);
    }

    /// <summary>
    /// Calls GameManager's RecordDeath. Respawn Handling
    /// is done from there.
    /// </summary>
    /// <param name="killer"></param>
    /// <param name="killed"></param>
    private void OnDeath(int killer, int killed)
    {
        isDead = true;
        //Hide Self till respawn (or stay dead, ghost)
        bodyImg.enabled = false;
        deathParts.Play();
        //Freeze and Clear motion
        _Rigibody2D.velocity = Vector2.zero;
        velocity = Vector2.zero;

        //Death Map: OnDeath > RecordDeath > HandleDeath >
        // doRespawnOrGhost
        GameManager.RecordDeath(killer, killed, false);
        GameManager.HandleDeath(killed, false);
    }
    #endregion

    /// <summary>
    /// Does nothing in offline mode. No lives, no ghosting.
    /// </summary>
    public override void Ghost()
    {
        //transform.tag = "PlayerGhost";
    }

    /// <summary>
    /// Respawning. Only Graphical UNLESS is our Player. Then we reposition ourselves.
    /// </summary>
    /// <param name="spawnPoint"></param>
    public override void Respawn(Vector3 spawnPoint)
    {
        transform.position = spawnPoint;
        damage = 0;

        _Rigibody2D.isKinematic = false;
        StartCoroutine(spawnProtection());
    }

    public void SetNewTarget()
    {
        CamManager.SetTarget(transform);
        GameHUDController.SetDamageTo(damage);
    }

    public void TurnPartsOn()
    {
        mainPartSys.Play();
    }

    public void TurnPartsOff()
    {
        mainPartSys.Stop();
    }

    private IEnumerator spawnProtection()
    {
        yield return spawnPauseWait;
        //TODO: Spawn animation
        bodyImg.enabled = true;
        isDead = false;
        deathParts.Stop();
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

        if (isDead || col == null)
            return;


        //apply force . . .
        else if (col.name.Contains("Punch")) {
            //Get name of puncher
            lastHitBy = col.GetComponentInParent<PlayerController2DOffline>().ID;
            lastHitTime = Time.time;
            
            if (invincibilityCount > 0) {
                return;
            } else {
                invincibilityCount = InvicibilityFrames;
            }
            float enemyChargePercentage = col.GetComponentInParent<PlayerController2DOffline>().GetChargePercent();
            damage += (PunchPercentAdd + (PunchPercentAdd * enemyChargePercentage));


            if (damage < 30)
            {
                hurtAnim(1);
            }
            else if (damage < 60)
            {
                hurtAnim(2);
            }
            else
            {
                hurtAnim(3);
            }

            if (IsPlayerControlled)
                GameHUDController.SetDamageTo(damage);

            if (col.name == "PunchForward") {
                //velocity += Vector2.right * PunchForceForward_Forward;
                if (col.transform.parent.localScale.x > 0) {
                    Vector2 temp = Vector2.right * (PunchForceForward_Forward + StrengthsList[col.transform.parent.name] - Defense);
                    temp += Vector2.up * (PunchForceForward_Up + StrengthsList[col.transform.parent.name] - Defense);
                    StartCoroutine(
                        applyPunchForce(temp * (damage / 100f) * damageTweek)
                    );
                } else {
                    Vector2 temp = Vector2.left * (PunchForceForward_Forward + StrengthsList[col.transform.parent.name] - Defense);
                    temp += Vector2.up * (PunchForceForward_Up + StrengthsList[col.transform.parent.name] - Defense);
                    StartCoroutine(
                        applyPunchForce(temp * (damage / 100f) * damageTweek)
                    );
                }
            } else if (col.name == "PunchUp") {
                StartCoroutine(
                    applyPunchForce(
                        (Vector2.up * (PunchForceUp + StrengthsList[col.transform.parent.name] - Defense) 
                        * (damage / 100f)
                        * damageTweek)
                    )
                );
            } else {
                if (!isGrounded) {
                    StartCoroutine(
                        applyPunchForce(
                            (Vector2.down * (PunchForceDown + StrengthsList[col.transform.parent.name] - Defense) 
                            * (damage / 100f)
                            * damageTweek)
                        )
                    );
                } else {
                    StartCoroutine(
                        applyPunchForce(
                            (Vector2.up * (PunchForceDown + StrengthsList[col.transform.parent.name] - Defense) * (damage / 200f))
                        )
                    );
                }
            }
        }
    }

    /// <summary>
    /// For stopping triggers.
    /// </summary>
    /// <param name="boolName"></param>
    /// <returns></returns>
    private IEnumerator stopAnimationWithDelay(string boolName)
    {
        yield return attackDisableDelay;
        anim.SetBool(boolName, false);
        //punching = false;
    }

    private IEnumerator applyPunchForce(Vector2 punchForce)
    {
        Vector2 tempPunchForce = punchForce;
        bool isTempForceLow = false;

        if(IsPlayerControlled)
            CamManager.PunchShake(tempPunchForce.magnitude);

        while (tempPunchForce.magnitude > 0.01f) {
            velocity += tempPunchForce;
            tempPunchForce = Vector2.Lerp(tempPunchForce, Vector2.zero, PunchForceDecel);

            if (!isTempForceLow &&
                tempPunchForce.magnitude < punchForce.magnitude * PunchDisablePerc) {
                isTempForceLow = true;
                //if the force goes below 25%, let the character move again. 
                punchForceApplied = false;
            } else if (!isTempForceLow) {
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

            OnDeath(lastHitBy, ID);
        }
    }

    private void hurtAnim(int hurtNum)
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

    /// <summary>
    /// Referenced by animation.
    /// </summary>
    public void AssignFistSize()
    {
        if(AttackObjs[0].transform.localScale.x > normalFistSizeMultiplier)
        {
            float temp = normalFistSizeMultiplier;
            AttackObjs[0].transform.localScale = new Vector3(temp, temp, 1f);
            AttackObjs[1].transform.localScale = new Vector3(temp, temp, 1f);
            AttackObjs[2].transform.localScale = new Vector3(temp, temp, 1f);
        }
        else
        {
            float fistScale;
            fistScale = normalFistSizeMultiplier  + chargePercentage * (chargeFistSizeMultiplier - normalFistSizeMultiplier);
            AttackObjs[0].transform.localScale = new Vector3(fistScale, fistScale, 1f);
            AttackObjs[1].transform.localScale = new Vector3(fistScale, fistScale, 1f);
            AttackObjs[2].transform.localScale = new Vector3(fistScale, fistScale, 1f);
        }
    }

    public float GetChargePercent ()
    {
        return chargePercentage;
    }

    public void PlayCharacterSFX(int SfxNum)
    {
        if (currentSFX == SfxNum)
            return;
        currentSFX = SfxNum;
        //myAudioSrc.Stop();
        myAudioSrc.clip = ChargeSFX[SfxNum];
        myAudioSrc.Play();
    }


    public void PlayCharacterSFX_OneShot(int SfxNum)
    {
        myAudioSrc.PlayOneShot(ChargeSFX[SfxNum]);
    }

    public void DisableFacingDirectionChange()
    {
        canTurnAround = false;
    }

    public void EnableFacingDirectionChange()
    {
        canTurnAround = true;
    }
}
//public void CheckWon()
//{
//    if (m_PhotonView.isMine && !isDead)
//    {
//        BattleUI.Won();
//    }
//}