using UnityEngine;
using System.Collections;

public class JumpAndRunMovement : MonoBehaviour 
{
    public float Speed;
    public float JumpForce;
    //public float Gravity;

    Rigidbody2D m_Body;
    PhotonView m_PhotonView;
    private PhotonTransformView m_PhotonTransform;

    private bool m_IsGrounded;
    private int jumpsRemaining;
    public int TotalJumpsAllowed;

    public Vector2 GroundCheckStartOffset;
    public Vector2 GroundCheckEndPoint;

    private Vector2 position;

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
        UpdateFacingDirection();
        m_PhotonTransform.SetSynchronizedValues(m_Body.velocity, 0f);
    }

    void FixedUpdate()
    {
        if( m_PhotonView.isMine == false )
        {
            return;
        }

        UpdateMovement();
        UpdateJumping();
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
        if( Input.GetKey( KeyCode.Space ) == true && m_IsGrounded == true )
        {
            m_Body.AddForce( Vector2.up * JumpForce , ForceMode2D.Impulse);
            m_PhotonView.RPC( "DoJump", PhotonTargets.Others );
            Debug.Log("Jumping!");
        }
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

        //RaycastHit2D hit = Physics2D.Raycast( position, -Vector2.up, 0.1f, 1 << LayerMask.NameToLayer( "Ground" ) );
        RaycastHit2D hit = Physics2D.Raycast(position+GroundCheckStartOffset, 
                                            -Vector2.up+GroundCheckEndPoint, 
                                            (-Vector2.up.y+GroundCheckEndPoint.y));
        Debug.DrawLine(position + GroundCheckStartOffset,  position + GroundCheckStartOffset + GroundCheckEndPoint, Color.red, 0.02f);
        m_IsGrounded = hit.collider != null;
        if (m_IsGrounded)
        {
            Debug.Log ("Grounded on: " + (hit.collider.name));
        }
    }

    private void Set2DPosition()
    {
        position.x = transform.position.x;
        position.y = transform.position.y;
    }
}
