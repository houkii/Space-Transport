using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class GameController : Singleton<GameController>
{
    [SerializeField]
    public MissionController MissionController;
    [SerializeField]
    public bool DevModeEnabled = true;
    public RewardFactory Rewards { get; private set; }
    public GameSettings Settings { get; private set; }

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
            SceneController.Instance.LoadLevel();
        }
        if(Input.GetKeyDown(KeyCode.Z))
        {
            PlayerController.Instance.Stats.AddScore(Rewards.GetReward(Reward.RewardType.DeliveryReward, new DeliveryRewardArgs(5,5)));
        }
    }
}

public class GameSettings
{
    public readonly float FuelCost = -50f;
    public readonly float FuelLoading = 150f;
}