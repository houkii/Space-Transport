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

    private string missionName;

    private void OnEnable()
    {
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            PF_Bridge.OnPlayfabCallbackSuccess += HandleCallbackSuccess;
        }
        else
        {
            SetPlayerScore();
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
        button.onClick.AddListener(() => GameController.Instance.PlayMission(missionName));
        name.text = missionName;

        PF_PlayerData.GetPlayerLeaderboardPosition(this.missionName);
        PF_PlayerData.GetHighScore(this.missionName);
    }

    private void SetPlayerScore()
    {
        string missionScoreName = "Score" + missionName;
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            if (PF_PlayerData.Statistics.ContainsKey(missionName))
            {
                score.text = GetYourScoreText(PF_PlayerData.Statistics[missionName]);
            }

            if (PF_PlayerData.RankPositions.ContainsKey(missionName))
            {
                rank.text = "Rank: " + PF_PlayerData.RankPositions[missionName];
            }
        }
        else if (PlayerPrefs.HasKey(missionScoreName))
        {
            score.text = GetYourScoreText(PlayerPrefs.GetInt(missionScoreName));
        }
    }

    private void SetHighestRankScore()
    {
        highScore.text = "Highscore: " + PF_PlayerData.TopScores[missionName];
    }

    private string GetYourScoreText(int scoreValue) => "Score: " + scoreValue;
}