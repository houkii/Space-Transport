using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System;
using UnityEngine;

#region CameraView

public interface ICameraView
{
    void Enable(Action onComplete);
    void Disable();
}

public abstract class CameraView : ICameraView
{
    public enum CameraViewType { Standard, CloseLook, Distant }
    public static Camera Cam;
    protected Sequence activeSequence;

    public void Enable(Action onCompleted = null)
    {
        activeSequence = GetSequence().OnComplete(() => onCompleted?.Invoke());
    }

    public void Disable()
    {
        activeSequence.Kill();
    }

    protected abstract Sequence GetSequence();
}

#endregion

#region StandardView

public class StandardView : CameraView
{
    private Vector3 defaultCameraPosition = new Vector3(0, 20, 2);

    protected override Sequence GetSequence()
    {
        var seq = DOTween.Sequence();
        seq.Append(Cam.transform.DOLocalRotateQuaternion(Quaternion.Euler(new Vector3(90, 0, 0)), 1f).SetEase(Ease.InOutSine))
            .Join(Cam.transform.DOLocalMove(defaultCameraPosition, 1f).SetEase(Ease.InOutSine))
            .Join(Cam.DOOrthoSize(80, 1f).SetEase(Ease.InOutSine));

        return seq;
    }
}

#endregion

#region CloseView

public class CloseView : CameraView
{
    protected override Sequence GetSequence()
    {
        var seq = DOTween.Sequence();
        seq.Append(Cam.transform.DOLocalRotateQuaternion(Quaternion.Euler(new Vector3(140, 0, 0)), 2f).SetEase(Ease.OutSine))
            .Join(Cam.transform.DOLocalMove(new Vector3(0, 20, 12), 2f).SetEase(Ease.OutSine))
            .Join(Cam.DOOrthoSize(65, 2f).SetEase(Ease.OutSine));

        return seq;
    }
}

#endregion

#region DistantView

public class DistantView : CameraView
{
    protected override Sequence GetSequence()
    {
        var seq = DOTween.Sequence();
        seq.Append(Cam.DOOrthoSize(1250, 10f).SetEase(Ease.InOutSine))
            .Join(Cam.transform.DOMove(new Vector3(0, 0, Cam.transform.position.z), 6.0f).SetEase(Ease.InOutSine));

        return seq;
    }
}

#endregion

#region CameraView Factory

public static class CameraViews
{
    public static CameraView ActiveView { get; private set; }

    public static void SetActive(CameraView.CameraViewType cameraViewType, Action onViewSetupCompleted = null)
    {
        switch (cameraViewType)
        {
            case CameraView.CameraViewType.Standard:
                SetActiveCameraView(new StandardView(), onViewSetupCompleted); break;
            case CameraView.CameraViewType.CloseLook:
                SetActiveCameraView(new CloseView(), onViewSetupCompleted); break;
            case CameraView.CameraViewType.Distant:
                SetActiveCameraView(new DistantView(), onViewSetupCompleted); break;
            default:
                SetActiveCameraView(new StandardView(), onViewSetupCompleted); break;
        }
    }

    private static void SetActiveCameraView(CameraView view, Action onViewSetupCompleted = null)
    {
        if (ActiveView != null)
        {
            ActiveView.Disable();
            GC.SuppressFinalize((object)ActiveView);

            if (ActiveView.GetType() != view.GetType())
            {
                ActiveView = view;
                ActiveView.Enable(onViewSetupCompleted);
            }
        }
        else
        {
            ActiveView = view;
            ActiveView.Enable(onViewSetupCompleted);
        }
    }

    public static void Initialize()
    {
        ActiveView = new StandardView();
    }
}

#endregion