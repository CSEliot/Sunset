using UnityEngine;
using System.Collections;

public class OnJoinedInstantiate : MonoBehaviour
{
    public Transform SpawnPosition;
    public float PositionOffset = 2.0f;
    private Master m;

    void Awake()
    {
        m = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
    }

    public void OnJoinedRoom()
    {
        Vector3 spawnPos = Vector3.up;
        if (this.SpawnPosition != null)
        {
            spawnPos = this.SpawnPosition.position;
        }

        Vector3 random = Random.insideUnitSphere;
        random.y = 0;
        random = random.normalized;
        Vector3 itempos = spawnPos + this.PositionOffset * random;

        PhotonNetwork.Instantiate(m.GetClientCharacter(), itempos, Quaternion.identity, 0);
        
    }
}
