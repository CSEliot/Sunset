using UnityEngine;
using System.Collections;

public class SpecialsController : MonoBehaviour {

    private int specialsNum;
    private JumpAndRunMovement j;
    private Animator anim;

    public GameObject Book;
    public float SpawnHeight;
    public float SpawnAdjust;

	// Use this for initialization
	void Start () {
        anim = GetComponent<Animator>();
        j = GetComponentInParent<JumpAndRunMovement>();
        if (transform.parent.name == "Lore(Clone)")
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
        ActivateSpecial();
    }

    private void ActivateSpecial()
    {
        switch (specialsNum)
        {
            case 1:
                break;
            case 2:
                LoreOverload();
                break;
            default:
                Debug.LogError("BAD SPECIAL NUM GIVEN: " + specialsNum);
                break;

        }
    }

    private void LoreOverload()
    {
        Vector3 bookSpawn = transform.parent.position + new Vector3(SpawnAdjust, SpawnHeight, 0);
        GameObject book = Instantiate(Book, bookSpawn, Quaternion.EulerAngles(0f, 0f, 0f)) as GameObject;
        book.GetComponent<BookSplosion>().SetOwnerTag(transform.tag);
    }
}
