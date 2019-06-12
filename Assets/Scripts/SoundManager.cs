using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SoundManager : Singleton<SoundManager>
{
    private AudioSource actionAudio;
    private AudioSource musicAudio;

    [SerializeField] private AudioClip menuForwardButton;
    [SerializeField] private AudioClip menuBackButton;
    [SerializeField] private AudioClip menuTheme;
    [SerializeField] private AudioClip missionFailedTheme;
    [SerializeField] private List<AudioClip> Explosions;

    public override void Awake()
    {
        base.Awake();
        actionAudio = gameObject.AddComponent<AudioSource>();
        musicAudio = gameObject.AddComponent<AudioSource>();
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnSceneChanged(Scene current, Scene next)
    {
        if(next.name == "PlayScene")
        {
            musicAudio.Stop();
        }
        else
        {
            PlayThemeLoop();
        }
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    public void PlayForwardButton()
    {
        actionAudio.PlayOneShot(menuForwardButton);
    }

    public void PlayBackButton()
    {
        actionAudio.PlayOneShot(menuBackButton);
    }

    public void PlayThemeLoop()
    {
        musicAudio.loop = true;
        musicAudio.volume = 0;
        musicAudio.clip = menuTheme;
        musicAudio.DOFade(1, 3f).SetEase(Ease.InExpo);
        musicAudio.Play();
    }

    public void PlayMissionFailedTheme()
    {
        musicAudio.volume = 1.1f;
        musicAudio.clip = missionFailedTheme;
        musicAudio.Play();
    }

    public void PlayExplosion()
    {
        actionAudio.PlayOneShot(Explosions[UnityEngine.Random.Range(0, Explosions.Count)]);
    }
}