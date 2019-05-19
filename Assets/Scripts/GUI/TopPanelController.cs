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
    private TextMeshProUGUI scoreValue;
    private int CurrentScore
    {
        set
        {
            scoreValue.text = value.ToString();
        }
    }

    [SerializeField]
    private Slider fuelSlider;
    private Sequence updateScoreSequence;

    private void Start()
    {
        fuelSlider.maxValue = PlayerController.Instance.Stats.MaxFuel;
        fuelSlider.value = PlayerController.Instance.Stats.Fuel;
    }

    private void OnEnable()
    {
        PlayerController.Instance.Stats.OnScoreUpdated += UpdateScore;
        PlayerController.Instance.Stats.OnFuelUpdated += UpdateFuel;
    }

    private void OnDisable()
    {
        PlayerController.Instance.Stats.OnScoreUpdated -= UpdateScore;
        PlayerController.Instance.Stats.OnFuelUpdated -= UpdateFuel;
    }

    private void UpdateScore(int value)
    {
        int currentScore = Int32.Parse(scoreValue.text);
        scoreValue.transform.localScale = Vector3.one;
        updateScoreSequence.Kill();
        updateScoreSequence = DOTween.Sequence();
        updateScoreSequence.Append(DOTween.To(() => currentScore, x => CurrentScore = x, value, 1.0f).SetEase(Ease.OutExpo))
            .Join(scoreValue.transform.DOShakeScale(1.0f, .5f, 5, 45)); 
    }

    private void UpdateFuel(float value)
    {
        fuelSlider.value = value;
    }
}
