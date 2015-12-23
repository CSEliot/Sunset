using UnityEngine;
using System.Collections;

public class JumpAndRunMovement : MonoBehaviour 
{
    public float Speed;
    public float JumpForce;
    public float Gravity;

    PhotonView m_PhotonView;
    private float oldX;
    private float oldY;

    bool m_IsGrounded;

    private float hMove;
    private float vMove; 

    void Awake() 
    {
        hMove = 0;
        vMove = 0;
        m_PhotonView = GetComponent<PhotonView>();
    }

    void Update() 
    {
        UpdateIsGrounded();
        UpdateFacingDirection();
        UpdateMovement();
        UpdateJumping();
    }

    void FixedUpdate()
    {
        oldX = transform.localPosition.x;
        oldY = transform.localPosition.y;

        if( m_PhotonView.isMine == false )
        {
            return;
        }

        transform.Translate(hMove, vMove, 0f);
    }

    void UpdateFacingDirection()
    {
        if( transform.localPosition.x - oldX > 0.2f )
        {
            transform.localScale = new Vector3( 1, 1, 1 );
        }
        else if (transform.localPosition.x - oldX < -0.2f)
        {
            transform.localScale = new Vector3( -1, 1, 1 );
        }
    }

    void UpdateJumping()
    {
        if( Input.GetKey( KeyCode.Space ) == true && m_IsGrounded == true )
        {
            vMove = JumpForce + Gravity;
        }
        else
        {
            vMove = Gravity;
        }
    }

    //[PunRPC]
    //void DoJump()
    //{
    //    m_Animator.SetTrigger( "IsJumping" );
    //}

    void UpdateMovement()
    {
        if( Input.GetAxisRaw( "Horizontal" ) > 0.5f )
        {
            hMove = Speed;
            
        }
        else if( Input.GetAxisRaw( "Horizontal" ) < -0.5f )
        {
            hMove = -Speed;
        }
        else
        {
            hMove = 0;
        }
    }

    void UpdateIsGrounded()
    {
        Vector2 position = new Vector2( transform.position.x, transform.position.y );

        //RaycastHit2D hit = Physics2D.Raycast( position, -Vector2.up, 0.1f, 1 << LayerMask.NameToLayer( "Ground" ) );
        RaycastHit2D hit = Physics2D.Raycast(position, -Vector2.up, 0.1f);

        m_IsGrounded = hit.collider != null;
    }
}
