using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MovableCanvasElement : MonoBehaviour
{
    private Vector2 activePosition;
    [SerializeField] private Vector2 inactivePosition;
    [SerializeField] private float tweenTime = .75f;
    [SerializeField] private Ease showEase = Ease.OutBack;
    [SerializeField] private Ease hideEase = Ease.InBack;
    [SerializeField] private bool activeOnStart = true;

    protected RectTransform RT;

    protected virtual void Awake()
    {
        this.Initialize();
    }

    public virtual Sequence Show()
    {
        gameObject.SetActive(true);
        Sequence showSequence = DOTween.Sequence();
        showSequence.Append(RT.DOAnchorPos(activePosition, tweenTime).SetEase(showEase));
        return showSequence;
    }

    public virtual Sequence Hide()
    {
        Sequence hideSequence = DOTween.Sequence();
        hideSequence.Append(RT.DOAnchorPos(inactivePosition, tweenTime).SetEase(hideEase)
            .OnComplete(() => gameObject.SetActive(false)));
        return hideSequence;
    }

    protected void Initialize()
    {
        RT = GetComponent<RectTransform>();
        activePosition = RT.anchoredPosition;
        RT.anchoredPosition = inactivePosition;
        if (activeOnStart)
        {
            this.Show();
        }
    }
}
