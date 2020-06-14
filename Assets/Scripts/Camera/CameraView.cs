using DG.Tweening;
using System;
using UnityEngine;

public interface ICameraView
{
    void Enable(Action onComplete);
    void Disable();
}

public abstract class CameraView : ICameraView
{
    public enum CameraViewType { Standard, NormalLook, CloseLook, Distant, QuickDistant }

    public static Camera Cam;
    public bool IsSet { get; private set; }
    protected Sequence activeSequence;

    public void Enable(Action onCompleted = null)
    {
        IsSet = false;
        activeSequence = GetSequence().OnComplete(() =>
        {
            onCompleted?.Invoke();
            IsSet = true;
        });
    }

    public void Disable()
    {
        activeSequence.Kill();
    }

    protected abstract Sequence GetSequence();
}

public class StandardView : CameraView
{
    private Vector3 defaultCameraPosition = new Vector3(0, 60, -2);

    protected override Sequence GetSequence()
    {
        var seq = DOTween.Sequence();
        return seq;
    }
}

public class NormalView : CameraView
{
    private Vector3 defaultCameraPosition = new Vector3(0, 60, -2);

    protected override Sequence GetSequence()
    {
        var seq = DOTween.Sequence();
        seq.Append(Cam.transform.DOLocalRotateQuaternion(Quaternion.Euler(new Vector3(90, 0, 0)), 1f).SetEase(Ease.InOutSine))
            .Join(Cam.transform.DOLocalMove(defaultCameraPosition, 1f).SetEase(Ease.InOutSine))
            .Join(Cam.DOOrthoSize(100, 1f).SetEase(Ease.InOutSine));

        return seq;
    }
}

public class CloseView : CameraView
{
    protected override Sequence GetSequence()
    {
        var seq = DOTween.Sequence();
        seq.Append(Cam.transform.DOLocalRotateQuaternion(Quaternion.Euler(new Vector3(140, 0, 0)), 1.2f).SetEase(Ease.OutSine))
            .Join(Cam.transform.DOLocalMove(new Vector3(0, 66.5f, 70), 1.2f).SetEase(Ease.OutSine))
            .Join(Cam.DOOrthoSize(116, 1.2f).SetEase(Ease.OutSine));

        return seq;
    }
}

public class DistantView : CameraView
{
    public float tweenTime = 8.0f;

    public DistantView(float _time = 8.0f)
    {
        tweenTime = _time;
    }

    protected override Sequence GetSequence()
    {
        var seq = DOTween.Sequence();

        var camPos = new Vector3(0, 0, Cam.transform.position.z);
        seq.Append(Cam.DOOrthoSize(1550, tweenTime).SetEase(Ease.InOutSine))
            .Join(Cam.transform.DORotate(Vector3.zero, 1f))
            .Join(Cam.transform.DOMove(camPos, tweenTime).SetEase(Ease.InOutSine));

        return seq;
    }
}