using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionChoosePanelController : MovableCanvasElement
{
    [SerializeField] private GameObject missionInstanceUIprefab;
    [SerializeField] private Transform missionContainer;
    private List<MissionInstanceUI> missionUIInstances = new List<MissionInstanceUI>();

    protected override void Awake()
    {
        base.Awake();
        this.InitializeMissionButtons();
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
