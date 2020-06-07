using DG.Tweening;
using System;
using UnityEngine;

[Serializable]
public class PlayerEffects
{
    public GameObject LandingFX;
    public GameObject FuelLoadingFX;
    public GameObject TravellerEnterFX;
    public GameObject ExplosionFX;
    public GameObject BlackHoleFX;

    private GameObject fuelLoadingFX;

    public void ShowLandingFX(Vector3 position, Quaternion rotation)
    {
        var obj = GameObject.Instantiate(LandingFX, position, rotation);
        obj.transform.SetParent(PlayerController.Instance.transform);
        GameObject.Destroy(obj, 2.5f);
    }

    public void ShowTravellerEnterFX(Vector3 position, Quaternion rotation)
    {
        var obj = GameObject.Instantiate(TravellerEnterFX, position, rotation);
        obj.transform.SetParent(PlayerController.Instance.transform);
        GameObject.Destroy(obj, 1f);
    }

    public void ShowFuelLoading(Vector3 position, Quaternion rotation)
    {
        if (fuelLoadingFX == null)
        {
            fuelLoadingFX = GameObject.Instantiate(FuelLoadingFX, position, rotation);
            fuelLoadingFX.transform.SetParent(PlayerController.Instance.transform);
        }
        else
        {
            fuelLoadingFX.SetActive(true);
            fuelLoadingFX.transform.position = position;
            fuelLoadingFX.transform.rotation = rotation;
        }
    }

    public void HideFuelLoading()
    {
        fuelLoadingFX.SetActive(false);
    }

    public void ShowExplosion(Vector3 position, Quaternion rotation)
    {
        var obj = GameObject.Instantiate(ExplosionFX, position, rotation);
        GameObject.Destroy(obj, 2.5f);
    }

    public void PlayIntroSequence()
    {
        var player = PlayerController.Instance.transform;
        var defaultPlayerScale = PlayerController.Instance.DefaultPlayerScale;
        var obj = GameObject.Instantiate(BlackHoleFX, player.position + new Vector3(0, 0, 75f), player.rotation);
        obj.transform.localScale = Vector3.zero;
        player.localScale = Vector3.zero;

        Sequence BHSeq = DOTween.Sequence();
        BHSeq.Append(obj.transform.DOScale(220, .75f).SetEase(Ease.OutBack))
            .AppendInterval(.35f)
            .Append(obj.transform.DOScale(0, .45f).SetEase(Ease.InBack))
            .AppendCallback(() => GameObject.Destroy(obj));

        Sequence playerSeq = DOTween.Sequence();
        playerSeq.AppendInterval(1.2f)
            .Append(player.DOScale(defaultPlayerScale.x, 1f).SetEase(Ease.OutElastic))
            .Join(Camera.main.transform.DOShakeRotation(1f, 1.25f));
    }

    public void PlayOutroSequence()
    {
        var player = PlayerController.Instance.transform;

        Rigidbody[] rbs = player.GetComponentsInChildren<Rigidbody>();
        Collider[] colls = player.GetComponentsInChildren<Collider>();
        var attractor = player.GetComponent<Attractor>();
        attractor.isAffectedByPull = false;
        attractor.isPulling = false;

        foreach (var rb in rbs)
        {
            rb.isKinematic = true;
        }

        foreach (var coll in colls)
        {
            coll.enabled = false;
        }

        var scaler = player.transform.localScale.x * player.transform.parent.transform.localScale.x;
        var obj = GameObject.Instantiate(BlackHoleFX, player.position + new Vector3(0, 0, 25f), player.rotation);

        obj.transform.SetParent(player);
        obj.transform.localScale = Vector3.zero;
        CameraViews.ActiveView.Disable();
        Camera.main.transform.SetParent(null);

        Sequence BHSeq = DOTween.Sequence();
        BHSeq.Append(obj.transform.DOScale(220 / scaler, 1f).SetEase(Ease.OutBack))
            .AppendInterval(.35f)
            .Append(obj.transform.DOScale(0, .45f).SetEase(Ease.InBack))
            .AppendCallback(() => GameObject.Destroy(obj));

        Sequence playerSeq = DOTween.Sequence();
        playerSeq.AppendInterval(.6f)
            .Append(player.DOScale(0, 1f).SetEase(Ease.InElastic))
            .Join(Camera.main.transform.DOShakeRotation(1f, 1.25f));
    }

    public void PlayLostFuelSequence()
    {
        Sequence lostFuelSeq = DOTween.Sequence();
        lostFuelSeq.Append(Camera.main.transform.DOShakeRotation(1.25f, 3))
        .AppendInterval(1f)
        .AppendCallback(() => PlayerController.Instance.Kill());
    }
}