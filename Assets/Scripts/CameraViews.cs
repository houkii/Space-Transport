using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

#region CameraView

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

#endregion CameraView

#region StandardView

public class StandardView : CameraView
{
    private Vector3 defaultCameraPosition = new Vector3(0, 60, -2);

    protected override Sequence GetSequence()
    {
        var seq = DOTween.Sequence();
        return seq;
    }
}

#endregion StandardView

#region Normal

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

#endregion Normal

#region CloseView

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

#endregion CloseView

#region DistantView

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

#endregion DistantView

#region CameraView Factory

public static class CameraViews
{
    public delegate void CameraViewChangedDelegate(CameraView view);

    public static CameraViewChangedDelegate OnCameraViewChanged;

    public static CameraView ActiveView { get; private set; }

    public static void SetActive(CameraView.CameraViewType cameraViewType, Action onViewSetupCompleted = null, bool finishPreviousViewTransition = false)
    {
        GameController.Instance.StartCoroutine(SetCameraViewType(cameraViewType, onViewSetupCompleted, finishPreviousViewTransition));
    }

    public static void SetInstantCameraViewType(CameraView.CameraViewType cameraViewType)
    {
        switch (cameraViewType)
        {
            case CameraView.CameraViewType.CloseLook:
                ActiveView = new CloseView(); break;
            default:
                ActiveView = new StandardView(); break;
        }
    }

    private static IEnumerator SetCameraViewType(CameraView.CameraViewType cameraViewType, Action onViewSetupCompleted = null, bool finishPreviousViewTransition = false)
    {
        if (finishPreviousViewTransition)
            yield return new WaitUntil(() => ActiveView.IsSet == true);

        switch (cameraViewType)
        {
            case CameraView.CameraViewType.Standard:
                SetActiveCameraView(new StandardView(), onViewSetupCompleted); break;
            case CameraView.CameraViewType.NormalLook:
                SetActiveCameraView(new NormalView(), onViewSetupCompleted); break;
            case CameraView.CameraViewType.CloseLook:
                SetActiveCameraView(new CloseView(), onViewSetupCompleted); break;
            case CameraView.CameraViewType.Distant:
                SetActiveCameraView(new DistantView(), onViewSetupCompleted); break;
            case CameraView.CameraViewType.QuickDistant:
                SetActiveCameraView(new DistantView(.45f), onViewSetupCompleted); break;
            default:
                SetActiveCameraView(new StandardView(), onViewSetupCompleted); break;
        }
    }

    private static void SetActiveCameraView(CameraView view, Action onViewSetupCompleted = null)
    {
        if (ActiveView != null)
        {
            if (ActiveView.GetType() != view.GetType())
            {
                ActiveView.Disable();
                GC.SuppressFinalize((object)ActiveView);
                ActiveView = view;
                ActiveView.Enable(onViewSetupCompleted);
                OnCameraViewChanged?.Invoke(view);
            }
        }
        else
        {
            ActiveView = view;
            ActiveView.Enable(onViewSetupCompleted);
            OnCameraViewChanged?.Invoke(view);
        }
    }

    public static void Initialize()
    {
        ActiveView = new StandardView();
    }
}

#endregion CameraView Factory