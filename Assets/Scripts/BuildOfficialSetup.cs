using UnityEngine;
using System.Collections;

public class BuildOfficialSetup : MonoBehaviour {

    public GameObject[] DisableOnStart;
    public GameObject[] EnableOnStart;

    public GameObject[] Other;

    public bool SetupInEditor;

    // Use this for initialization
    void Awake () {

        if (Application.isEditor && !SetupInEditor)
            return;

        for(int x = 0; x < DisableOnStart.Length; x++) {
            DisableOnStart[x].SetActive(false);
        }
        for (int x = 0; x < EnableOnStart.Length; x++) {
            EnableOnStart[x].SetActive(true);
        }

    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
