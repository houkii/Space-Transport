using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MovableCanvasElement : MonoBehaviour
{
    private Vector2 activePosition;
    [SerializeField]
    private Vector2 inactivePosition;
    [SerializeField]
    private float tweenTime = .75f;
    [SerializeField]
    private Ease showEase = Ease.OutBack;
    [SerializeField]
    private Ease hideEase = Ease.InBack;
    [SerializeField]
    private bool activeOnStart = true;

    private RectTransform RT;

    private void Awake()
    {
        RT = GetComponent<RectTransform>();
        activePosition = RT.anchoredPosition;
        RT.anchoredPosition = inactivePosition;
        if (activeOnStart)
        {
            this.Show();
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        RT.DOAnchorPos(activePosition, tweenTime)
            .SetEase(showEase);
    }

    public void Hide()
    {
        RT.DOAnchorPos(inactivePosition, tweenTime)
            .SetEase(hideEase)
            .OnComplete(() => gameObject.SetActive(false));
    }
}
