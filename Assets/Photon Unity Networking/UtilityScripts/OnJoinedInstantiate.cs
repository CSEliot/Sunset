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
        PhotonNetwork.Instantiate(m.GetClientCharacter(), spawnPos, Quaternion.identity, 0);
        PlayerHead.sprite = GetImage();
    }

    public Sprite GetImage()
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

    public int GetImageNum()
    {
        for (int i = 0; i < UIHeads.Length; i++)
        {
            if (UIHeads[i].name == m.GetClientCharacter())
            {
                return i;
            }
        }
        Debug.LogError("No Head Name Found!");
        return -1;
    }

    public Sprite GetImage(int num)
    {
        return UIHeads[num];
    }
}
