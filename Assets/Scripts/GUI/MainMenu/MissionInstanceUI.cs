using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionInstanceUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI name;
    [SerializeField] private TextMeshProUGUI score;

    public void Initialize(string missionName)
    {
        button.onClick.AddListener(() => GameController.Instance.PlayMission(missionName));
        name.text = missionName;

        string missionScoreName = "score" + missionName;
        Debug.Log(missionScoreName);
        if (PlayerPrefs.HasKey(missionScoreName))
        {
            score.text = "highscore: " + PlayerPrefs.GetInt(missionScoreName);
        }
    }
}