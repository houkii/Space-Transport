using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

public class MiddleInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI message;

    private Color defaultColor;
    private Color fadedColor;
    private Sequence colSeq;

    private void Awake()
    {
        defaultColor = message.color;
        fadedColor = new Color(defaultColor.r, defaultColor.g, defaultColor.b, 0);
    }

    public void Show(string info, Action onShown = null)
    {
        colSeq.Kill();
        gameObject.SetActive(true);
        message.color = fadedColor;
        message.text = info;
        colSeq = DOTween.Sequence();
        colSeq.Append(message.DOColor(defaultColor, .45f))
            .AppendInterval(1.5f)
            .Append(message.DOColor(fadedColor, .45f))
            .AppendCallback(() =>
            {
                onShown?.Invoke();
                gameObject.SetActive(false);
            });
    }
}