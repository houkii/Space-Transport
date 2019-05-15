using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class SpeechBubble : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI Text;
    private Image Image;
    private Sequence activeSequence;

    void Awake()
    {
        Text = GetComponentInChildren<TextMeshProUGUI>();
        Image = GetComponent<Image>();
        gameObject.SetActive(false);
    }

    public void ShowInfo(string message)
    {
        if (gameObject.activeSelf)
        {
            activeSequence.Kill();
        }
        else
        {
            gameObject.SetActive(true);
        }

        Text.text = message;
        transform.localScale = Vector3.zero;
        activeSequence = this.GetInfoSequence();
    }

    public void Hide()
    {
        activeSequence.Kill();
        gameObject.SetActive(false);
        Text.text = string.Empty;
    }

    private Sequence GetInfoSequence()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(1, .75f).SetEase(Ease.OutElastic)).
                AppendInterval(4f).
                Append(transform.DOScale(0, .65f).SetEase(Ease.InElastic)).
                OnComplete(() => this.Hide());

        return seq;
    }
}
