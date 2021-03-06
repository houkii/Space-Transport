﻿using UnityEngine;
using UnityEngine.Events;

public class CameraController : Singleton<CameraController>
{
    public UnityEvent OnStandardViewSet;
    public Camera RadarCamera;
    public CameraViews CameraViews { get; private set; }

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
    private MinimapElement playerMinimapView;
    private CameraView previousView;

    public float currentToMaxCameraSizeRation => camera.orthographicSize / maxCameraSize;

    public override void Awake()
    {
        base.Awake();
        camera = Camera.main;
        CameraViews = new CameraViews();
        CameraViews.Initialize(camera);
        effects = GetComponent<Effects>();
        var playerPos = PlayerController.Instance.transform.position;
        transform.position = new Vector3(playerPos.x, playerPos.y, transform.position.z);
        playerMinimapView = PlayerController.Instance.gameObject.transform.GetComponentInChildren<MinimapElement>();
    }

    public void OnEnable()
    {
        RegisterCallbacks();
    }

    public void ShowQuickDistantView()
    {
        transform.parent = null;
        transform.rotation = Quaternion.Euler(Vector3.zero);
        previousView = CameraViews.ActiveView;
        CameraViews.SetActive(CameraView.CameraViewType.QuickDistant);
    }

    public void ShowPreviousView()
    {
        if (previousView is CloseView)
        {
            transform.parent = PlayerController.Instance.transform;
            CameraViews.SetActive(CameraView.CameraViewType.Close);
        }
        else
        {
            CameraViews.SetActive(CameraView.CameraViewType.Standard);
        }
    }

    public void ToggleView()
    {
        if (CameraViews.ActiveView is DistantView)
        {
            ShowPreviousView();
        }
        else
        {
            ShowQuickDistantView();
        }
    }

    public bool ToggleRadar()
    {
        if (RadarCamera.transform.parent == null)
        {
            RadarCamera.transform.SetParent(PlayerController.Instance.transform);
            RadarCamera.transform.localPosition = new Vector3(0, 300, 0);
            RadarCamera.transform.rotation = Quaternion.identity;
            RadarCamera.orthographicSize = 1800;
            playerMinimapView.transform.parent.transform.localScale = GameController.Instance.Settings.PlayerScaleMiniMap1;

            return true;
        }
        else
        {
            RadarCamera.transform.SetParent(null);
            RadarCamera.transform.position = new Vector3(0, 0, -1500);
            RadarCamera.transform.rotation = Quaternion.identity;
            RadarCamera.orthographicSize = 3500;
            playerMinimapView.transform.parent.transform.localScale = GameController.Instance.Settings.PlayerScaleMiniMap2;
            return false;
        }
    }

    public void SetNormalLook()
    {
        CameraViews.SetActive(CameraView.CameraViewType.Normal, SetStandardViewParams);
    }

    private void SetPosition(Vector3 position)
    {
        transform.position = Vector3.Lerp(transform.position, new Vector3(position.x, position.y, -300f), .1f);
    }

    private void SetSize(float ratio)
    {
        float size = minCameraSize + (maxCameraSize - minCameraSize) * ratio;
        sizeBuffer.Add(size);
        camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, sizeBuffer.AverageValue, interpolationValue);
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
            SetPosition(player.transform.position);
            SetSize(player.CurrentToMaximumVelocityMagnitudeRatio);
        }
    }

    private void SetStandardViewParams()
    {
        angleBuffer.Clear();
        sizeBuffer.Clear();
        transform.SetParent(null);
        player.transform.SetParent(null);
        CameraViews.SetActive(CameraView.CameraViewType.Standard);
        OnStandardViewSet?.Invoke();
    }

    private void RegisterCallbacks()
    {
        PlayerController.Instance.OnPlayerLanded += (x) =>
        {
            transform.SetParent(player.transform);
            CameraViews.SetActive(CameraView.CameraViewType.Close);
        };

        PlayerController.Instance.OnPlayerTookOff += (x) =>
        {
            CameraViews.SetActive(CameraView.CameraViewType.Normal, SetStandardViewParams);
        };

        PlayerController.Instance.OnPlayerDied.AddListener(() =>
        {
            if (transform.parent != null)
                transform.SetParent(null);

            CameraViews.SetActive(CameraView.CameraViewType.Distant);
        });
    }
}