using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopPanelController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreValue;
    [SerializeField] private Slider fuelSlider;
    [SerializeField] private Button infoButton;
    [SerializeField] private Button radarButton;
    private Sequence updateScoreSequence;

    private int CurrentScore
    {
        set => scoreValue.text = value.ToString();
    }

    private void Start()
    {
        fuelSlider.maxValue = PlayerController.Instance.Stats.MaxFuel;
        fuelSlider.value = PlayerController.Instance.Stats.Fuel;
        infoButton.onClick.AddListener(ToggleInfo);
        radarButton.onClick.AddListener(ToggleRadarState);
        SetInfoButton(GameController.Instance.Settings.InfoActive);
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
        int currentScore = int.Parse(scoreValue.text);
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

    private void ToggleInfo()
    {
        GameController.Instance.Settings.InfoActive = !GameController.Instance.Settings.InfoActive;
        PlayerPrefs.SetInt("InfoActive", GameController.Instance.Settings.InfoActive ? 1 : 0);
        SetInfoButton(GameController.Instance.Settings.InfoActive);
    }

    private void SetInfoButton(bool active)
    {
        var buttonText = infoButton.GetComponentInChildren<TextMeshProUGUI>();

        if (active)
            buttonText.text = "Info ON";
        else
            buttonText.text = "Info OFF";
    }

    private void ToggleRadarState()
    {
        bool hasParent = CameraController.Instance.ToggleRadar();
        if (hasParent)
        {
            radarButton.GetComponentInChildren<TextMeshProUGUI>().text = "I";
        }
        else
        {
            radarButton.GetComponentInChildren<TextMeshProUGUI>().text = "II";
        }
    }
}