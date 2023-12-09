using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAudioManager : MonoBehaviour
{
    public AudioClip[] audioClips;  // Array to store your audio clips
    public AudioClip menuSong;
    private AudioSource audioSource;
    private AudioClip currentSong;
    private float currentTime;
    private List<int> lastPlayedIndices = new List<int>();
    private const int maxLastPlayedCount = 3;  // Number of songs to remember
    public static GameAudioManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

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
        currentSong = audioClips[randomIndex];
        currentTime = 0;
        audioSource.Play();
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }

    public void PlayMenuSong()
    {
        //Store the song that was playing, play the menu song, and when the menu is exited, play the song that was playing from the same point
        audioSource.Pause();
        currentSong = audioSource.clip;
        currentTime = audioSource.time;
        audioSource.clip = menuSong;
        audioSource.Play();
    }

    public void ResumeGameSong()
    {
        audioSource.clip = currentSong;
        audioSource.time = currentTime;
        audioSource.Play();
    }
}
