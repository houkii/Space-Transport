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

public static class VFX
{
    public static void PlayTeleportEffect(Transform transform)
    {
        Rigidbody[] rbs = transform.GetComponentsInChildren<Rigidbody>();
        Collider[] colls = transform.GetComponentsInChildren<Collider>();

        foreach (var rb in rbs)
        {
            rb.isKinematic = true;
        }

        foreach (var coll in colls)
        {
            coll.enabled = false;
        }

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(0, 1f).SetEase(Ease.OutSine));
    }
}