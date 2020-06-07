using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MainMenuCameraController : MonoBehaviour
{
    private Effects fx;

    private void Awake()
    {
        fx = GetComponent<Effects>();
        fx.intensity = 0.035f;
        DOTween.To(() => fx.intensity, x => fx.intensity = x, 0.002f, 2f).SetEase(Ease.OutElastic);
    }

    void Update()
    {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * 0.75f);
    }
}
