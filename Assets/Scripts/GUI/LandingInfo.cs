using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;


public class LandingInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI landingText;

    private void Awake()
    {
        landingText.gameObject.SetActive(false);
    }

    public void ShowLandingInfo(LandingRewardArgs landingData)
    {
        if(landingData.Angle <= 3)
        {
            Show("Great landing!", Color.green);
        }
        else if(landingData.Angle <= 12)
        {
            Show("Good landing!", Color.white);
        }
        else if(landingData.Angle <= 40)
        {
            Show("Barely landed!", Color.red);
        }
    }

    private void Show(string text, Color color)
    {
        landingText.transform.localScale = Vector3.zero;
        landingText.gameObject.SetActive(true);
        landingText.text = text;
        landingText.color = color;
        Sequence showTextSeq = DOTween.Sequence();
        showTextSeq.Append(landingText.transform.DOScale(Vector3.one, .5f).SetEase(Ease.OutElastic))
            .AppendInterval(1.25f)
            .Append(landingText.transform.DOScale(Vector3.zero, .35f).SetEase(Ease.InBack))
            .AppendCallback(() => landingText.gameObject.SetActive(false));
    }
}
