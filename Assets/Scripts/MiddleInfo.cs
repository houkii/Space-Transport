using DG.Tweening;
using TMPro;
using UnityEngine;

public class MiddleInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI message;

    private Color defaultColor;
    private Color fadedColor;

    private void Awake()
    {
        defaultColor = message.color;
        fadedColor = new Color(defaultColor.r, defaultColor.g, defaultColor.b, 0);
    }

    public void Show(string info)
    {
        gameObject.SetActive(true);
        message.color = fadedColor;
        message.text = info;
        Sequence colSeq = DOTween.Sequence();
        colSeq.Append(message.DOColor(defaultColor, .45f))
            .AppendInterval(1.5f)
            .Append(message.DOColor(fadedColor, .45f))
            .AppendCallback(() => gameObject.SetActive(false));
    }
}