using UnityEngine;
using System.Collections;

public class SpawnOfflineTest : MonoBehaviour {

    private Master m;

    public GameObject[] CharacterList;

    public Transform[] SpawnList;

    private bool isNewSceneActive; //Characters should be spawned in the INGAME scene.

	// Use this for initialization
	void Start () {

        isNewSceneActive = false;

        m = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();

        UnityEngine.SceneManagement.SceneManager.SetActiveScene(
            UnityEngine.SceneManagement.SceneManager.GetSceneByName("GameScreen_Test")
            );

	}
	
	// Update is called once per frame
	void Update () {

        if (!isNewSceneActive)
        {
            //chosen character is stored by name, we don't know position number
            Instantiate(CharacterList[m.Client_CharNum], SpawnList[0].position, Quaternion.Euler(Vector3.zero));

            isNewSceneActive = true;
        }


	}
}
