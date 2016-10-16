using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ChangeScene : MonoBehaviour {

    public bool IsAdditive;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void GoTo(string SceneName)
    {
        SceneManager.LoadScene(SceneName, IsAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);
    }

}
