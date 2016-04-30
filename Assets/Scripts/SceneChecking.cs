using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneChecking : MonoBehaviour {

    public bool newScene;
    public string targetSceneName;
    private Master m;

	// Use this for initialization
	void Start () {
        m = GameObject.FindGameObjectWithTag("Master").GetComponent
            <Master>();
        newScene = false;
	}
	
	// Update is called once per frame
	void Update () {
        if (newScene)
        {
            ChangeScene();
        }
	}

    public void ChangeScene() {
        if (targetSceneName == "GameScreen_")
        {
            if (targetSceneName != "null")
            {
                SceneManager.LoadScene(targetSceneName + m.GetRoomName());
                m.PlaySFX(0);
            }
        }
        else if (m.Client_CharNum == 6)
        { 
            return;
        }
        else if (targetSceneName != "null")
        {
            SceneManager.LoadScene(targetSceneName);
            m.PlaySFX(0);
        }
	}
}