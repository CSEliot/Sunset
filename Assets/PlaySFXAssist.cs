using UnityEngine;
using System.Collections;

public class PlaySFXAssist : MonoBehaviour
{
    public void Play( int SFX)
    {
        GameObject.FindGameObjectWithTag("Master").GetComponent<Master>().PlaySFX(SFX);
    }
}
