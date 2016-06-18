using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneChecking : MonoBehaviour {
    
    public string targetSceneName;
    private Master m;

	// Use this for initialization
	void Start () {
        m = GameObject.FindGameObjectWithTag("Master").GetComponent
            <Master>();
	}
	
	// Update is called once per frame
	void Update () {
	}

    public void ChangeScene() {
        Debug.Log("Changing scene to: " + targetSceneName);
        if(targetSceneName == "MapSelect" && m.IsTestMode)
        {
            SceneManager.LoadScene("GameScreen_Test");
            m.PlaySFX(0);
        }
        else if (targetSceneName != "null")
        {
            SceneManager.LoadScene(targetSceneName);
            m.PlaySFX(0);
        }
        if (m.Client_CharNum == 6)
        { 
            return;
        }
	}
}