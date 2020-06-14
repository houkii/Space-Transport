using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;


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