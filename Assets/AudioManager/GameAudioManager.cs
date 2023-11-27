using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAudioManager : MonoBehaviour
{
    public AudioClip[] audioClips;  // Array to store your audio clips
    private AudioSource audioSource;
    private List<int> lastPlayedIndices = new List<int>();
    private const int maxLastPlayedCount = 3;  // Number of songs to remember

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        PlayNextSong();
    }

    void Update()
    {
        // Check if the current song has finished playing
        if (!audioSource.isPlaying)
        {
            PlayNextSong();
        }
    }

    void PlayNextSong()
    {
        // Ensure we have at least one audio clip
        if (audioClips.Length == 0)
        {
            return;
        }

        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, audioClips.Length);
        } while (lastPlayedIndices.Contains(randomIndex));

        // Remove the oldest index if the list is at max count
        if (lastPlayedIndices.Count >= maxLastPlayedCount)
        {
            lastPlayedIndices.RemoveAt(0);
        }

        lastPlayedIndices.Add(randomIndex);

        audioSource.clip = audioClips[randomIndex];
        audioSource.Play();
    }
}
