using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlatformController : MonoBehaviour
{
    [SerializeField] private Color baseColor = Color.red;
    [SerializeField] private float floor = -2.2f;
    [SerializeField] private float ceiling = 3f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private Vector3 bottomLocalPosition;

    private Renderer renderer;
    private Material mat;
    private Color finalColor;
    private float emission;
    private PlanetController parentPlanetController;
    private Vector3 defaultLocalPos;
    private Sequence activeSequence;

    private void Awake()
    {
        this.InitializeMaterial();
        parentPlanetController = GetComponentInParent<PlanetController>();
        defaultLocalPos = transform.localPosition;
        this.RegisterPlayerCallbacks();
    }

    private void Update()
    {
        SetEmission();
    }

    private void InitializeMaterial()
    {
        renderer = GetComponent<Renderer>();
        var platformMat = renderer.sharedMaterials[0];
        mat = new Material(platformMat);
        renderer.sharedMaterials = new Material[] { mat };
    }

    private void SetEmission()
    {
        emission = floor + Mathf.PingPong(Time.time * speed, ceiling - floor);
        finalColor = baseColor * Mathf.LinearToGammaSpace(emission);
        mat.SetColor("_EmissionColor", finalColor);
    }

    private void MoveDown()
    {
        activeSequence.Kill();
        activeSequence = DOTween.Sequence();
        activeSequence.Append(transform.DOLocalMove(bottomLocalPosition, 2.0f))
            .AppendCallback(() =>
            {
                baseColor = Color.green;
            });
    }

    private void MoveUp()
    {
        activeSequence.Kill();
        activeSequence = DOTween.Sequence();
        activeSequence.Append(transform.DOLocalMove(defaultLocalPos, 3.0f))
            .AppendCallback(() =>
            {
                baseColor = Color.red;
            });
    }

    private void RegisterPlayerCallbacks()
    {
        PlayerController.Instance.OnPlayerLanded += (planet) =>
        {
            if (planet == parentPlanetController)
            {
                baseColor = Color.green;
                //this.MoveDown();
            }
        };

        PlayerController.Instance.OnPlayerTookOff += (planet) =>
        {
            if (planet == parentPlanetController)
            {
                baseColor = Color.red;
                //this.MoveUp();
            }
        };
    }
}
