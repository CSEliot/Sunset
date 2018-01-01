using UnityEngine;
using System.Collections;

public class OpenURL : MonoBehaviour {

	public void Do()
    {
        if(Application.platform == RuntimePlatform.WebGLPlayer)
            Application.ExternalEval("window.open(\"http://www.unity3d.com\")");
        else
            Application.OpenURL("http://sunset.btong.me/");
    }
}
