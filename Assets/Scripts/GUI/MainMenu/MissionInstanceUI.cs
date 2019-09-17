using PlayFab;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionInstanceUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI name;
    [SerializeField] private TextMeshProUGUI score;

    private string missionName;

    private void OnEnable()
    {
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            PF_Bridge.OnPlayfabCallbackSuccess += HandleCallbackSuccess;
        }
        else
        {
            SetMapData();
        }
    }

    private void OnDisable()
    {
        PF_Bridge.OnPlayfabCallbackSuccess -= HandleCallbackSuccess;
    }

    private void HandleCallbackSuccess(string details, PlayFabAPIMethods method, MessageDisplayStyle displayStyle)
    {
        if (method == PlayFabAPIMethods.GetPlayerLeaderboard && details == missionName)
        {
            SetMapData();
        }
    }

    public void Initialize(string missionName)
    {
        this.missionName = missionName;
        button.onClick.AddListener(() => GameController.Instance.PlayMission(missionName));
        name.text = missionName;

        PF_PlayerData.GetPlayerLeaderboardPosition(this.missionName);
    }

    private void SetMapData()
    {
        string missionScoreName = "score" + missionName;
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            if (PF_PlayerData.Statistics.ContainsKey(missionName))
            {
                score.text = GetYourScoreText(PF_PlayerData.Statistics[missionName]);
            }
        }
        else if (PlayerPrefs.HasKey(missionScoreName))
        {
            score.text = GetYourScoreText(PlayerPrefs.GetInt(missionScoreName));
        }
    }

    private string GetYourScoreText(int scoreValue) => "your score: " + scoreValue;
}