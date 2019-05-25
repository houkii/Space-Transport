using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionChoosePanelController : MovableCanvasElement
{
    [SerializeField] private GameObject missionInstanceUIprefab;
    [SerializeField] private Transform missionContainer;
    [SerializeField] private Button backToMainButton;
    private List<MissionInstanceUI> missionUIInstances = new List<MissionInstanceUI>();

    protected override void Awake()
    {
        base.Awake();
        this.InitializeMissionButtons();
    }

    private void OnEnable()
    {
        backToMainButton.onClick.AddListener(SoundManager.Instance.PlayBackButton);
    }

    private void OnDisable()
    {
        backToMainButton.onClick.RemoveListener(SoundManager.Instance.PlayBackButton);
    }

    private void InitializeMissionButtons()
    {
        foreach(var mission in GameController.Instance.MissionController.AvailableMissions)
        {
            var uiMission = Instantiate(missionInstanceUIprefab, missionContainer)
                .GetComponent<MissionInstanceUI>();

            uiMission.Initialize(mission.name);
        }
    }
}
