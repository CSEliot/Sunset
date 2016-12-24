using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple Audio helper class to make audio calls easily without clunky reference holding.
/// Helper classes like these should not be referenced continuously.
/// </summary>
public class _Audio : MonoBehaviour {

    private Master M;
    private bool isNewMusicIncoming;
    private const string myTag = "AudioHelper";

    private void Awake()
    {
        M = GameObject.FindGameObjectWithTag("Master").GetComponent<Master>();
    }
 
    // Use this for initialization
    void Start () {
        isNewMusicIncoming = false;
        tag = _Audio.myTag;
	}

    #region Public Methods
    //--Static Helpers
    /// <summary>
    /// Mostly unused outside of static helper functions. Given public access for convenience.
    /// </summary>
    /// <returns></returns>
    public static _Audio GetRef()
    {
        return GameObject.FindGameObjectWithTag(_Audio.myTag).GetComponent<_Audio>();
    }

    /// <summary>
    /// Plays a sound effect. Unless "ChangeMusic()" is called, then swaps music once.
    /// </summary>
    /// <param name="audNum">Refers to the location of the audio file within the sfx/msx array.</param>
    public static void Play(int audNum)
    {
        _Audio.GetRef()._play(audNum);
    }

    /// <summary>
    /// Next time _Audio.Play(x) is called, it'll swap the music instead.
    /// </summary>
    public static void ChangeMusic()
    {
        _Audio.GetRef().isNewMusicIncoming = true;
    }
    #endregion

    #region Private Helper Methods
    /// <summary>
    /// Helper function to "_Audio.Play(x)"
    /// "Plays a sound effect. Unless "ChangeMusic()" is called, then swaps music once.'
    /// </summary>
    /// <param name="audNum">Refers to the location of the audio file within the sfx/msx array.</param>
    private void _play(int audNum)
    {
        if (isNewMusicIncoming) {
            isNewMusicIncoming = false;
            M.PlayMSX(audNum);
            return;
        } 
        M.PlaySFX(audNum);
    }
    #endregion
}
