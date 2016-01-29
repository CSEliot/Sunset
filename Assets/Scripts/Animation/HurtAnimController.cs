using UnityEngine;
using System.Collections;

public class HurtAnimController : MonoBehaviour {

    private Animator anim;
	// Use this for initialization
	void Start () {
        anim = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void UnHurt()
    {
        anim.SetBool("HurtSmall", false);
        anim.SetBool("HurtMedium", false);
        anim.SetBool("HurtBig", false);
    }
}
