using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class StageAnimations : MonoBehaviour {

    public Animator[] List; 

	// Use this for initialization
	void Start () {
        for (int x = 0; x < List.Length; x++)
        {
            List[x].enabled = false;
            List[x].Play("Entry");
        }
        if (SceneManager.GetActiveScene().name == "GameScreen_Practice")
            Activate();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public static void Activate()
    {
        if(GameObject.FindGameObjectWithTag("StageAnim") != null)
            GameObject.FindGameObjectWithTag("StageAnim").GetComponent<StageAnimations>()._Activate();
    }

    public void _Activate()
    {
        if (List == null || List.Length == 0)
            return;

        for (int x = 0; x < List.Length; x++)
        {
            List[x].enabled = true;
            List[x].Play("Entry");
        }
    }


}
