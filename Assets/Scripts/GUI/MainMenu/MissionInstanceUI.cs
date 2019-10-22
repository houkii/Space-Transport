using PlayFab;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionInstanceUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI name;
    [SerializeField] private TextMeshProUGUI score;
    [SerializeField] private TextMeshProUGUI rank;
    [SerializeField] private TextMeshProUGUI highScore;
    [SerializeField] private Image lockImage;

    private string missionName;

    private void OnEnable()
    {
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            PF_Bridge.OnPlayfabCallbackSuccess += HandleCallbackSuccess;
        }
    }

    private void OnDisable()
    {
        PF_Bridge.OnPlayfabCallbackSuccess -= HandleCallbackSuccess;
    }

    private void HandleCallbackSuccess(string details, PlayFabAPIMethods method, MessageDisplayStyle displayStyle)
    {
        if (details != missionName)
            return;

        if (method == PlayFabAPIMethods.GetPlayerLeaderboard)
        {
            SetPlayerScore();
        }
        else if (method == PlayFabAPIMethods.GetFriendsLeaderboard)
        {
            SetHighestRankScore();
        }
    }

    public void Initialize(string missionName)
    {
        this.missionName = missionName;
        name.text = missionName;
        if (GameController.Instance.PlayerData.IsMissionAvailable(missionName) || GameController.Instance.DevModeEnabled)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => GameController.Instance.PlayMission(missionName));
            lockImage.gameObject.SetActive(false);

            if (PlayFabClientAPI.IsClientLoggedIn())
            {
                PF_PlayerData.GetPlayerLeaderboardPosition(this.missionName);
                PF_PlayerData.GetHighScore(this.missionName);
            }
            else
            {
                SetPlayerScore();
            }
        }
        else
        {
            lockImage.gameObject.SetActive(true);
            button.onClick.AddListener(() => DialogCanvasManager.Instance.midInfo.Show("Mission Locked!"));
        }
    }

    private void SetPlayerScore()
    {
        string missionScoreName = "score" + missionName;
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            if (PF_PlayerData.Statistics.ContainsKey(missionName))
            {
                score.text = GetYourScoreText(PF_PlayerData.Statistics[missionName]);
            }

            if (PF_PlayerData.RankPositions.ContainsKey(missionName))
            {
                rank.text = "Rank: " + (PF_PlayerData.RankPositions[missionName] + 1);
            }
        }
        else if (PlayerPrefs.HasKey(missionScoreName))
        {
            score.text = GetYourScoreText(PlayerPrefs.GetInt(missionScoreName));
        }
    }

    private void SetHighestRankScore()
    {
        highScore.text = "Record: " + PF_PlayerData.TopScores[missionName];
    }

    private string GetYourScoreText(int scoreValue) => "Score: " + scoreValue;
}