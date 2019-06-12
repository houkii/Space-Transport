using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIRewardEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rewardName;
    [SerializeField] private TextMeshProUGUI rewardValue;

    private RectTransform rt;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Show(string name, int value)
    {
        gameObject.SetActive(true);
        rewardName.text = name + ":";
        rewardValue.text = "+"  + value.ToString();
        SetRandomColor();
        rt = transform.GetComponent<RectTransform>();

        transform.localScale = Vector3.zero;
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(Vector3.one, .4f).SetEase(Ease.OutBounce))
            .Append(rt.DOAnchorPosY(rt.anchoredPosition.y + 40f, 1.5f).SetEase(Ease.InBack))
            .Join(rewardName.DOFade(0, 2).SetEase(Ease.InExpo))
            .Join(rewardValue.DOFade(0, 2).SetEase(Ease.InExpo))
            .Join(transform.DOScale(Vector3.zero, 2.25f).SetEase(Ease.InExpo))
            .AppendCallback(() => Destroy(gameObject));
    }

    private void SetRandomColor()
    {
        rewardName.color = GameController.Instance.ColorPaletteDarkPink.RandomColor;
        rewardValue.color = GameController.Instance.ColorPaletteLightPink.RandomColor;
    }
}
