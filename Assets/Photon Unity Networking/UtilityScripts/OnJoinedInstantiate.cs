using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class OnJoinedInstantiate : MonoBehaviour
{
    public Transform[] SpawnPosition;
    public float PositionOffset = 2.0f;
    private Master m;

     

    void Awake()
    {
        m = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
    }

    public void OnReadyUp(int myID)
    {
        Vector3 spawnPos;
        int totalPlayersFound = PhotonNetwork.playerList.Length;
        spawnPos = SpawnPosition[myID].position;

        Vector3 random = Random.insideUnitSphere;
        random.y = 0;
        random = random.normalized;
        Vector3 itempos = spawnPos;
        PhotonNetwork.Instantiate(m.GetClientCharacterName(), spawnPos, Quaternion.identity, 0);
    }
}
