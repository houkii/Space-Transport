using UnityEngine;
using UnityEngine.Events;

public class CameraController : Singleton<CameraController>
{
    [SerializeField] private PlayerController player;
    [SerializeField] private float minCameraSize = 120f;
    [SerializeField] private float maxCameraSize = 150f;
    [SerializeField] private Vector3 defaultCameraPosition = new Vector3(0, 20, 4);
    [SerializeField] private float maximumVerticalOffset = 9f;
    [SerializeField] private float interpolationValue = 0.05f;
    [SerializeField] private float skyboxRotateSpeed = 1.5f;

    public UnityEvent OnStandardViewSet;
    public Camera RadarCamera;

    private Camera camera;
    private Effects effects;
    private CircularBuffer angleBuffer = new CircularBuffer(50);
    private CircularBuffer sizeBuffer = new CircularBuffer(50);

    public float currentToMaxCameraSizeRation => camera.orthographicSize / maxCameraSize;

    public override void Awake()
    {
        base.Awake();
        camera = Camera.main;
        CameraViews.Initialize();
        CameraView.Cam = camera;
        effects = GetComponent<Effects>();
        var playerPos = PlayerController.Instance.transform.position;
        transform.position = new Vector3(playerPos.x, playerPos.y, transform.position.z);
    }

    private void OnEnable()
    {
        RegisterCallbacks();
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

    public void NormalLook()
    {
        CameraViews.SetActive(CameraView.CameraViewType.NormalLook, SetStandardViewParams);
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
    }

    private CameraView previousView;

    public void ShowQuickDistantView()
    {
        //PlaySceneCanvasController.Instance.HideIndicators();
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
            CameraViews.SetActive(CameraView.CameraViewType.CloseLook);
        }
        else
        {
            //PlaySceneCanvasController.Instance.ShowIndicators();
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
            return true;
        }
        else
        {
            RadarCamera.transform.SetParent(null);
            RadarCamera.transform.position = new Vector3(0, 0, -1500);
            RadarCamera.transform.rotation = Quaternion.identity;
            RadarCamera.orthographicSize = 3500;
            return false;
        }
    }
}