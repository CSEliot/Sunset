using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class StageAnimations : MonoBehaviour {

    private Animator localAnim;

	// Use this for initialization
	void Start () {
        localAnim = GetComponent<Animator>();
        localAnim.enabled = false;
        gameObject.tag = "StageAnim";
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public static void Activate()
    {
        GameObject[] localAnims = GameObject.FindGameObjectsWithTag("StageAnim");
        if (localAnims == null || localAnims.Length == 0)
            return;

        for(int x = 0; x < localAnims.Length; x++) {
            localAnims[x].GetComponent<StageAnimations>().ActivateLocal();
        }
    }

    public void ActivateLocal()
    {
        localAnim.enabled = true;
    }


}
