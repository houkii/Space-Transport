﻿using System.Collections;
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
    public enum CameraViewType { Standard, NormalLook, CloseLook, Distant, QuickDistant }
    public static Camera Cam;
    public bool IsSet { get; private set; }
    protected Sequence activeSequence;

    public void Enable(Action onCompleted = null)
    {
        IsSet = false;
        activeSequence = GetSequence().OnComplete(() => {
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

#endregion

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

#endregion

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

#endregion

#region CloseView

public class CloseView : CameraView
{
    protected override Sequence GetSequence()
    {
        var seq = DOTween.Sequence();
        seq.Append(Cam.transform.DOLocalRotateQuaternion(Quaternion.Euler(new Vector3(140, 0, 0)), 1.0f).SetEase(Ease.OutSine))
            .Join(Cam.transform.DOLocalMove(new Vector3(0, 66.5f, 70), 1.0f).SetEase(Ease.OutSine))
            .Join(Cam.DOOrthoSize(116, 2f).SetEase(Ease.OutSine));

        return seq;
    }
}

#endregion

#region DistantView

public class DistantView : CameraView
{
    public float tweenTime = 8.0f;

    public DistantView(float _time = 8.0f) { tweenTime = _time; }

    protected override Sequence GetSequence()
    {
        var seq = DOTween.Sequence();
        
        seq.Append(Cam.DOOrthoSize(1250, tweenTime).SetEase(Ease.InOutSine))
            .Join(Cam.transform.DOMove(new Vector3(0, 0, Cam.transform.position.z), tweenTime).SetEase(Ease.InOutSine));

        return seq;
    }
}

#endregion

#region CameraView Factory

public static class CameraViews
{
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
        if(finishPreviousViewTransition)
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