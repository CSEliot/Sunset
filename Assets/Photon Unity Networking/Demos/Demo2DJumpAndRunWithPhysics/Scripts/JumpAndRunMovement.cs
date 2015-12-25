using UnityEngine;
using System.Collections;

public class JumpAndRunMovement : MonoBehaviour 
{
    public float Speed;
    public float JumpForce;
    //public float Gravity;

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

    public int jumpLag;
    private int totalJumpFrames;

    public GameObject[] AttackObjs;
    public int AttackLag;
    private int totalAttackFrames;
    private WaitForSeconds attackDisableDelay;

    private bool facingRight;

    public float PunchForceUp;
    public float PunchForceForward_Forward;
    public float PunchForceForward_Up;
    public float PunchForceDown;


    void Awake() 
    {
        attackDisableDelay = new WaitForSeconds(0.15f);
        facingRight = true;
        position = new Vector2();
        m_Body = GetComponent<Rigidbody2D>();
        jumpsRemaining = TotalJumpsAllowed;
        m_PhotonView = GetComponent<PhotonView>();
        m_PhotonTransform = GetComponent<PhotonTransformView>();
    }

    void Update() 
    {
        UpdateIsGrounded();
        UpdateIsRunning();
        m_PhotonTransform.SetSynchronizedValues(m_Body.velocity, 0f);
        UpdateFacingDirection();
        if(!m_PhotonView.isMine)
            return;
        UpdateAttacks();
        UpdateJumping();
    }

    void FixedUpdate()
    {
        if(!m_PhotonView.isMine)
            return;
        UpdateMovement();
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
            m_Body.AddForce( Vector2.up * JumpForce , ForceMode2D.Impulse);
            jumped = true;
            jumpsRemaining -= 1;
            m_PhotonView.RPC( "DoJump", PhotonTargets.Others);
            Debug.Log("Jumping!");
            totalJumpFrames = jumpLag;
        }
        totalJumpFrames -= 1;
    }

    [PunRPC]
    void DoJump()
    {
    }

    void UpdateMovement()
    {
        Vector2 movementVelocity = m_Body.velocity;

        movementVelocity.x = Speed * Input.GetAxis("Horizontal");

        m_Body.velocity = movementVelocity;
    }

    void UpdateIsRunning()
    {
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
        if (col != null)
        {
            Debug.Log("I collided with: " + col.name);
            Debug.Log("And I am: " + gameObject.name);
        }
        
        //apply force . . .
        if (col.name.Contains("Punch"))
        {
            if (col.name.Contains("Forward"))
            {
                m_Body.AddForce(Vector2.right * PunchForceForward_Forward +
                                Vector2.up * PunchForceForward_Up, ForceMode2D.Impulse);
            }
            else if (col.name.Contains("Up"))
            {
                m_Body.AddForce(Vector2.up * PunchForceUp, ForceMode2D.Impulse);
            }
            else if (col.name.Contains("Down"))
            {
                m_Body.AddForce(Vector2.down * PunchForceDown, ForceMode2D.Impulse);
            }
        }
    }

    IEnumerator DisableDelay(GameObject dis)
    {
        yield return attackDisableDelay;
        dis.SetActive(false);
    }
}
