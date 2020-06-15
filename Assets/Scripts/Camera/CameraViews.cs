using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;


public class CameraViews
{
    public delegate void CameraViewChangedDelegate(CameraView view);
    public CameraViewChangedDelegate OnCameraViewChanged;
    public CameraView ActiveView { get; private set; }

    public void Initialize(Camera camera)
    {
        ActiveView = new StandardView();
        CameraView.Cam = camera;
    }

    public void SetActive(CameraView.CameraViewType cameraViewType, Action onViewSetupCompleted = null, bool finishPreviousViewTransition = false)
    {
        GameController.Instance.StartCoroutine(SetCameraViewType(cameraViewType, onViewSetupCompleted, finishPreviousViewTransition));
    }

    public void SetInstantCameraViewType(CameraView.CameraViewType cameraViewType)
    {
        switch (cameraViewType)
        {
            case CameraView.CameraViewType.Close:
                ActiveView = new CloseView(); break;
            default:
                ActiveView = new StandardView(); break;
        }
    }

    private IEnumerator SetCameraViewType(CameraView.CameraViewType cameraViewType, Action onViewSetupCompleted = null, bool finishPreviousViewTransition = false)
    {
        if (finishPreviousViewTransition)
            yield return new WaitUntil(() => ActiveView.IsSet == true);

        switch (cameraViewType)
        {
            case CameraView.CameraViewType.Standard:
                SetActiveCameraView(new StandardView(), onViewSetupCompleted); break;
            case CameraView.CameraViewType.Normal:
                SetActiveCameraView(new NormalView(), onViewSetupCompleted); break;
            case CameraView.CameraViewType.Close:
                SetActiveCameraView(new CloseView(), onViewSetupCompleted); break;
            case CameraView.CameraViewType.Distant:
                SetActiveCameraView(new DistantView(), onViewSetupCompleted); break;
            case CameraView.CameraViewType.QuickDistant:
                SetActiveCameraView(new DistantView(.45f), onViewSetupCompleted); break;
            default:
                SetActiveCameraView(new StandardView(), onViewSetupCompleted); break;
        }
    }

    private void SetActiveCameraView(CameraView view, Action onViewSetupCompleted = null)
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


}