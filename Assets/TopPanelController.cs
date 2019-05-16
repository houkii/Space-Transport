using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

public class TopPanelController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI ScoreValue;
    private Sequence updateScoreSequence;
    private int CurrentScore
    {
        set
        {
            ScoreValue.text = value.ToString();
        }
    }

    private void OnEnable()
    {
        PlayerController.Instance.Stats.OnScoreUpdated += UpdateScore;
    }

    private void OnDisable()
    {
        PlayerController.Instance.Stats.OnScoreUpdated -= UpdateScore;
    }

    private void UpdateScore(int value)
    {
        int currentScore = Int32.Parse(ScoreValue.text);
        ScoreValue.transform.localScale = Vector3.one;
        updateScoreSequence.Kill();
        updateScoreSequence = DOTween.Sequence();
        updateScoreSequence.Append(DOTween.To(() => currentScore, x => CurrentScore = x, value, 1.0f).SetEase(Ease.OutExpo))
            .Join(ScoreValue.transform.DOShakeScale(1.0f, .5f, 5, 45));
        
    }
}
