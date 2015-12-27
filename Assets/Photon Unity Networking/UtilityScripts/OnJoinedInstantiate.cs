using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class OnJoinedInstantiate : MonoBehaviour
{
    public Transform[] SpawnPosition;
    public float PositionOffset = 2.0f;
    private Master m;

    public Sprite[] UIHeads;
    public Image PlayerHead; 

    void Awake()
    {
        if (GameObject.FindGameObjectWithTag("Master") == null)
        {
            SceneManager.LoadScene("CharacterSelect");
        }
        else
        {
            m = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
        }
    }

    public void OnJoinedRoom()
    {
        Vector3 spawnPos = Vector3.up;
        int totalPlayersFound = PhotonNetwork.playerList.Length;
        spawnPos = SpawnPosition[totalPlayersFound-1].position;

        Vector3 random = Random.insideUnitSphere;
        random.y = 0;
        random = random.normalized;
        Vector3 itempos = spawnPos + this.PositionOffset * random;
        PhotonNetwork.Instantiate(m.GetClientCharacter(), itempos, Quaternion.identity, 0);
        PlayerHead.sprite = GetImage();
    }

    private Sprite GetImage()
    {
        for (int i = 0; i < UIHeads.Length; i++)
        {
            if (UIHeads[i].name == m.GetClientCharacter())
            {
                return UIHeads[i];
            }
        }
        Debug.LogError("No Head Name Found!");
        return null;
    }
}
