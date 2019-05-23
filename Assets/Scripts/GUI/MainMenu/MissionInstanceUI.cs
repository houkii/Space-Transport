using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MissionInstanceUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI name;

    public void Initialize(string missionName)
    {
        button.onClick.AddListener(() => GameController.Instance.PlayMission(missionName));
        name.text = missionName;
    }
}
