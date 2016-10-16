using UnityEngine;
using System.Collections;

public abstract class PlayerController2D : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    /// <summary>
    /// Nothing to do. You stay invisible and immobile
    /// </summary>
    public abstract void Ghost();

    public abstract void Respawn(Vector3 spawnPoint);
}
