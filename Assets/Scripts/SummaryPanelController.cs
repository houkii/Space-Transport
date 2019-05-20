using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SummaryPanelController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI SummaryText;
    [SerializeField]
    private TextMeshProUGUI TotalPointsText;
    [SerializeField]
    private TextMeshProUGUI TotalFuelUsedText;
    [SerializeField]
    private TextMeshProUGUI TravellersDeliveredText;
    [SerializeField]
    private Button MainMenuButton;
    [SerializeField]
    private Button RestartButton;
    [SerializeField]
    private Button NextMissionButton;

    void Awake()
    {
        MainMenuButton.onClick.AddListener(SceneController.Instance.LoadMainMenu);
        RestartButton.onClick.AddListener(SceneController.Instance.LoadLevel);
        NextMissionButton.onClick.AddListener(SceneController.Instance.LoadLevel);
    }

    public void Show(bool missionCompleted)
    {
        SummaryText.text = missionCompleted ? "Mission Completed!" : "Mission Failed!";

        TravellersDeliveredText.text = string.Format("{0}/{1}",
            (GameController.Instance.MissionController.TotalNpcs - GameController.Instance.MissionController.NpcsLeft),
            GameController.Instance.MissionController.TotalNpcs);

        TotalFuelUsedText.text = string.Format("{0:0.#}", PlayerController.Instance.Stats.TotalFuelUsed);
        TotalPointsText.text = string.Format("{0}", PlayerController.Instance.Stats.Score);
        GetComponent<MovableCanvasElement>().Show();
    }

    public void Hide()
    {
        GetComponent<MovableCanvasElement>().Hide();
    }
}
