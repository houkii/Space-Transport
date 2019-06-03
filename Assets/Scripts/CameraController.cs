using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class CameraController : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private float minCameraSize = 120f;
    [SerializeField] private float maxCameraSize = 150f;
    [SerializeField] private Vector3 defaultCameraPosition = new Vector3(0, 20, 4);
    [SerializeField] private float maximumVerticalOffset = 9f;
    [SerializeField] private float interpolationValue = 0.05f;
    [SerializeField] private float skyboxRotateSpeed = 1.5f;

    private Camera camera;
    private Effects effects;
    private CircularBuffer angleBuffer = new CircularBuffer(50);
    private CircularBuffer sizeBuffer = new CircularBuffer(50);

    private void Awake()
    {
        camera = Camera.main;
        CameraViews.Initialize();
        CameraView.Cam = camera;
        effects = GetComponent<Effects>();
    }

    private void OnEnable()
    {
        this.RegisterCallbacks();
    }

    private void SetPosition(Vector3 position)
    {
        transform.position = Vector3.Lerp(transform.position, new Vector3(position.x, position.y, -200f), .1f);
    }

    private void SetSize(float ratio)
    {
        float size = minCameraSize + (maxCameraSize - minCameraSize) * ratio;
        sizeBuffer.Add(size);
        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, sizeBuffer.AverageValue, interpolationValue);
    }

    private void SetRotation()
    {

    }

    private void Update()
    { 
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * skyboxRotateSpeed);
        effects.intensity = player.CurrentToMaximumVelocityMagnitudeRatio / 250;
    }

    private void FixedUpdate()
    {
        if (CameraViews.ActiveView is StandardView)
        {
            this.SetPosition(player.transform.position);
            this.SetSize(player.CurrentToMaximumVelocityMagnitudeRatio);
        }
    }

    private void SetStandardViewParams()
    {
        angleBuffer.Clear();
        sizeBuffer.Clear();
        transform.SetParent(null);
        player.transform.SetParent(null);
        CameraViews.SetActive(CameraView.CameraViewType.Standard);
    }

    private void RegisterCallbacks()
    {
        PlayerController.Instance.OnPlayerLanded += (x) =>
        {
            transform.SetParent(player.transform);
            CameraViews.SetActive(CameraView.CameraViewType.CloseLook);
        };

        PlayerController.Instance.OnPlayerTookOff += (x) =>
        {
            CameraViews.SetActive(CameraView.CameraViewType.NormalLook, SetStandardViewParams);
        };

        PlayerController.Instance.OnPlayerDied.AddListener(() =>
        {
            if (transform.parent != null)
                transform.SetParent(null);

            CameraViews.SetActive(CameraView.CameraViewType.Distant);
        });

        //GameController.Instance.MissionController.OnMissionCompleted.AddListener(() =>
        //{
        //    if (transform.parent != null)
        //        transform.SetParent(null);

        //    CameraViews.SetActive(CameraView.CameraViewType.Distant, null, true);
        //});
    }

    CameraView previousView;

    public void ShowQuickDistantView()
    {
        PlaySceneCanvasController.Instance.HideIndicators();
        transform.rotation = Quaternion.Euler(Vector3.zero);
        previousView = CameraViews.ActiveView;
        CameraViews.SetActive(CameraView.CameraViewType.QuickDistant);
    }

    public void ShowPreviousView()
    {
        PlaySceneCanvasController.Instance.ShowIndicators();
        if(previousView is CloseView)
        {
            CameraViews.SetActive(CameraView.CameraViewType.CloseLook);
        }
        else
        {
            CameraViews.SetActive(CameraView.CameraViewType.Standard);
        }
    }
}