using UnityEngine;
using System.Collections;

public class SpecialsController : MonoBehaviour {

    private int specialsNum;
    private JumpAndRunMovement j;
    private Animator anim;

	// Use this for initialization
	void Start () {
        anim = GetComponent<Animator>();
        j = GetComponentInParent<JumpAndRunMovement>();
        if (transform.parent.name == "Lore")
        {
            specialsNum = 2;
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void StopChar()
    {
        //j.UnpauseMvmnt();
    }

    public void UnstopChar()
    {
        anim.SetBool("Activating", false);
        j.UnpauseMvmnt();
        ActivateSpecial(specialsNum);
    }

    private void ActivateSpecial(int spclNum)
    {
        Debug.Log("Activating Special Number: " + spclNum);
    }
}
