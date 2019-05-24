using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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
    private Sequence activeSequence;
    private bool IsInCloseView = false;

    private Sequence closeViewSequence;
    private Sequence standardViewSequence;
    private Sequence endGameViewSequence;

    private void Awake()
    {
        camera = Camera.main;
        effects = GetComponent<Effects>();
    }

    private void OnEnable()
    {
        PlayerController.Instance.OnPlayerLanded += (x) => SetCloseView();
        PlayerController.Instance.OnPlayerTookOff += (x) => SetStandardView();
        PlayerController.Instance.OnPlayerDied.AddListener(SetEndGameView);
    }

    //private void OnDisable()
    //{
    //    PlayerController.Instance.OnPlayerLanded -= (x) => SetCloseView();
    //    PlayerController.Instance.OnPlayerTookOff -= (x) => SetStandardView();
    //}

    //private void SetPosition()
    //{
    //    float angle = player.VelocityToDirectionAngle;
    //    this.angleBuffer.Add(angle - 90f);
    //    float verticalOffset = ((angleBuffer.AverageValue / 90f) * maximumVerticalOffset * player.CurrentToMaximumVelocityMagnitudeRatio);
    //    float verticalPosition = defaultCameraPosition.z - verticalOffset;
    //    Vector3 newPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, verticalPosition);
    //    transform.localPosition = Vector3.Lerp(transform.localPosition, newPosition, interpolationValue);
    //}

    private void SetPosition(Vector3 position)
    {
        transform.position = Vector3.Lerp(transform.position, new Vector3(position.x, position.y, -100f), .1f);
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
        if (!IsInCloseView && player.gameObject.activeSelf)
        {
            this.SetPosition(player.transform.position);
            this.SetSize(player.CurrentToMaximumVelocityMagnitudeRatio);
        }
    }

    private void SetCloseView()
    {
        standardViewSequence.Kill();
        transform.SetParent(player.transform);
        closeViewSequence = DOTween.Sequence();
        closeViewSequence.Append(transform.DOLocalRotateQuaternion(Quaternion.Euler(new Vector3(140, 0, 0)), 2f).SetEase(Ease.OutSine))
            .Join(transform.DOLocalMove(new Vector3(0, 20, 12), 2f).SetEase(Ease.OutSine))
            .Join(camera.DOOrthoSize(65, 2f).SetEase(Ease.OutSine));
        
        IsInCloseView = true;
    }

    private void SetStandardView()
    {
        closeViewSequence.Kill();
        standardViewSequence = DOTween.Sequence();
        standardViewSequence.Append(transform.DOLocalRotateQuaternion(Quaternion.Euler(new Vector3(90, 0, 0)), 1f).SetEase(Ease.InOutSine))
            .Join(transform.DOLocalMove(defaultCameraPosition, 1f).SetEase(Ease.InOutSine))
            .Join(camera.DOOrthoSize(80, 1f).SetEase(Ease.InOutSine))
            .AppendCallback(() => {
                angleBuffer.Clear();
                sizeBuffer.Clear();
                IsInCloseView = false;
                transform.SetParent(null);
                player.transform.SetParent(null);
            });
    }

    private void SetEndGameView()
    {
        transform.SetParent(null);
        closeViewSequence.Kill();
        endGameViewSequence = DOTween.Sequence();
        endGameViewSequence.Append(camera.DOOrthoSize(1250, 10f).SetEase(Ease.InOutSine))
            .Join(camera.transform.DOMove(new Vector3(0,0, camera.transform.position.z), 6.0f).SetEase(Ease.InOutSine));
    }

    public class CircularBuffer
    {
        private int capacity;
        private Queue<float> Values;
        public float AverageValue => AveragedValue();

        public CircularBuffer(int capacity)
        {
            this.capacity = capacity;
            Values = new Queue<float>(capacity);
        }

        private float AveragedValue()
        {
            float sum = 0f;
            foreach (float item in Values)
            {
                sum += item;
            }
            return (sum / Values.Count);
        }

        public void Add(float value)
        {
            if (Values.Count+1 > capacity)
            {
                Values.Dequeue();
            }
            Values.Enqueue(value);
        }

        public void Clear()
        {
            Values.Clear();
        }
    }
}
