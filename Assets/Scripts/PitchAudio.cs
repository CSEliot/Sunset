using UnityEngine;
using System.Collections;

public static class PitchAudio
{
    static float originalPitch = -1f;
    static float pitchMin = 0.8f;
    static float pitchMax = 1.2f;

    // Use this for initialization
    public static void Rand(AudioSource src)
    {
        if (originalPitch == -1f)
            originalPitch = src.pitch;
        src.pitch = src.pitch * Random.Range(pitchMin, pitchMax);
    }
}
