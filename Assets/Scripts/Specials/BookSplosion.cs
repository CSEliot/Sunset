using UnityEngine;
using System.Collections;

public class BookSplosion : MonoBehaviour {

    private Rigidbody2D r;
    public float DownVelocity;
    private bool velocityAssigned;
    private string ownerTag;

	// Use this for initialization
	void Start () {
        velocityAssigned = false;
        r = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () {
        if (!velocityAssigned)
            r.velocity = new Vector2(0f, -DownVelocity);
            velocityAssigned = true;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        Destroy(gameObject);
    }

    public void SetOwnerTag(string tag)
    {
        ownerTag = tag;
    }
}
