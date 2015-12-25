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

    public float GroundCheckEndPoint;

    private Vector2 position;

    public int jumpLag;
    private int totalJumpFrames;

    void Awake() 
    {
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
        UpdateMovement();
        UpdateJumping();
        UpdateFacingDirection();
        m_PhotonTransform.SetSynchronizedValues(m_Body.velocity, 0f);
    }

    void FixedUpdate()
    {
        if( m_PhotonView.isMine == false )
        {
            return;
        }

    }

    void UpdateFacingDirection()
    {
        if( m_Body.velocity.x > 0.2f )
        {
            transform.localScale = new Vector3( 1, 1, 1 );
        }
        else if( m_Body.velocity.x < -0.2f )
        {
            transform.localScale = new Vector3( -1, 1, 1 );
        }
    }

    void UpdateJumping()
    {
        if( Input.GetKey( KeyCode.Space ) == true 
            && jumpsRemaining > 0 && totalJumpFrames < 0)
        {
            m_Body.AddForce( Vector2.up * JumpForce , ForceMode2D.Impulse);
            jumped = true;
            jumpsRemaining -= 1;
            m_PhotonView.RPC( "DoJump", PhotonTargets.Others );
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

        if( Input.GetAxisRaw( "Horizontal" ) > 0.5f )
        {
            movementVelocity.x = Speed;
            
        }
        else if( Input.GetAxisRaw( "Horizontal" ) < -0.5f )
        {
            movementVelocity.x = -Speed;
        }
        else
        {
            movementVelocity.x = 0;
        }

        m_Body.velocity = movementVelocity;
    }

    void UpdateIsRunning()
    {
    }

    void UpdateIsGrounded()
    {
        Set2DPosition();

        RaycastHit2D hit = 
            Physics2D.Raycast(position, 
                              -Vector2.up, 
                              GroundCheckEndPoint, 
                              mask.value);

            Debug.DrawLine(position, 
                           position - Vector2.up*GroundCheckEndPoint, 
                           Color.red, 
                           0.02f);
        m_IsGrounded = hit.collider != null;
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
}
