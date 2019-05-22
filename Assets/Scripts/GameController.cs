using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using System.Linq;

public class GameController : Singleton<GameController>
{
    [SerializeField]
    public MissionController MissionController;
    [SerializeField]
    public bool DevModeEnabled = true;
    public RewardFactory Rewards { get; private set; }
    public GameSettings Settings { get; private set; }
    public int MissionID { get; private set; }

    private void Start()
    {
        Rewards = new RewardFactory();
        Settings = new GameSettings();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            SceneController.Instance.LoadMainMenu();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartMission();
        }
        if(Input.GetKeyDown(KeyCode.Z))
        {
            PlayerController.Instance.Stats.AddScore(Rewards.GetReward(Reward.RewardType.DeliveryReward, new DeliveryRewardArgs(5,5)));
        }
    }

    public void InitializePlayScene()
    {
        MissionController.InitializeMission(MissionID);
    }

    public void RestartMission()
    {
        if(MissionID >= 0)
        {
            SceneController.Instance.LoadLevel();
        }
    }

    public void PlayNextMission()
    {
        MissionID++;
        if(MissionController.AvailableMissions.Count < MissionID)
        {
            PlayMission(MissionID);
        }
        else
        {
            SceneController.Instance.LoadMainMenu();
        }
    }

    public void PlayMission(int missionID)
    {
        MissionID = missionID;
        SceneController.Instance.LoadLevel();
    }

    public void PlayMission(string missionName)
    {
        var mission = MissionController.AvailableMissions.Find(x => x.name == missionName);
        var id = MissionController.AvailableMissions.FindIndex(x => x == mission);
        PlayMission(id);
    }
}

public class GameSettings
{
    public readonly float FuelCost = -50f;
    public readonly float FuelLoading = 150f;
}