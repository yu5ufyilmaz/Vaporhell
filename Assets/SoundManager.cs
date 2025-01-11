using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance; // Singleton pattern için

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioClip bgMusic;

    [Header("Volume Settings")]
    [Range(0, 1)] public float musicVolume = 0.5f;
    [Range(0, 1)] public float sfxVolume = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Sahne değişse bile yok olmasın
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayBackgroundMusic();
    }

    public void PlayBackgroundMusic()
    {
        if (musicSource != null && bgMusic != null)
        {
            musicSource.clip = bgMusic;
            musicSource.volume = musicVolume;
            musicSource.loop = true; // Sürekli çal
            musicSource.Play();
        }
    }
    public void PlayMusic(AudioClip clip)
    {
        if (musicSource != null)
        {
            musicSource.clip = clip;
            musicSource.volume = musicVolume;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
    }
}