using UnityEngine;
using System.Collections;

public class SceneChecking : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ChangeSceneTo( int sceneNum){
		Application.LoadLevel (sceneNum);
	}

	public void ChangeSceneTo( string sceneName){
		Application.LoadLevel (sceneName);
	}
}