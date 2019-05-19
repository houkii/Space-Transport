using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MinimapElement : MonoBehaviour
{
    public bool isPingingConstantly = true;
    public float pingScale = 3f;
    public float pingTime = 3f;

    private Vector3 defaultScale;

    private void Awake()
    {
        this.defaultScale = this.transform.localScale;

        if (this.isPingingConstantly)
        {
            this.MainSequence();
        }
        else
        {
            OnShow(this);
        }
    }

    private Sequence MainSequence()
    {
        Sequence miniMapElementSequence = DOTween.Sequence();
        miniMapElementSequence.Append(OnShow(this)).
            Append(Ping(this, pingScale, pingTime).SetLoops(System.Int32.MaxValue, LoopType.Yoyo));

        return miniMapElementSequence;
    }

    private static Tween OnShow(MinimapElement target)
    {
        target.transform.localScale = target.defaultScale * 5;
        Tween entryTween = target.transform.DOScale(target.defaultScale, 1.5f).SetEase(Ease.InOutCubic);
        return entryTween;
    }

    private static Tween Ping(MinimapElement target, float scale, float time)
    {
        Tween pingTween = target.transform.DOScale(target.defaultScale * scale, time/2).SetEase(Ease.InExpo).SetLoops(1, LoopType.Yoyo);
        return pingTween;
    }
}
