using DG.Tweening;
using System;
using UnityEngine;

[Serializable]
public class ShipSounds
{
    public AudioClip StartEngine;
    public AudioClip Running;
    public AudioClip StopEngine;
    public AudioClip LandingSound;
    public AudioClip GetOnBoard;
    public AudioClip DestinationReached;
    public float engineStartPitch = 0.75f;
    public float engineEndPitch = 1.1f;
    public float tweenTime = 4f;
    public float RandomPitch => UnityEngine.Random.Range(-.05f, .05f);
    public Tween pitchTween;
}