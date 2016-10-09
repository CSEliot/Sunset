using UnityEngine;
using System.Collections;

public class RandPos : MonoBehaviour {

    public Vector3 MinValues;

    public Vector3 MaxValues;

    private Vector3 newPos = new Vector3();

    // Use this for initialization
    void Start () {

        newPos = new Vector3();

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void Randomize()
    {
        CBUG.Log("Randomizing! Skelly Position");
        newPos.Set(Random.Range(MinValues.x, MaxValues.x), Random.Range(MinValues.y, MaxValues.y), Random.Range(MinValues.z, MaxValues.z));
        transform.localPosition = newPos;
    }
}
