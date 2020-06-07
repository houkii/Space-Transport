using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class ShipModelController : MonoBehaviour
{
    public enum ShipModelState { Idle, Moving, Landed }
    public ShipModelState CurrentState { get; set; }

    [SerializeField] private GameObject Thruster;
    [SerializeField] private GameObject LeftWing;
    [SerializeField] private GameObject RightWing;

    [SerializeField] private float wingsAngleMax = 25f;
    [SerializeField] private float wingsAngleMin = 0f;
    [SerializeField] private float wingsAngleLanded = 5f;
    [SerializeField] private float thrusterActivePosZ = -3.2f;
    [SerializeField] private float thrusterInactivePosZ = -2.2f;

    private Color emissionStartColor;
    private Color emissionSecondColor = Color.yellow;
    private Material shipMaterial;

    private Sequence activeSequence;
    private Sequence colorSequence;

    private void Awake()
    {
        CurrentState = ShipModelState.Idle;
        var renderer = transform.GetComponent<MeshRenderer>();
        shipMaterial = new Material(renderer.sharedMaterials[0]);
        renderer.sharedMaterials = new Material[] { shipMaterial };
        emissionStartColor = shipMaterial.GetColor("_EmissionColor");
        colorSequence = GetEmissionSequence(emissionStartColor, emissionSecondColor);
    }

    private void Update()
    {
        HandleState();
    }

    private void HandleState()
    {
        switch(CurrentState)
        {
            case ShipModelState.Idle:
                this.SetSequence(GetIdleSequence());
                break;

            case ShipModelState.Moving:
                this.SetSequence(GetMovingSequence());
                break;

            case ShipModelState.Landed:
                this.SetSequence(GetLandingSequence());
                break;

            default:
                break;
        }
    }

    private void SetSequence(Sequence seq)
    {
        activeSequence.Kill();
        activeSequence = seq;
    }

    private Sequence GetMovingSequence()
    {
        Sequence seq = DOTween.Sequence();

        seq.Append(RightWing.transform.DOLocalRotateQuaternion(Quaternion.Euler(new Vector3(0, -wingsAngleMax, 0f)), 1.0f).SetEase(Ease.OutExpo))
            .Join(LeftWing.transform.DOLocalRotateQuaternion(Quaternion.Euler(new Vector3(0, wingsAngleMax, -180f)), 1.0f).SetEase(Ease.OutExpo))
            .Join(Thruster.transform.DOLocalMoveZ(thrusterActivePosZ, 1.0f).SetEase(Ease.OutExpo));

        return seq;
    }

    private Sequence GetIdleSequence()
    {
        Sequence seq = DOTween.Sequence();

        seq.Append(RightWing.transform.DOLocalRotateQuaternion(Quaternion.Euler(new Vector3(0, -wingsAngleMin, 0f)), 1.0f).SetEase(Ease.OutExpo))
            .Join(LeftWing.transform.DOLocalRotateQuaternion(Quaternion.Euler(new Vector3(0, wingsAngleMin, -180f)), 1.0f).SetEase(Ease.OutExpo))
            .Join(Thruster.transform.DOLocalMoveZ(thrusterInactivePosZ, 0.9f).SetEase(Ease.OutExpo));

        return seq;
    }

    private Sequence GetLandingSequence()
    {
        Sequence seq = DOTween.Sequence();

        seq.Append(RightWing.transform.DOLocalRotateQuaternion(Quaternion.Euler(new Vector3(0, -wingsAngleLanded, 0f)), 1.0f).SetEase(Ease.OutExpo))
            .Join(LeftWing.transform.DOLocalRotateQuaternion(Quaternion.Euler(new Vector3(0, wingsAngleLanded, -180f)), 1.0f).SetEase(Ease.OutExpo))
            .Join(Thruster.transform.DOLocalMoveZ(-2f, 1.0f).SetEase(Ease.OutExpo));

        return seq;
    }

    private Sequence GetEmissionSequence(Color from, Color to)
    {
        Sequence seq = DOTween.Sequence();
        Vector4 startColor = new Vector4(from.r, from.g, from.b, from.a);
        Vector4 endColor = new Vector4(to.r, to.g, to.b, to.a);

        seq.Append(shipMaterial.DOVector(endColor, "_EmissionColor", 1.0f).SetEase(Ease.OutBack))
            .Append(shipMaterial.DOVector(startColor, "_EmissionColor", 1.0f).SetEase(Ease.OutBack))
            .SetLoops(Int32.MaxValue, LoopType.Yoyo);

        return seq;
    }
}
