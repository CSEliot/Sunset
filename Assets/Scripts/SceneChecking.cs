using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneChecking : MonoBehaviour {

    public bool newScene;
    public string targetSceneName;

	// Use this for initialization
	void Start () {
        newScene = false;
	}
	
	// Update is called once per frame
	void Update () {
        if (newScene)
        {
            ChangeScene();
        }
	}

	public void ChangeScene(){
        if(targetSceneName!="null")
            SceneManager.LoadScene(targetSceneName);
	}
}