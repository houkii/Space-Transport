using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SummaryPanelController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI SummaryText;
    [SerializeField] private TextMeshProUGUI TravellersDeliveredText;
    [SerializeField] private TextMeshProUGUI PersonalRecordText;
    [SerializeField] private TextMeshProUGUI GlobalHighScoreText;
    [SerializeField] private TweenableStat TotalPointsText;
    [SerializeField] private TweenableStat TotalFuelUsedText;
    [SerializeField] private Button MainMenuButton;
    [SerializeField] private Button RestartButton;
    [SerializeField] private Button NextMissionButton;

    private void Awake()
    {
        MainMenuButton.onClick.AddListener(SceneController.Instance.LoadMainMenu);
        MainMenuButton.onClick.AddListener(SoundManager.Instance.PlayBackButton);
        RestartButton.onClick.AddListener(GameController.Instance.RestartMission);
        RestartButton.onClick.AddListener(SoundManager.Instance.PlayForwardButton);
        NextMissionButton.onClick.AddListener(GameController.Instance.PlayNextMission);
        NextMissionButton.onClick.AddListener(SoundManager.Instance.PlayForwardButton);
    }

    private void Start()
    {
        PlayerController.Instance.OnNewGlobalHighScore.AddListener(() => ShowRecord(GlobalHighScoreText.rectTransform));
        PlayerController.Instance.OnNewPersonalHighScore.AddListener(() => ShowRecord(PersonalRecordText.rectTransform));
    }

    public void Show(bool missionCompleted)
    {
        SummaryText.text = missionCompleted ? "Mission Completed!" : "Mission Failed!";

        TravellersDeliveredText.text = string.Format("{0}/{1}",
            (GameController.Instance.MissionController.TotalNpcs - GameController.Instance.MissionController.NpcsLeft),
            GameController.Instance.MissionController.TotalNpcs);

        //TotalFuelUsedText.text = string.Format("{0:0.#}", PlayerController.Instance.Stats.TotalFuelUsed);
        //TotalPointsText.text = string.Format("{0}", PlayerController.Instance.Stats.Score);
        TotalFuelUsedText.Set(PlayerController.Instance.Stats.TotalFuelUsed, "{0:0.0}");
        TotalPointsText.Set(PlayerController.Instance.Stats.Score, "{0:0}");

        GetComponent<MovableCanvasElement>().Show();
    }

    public void Hide()
    {
        GetComponent<MovableCanvasElement>().Hide();
    }

    private void ShowRecord(RectTransform rt)
    {
        rt.gameObject.SetActive(true);
        rt.localScale = Vector3.zero;
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(3f)
            .Append(rt.DOScale(1, .45f).SetEase(Ease.OutBounce))
            .Append(rt.DOScale(1.1f, .2f).SetEase(Ease.InExpo).SetLoops(int.MaxValue, LoopType.Yoyo));
    }
}