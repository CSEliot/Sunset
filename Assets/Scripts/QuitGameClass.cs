using UnityEngine;
using System.Collections;

public class QuitGameClass : MonoBehaviour {

	// Use this for initialization
	void Start () {
        if (Application.platform == RuntimePlatform.WebGLPlayer)
        {
            gameObject.SetActive(false);
            CBUG.Do("Disabling exit button!");
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void QuitGame()
    {
		//System.Diagnostics.Process.GetCurrentProcess().Kill();
		//
        Application.Quit();
		//Test

    }
}
