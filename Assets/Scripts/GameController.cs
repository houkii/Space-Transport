using UnityEngine;

public class GameController : Singleton<GameController>
{
    public MissionController MissionController;
    public RewardFactory Rewards { get; private set; }
    public GameSettings Settings { get; private set; }
    public int MissionID { get; private set; }
    public bool DevModeEnabled = true;

    private void Start()
    {
        Rewards = new RewardFactory();
        Settings = new GameSettings();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneController.Instance.LoadMainMenu();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartMission();
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            PlayerController.Instance.Stats.AddScore(Rewards.GetReward(Reward.RewardType.DeliveryReward, new DeliveryRewardArgs(5, 5)));
        }
    }

    public void InitializePlayScene()
    {
        MissionController.InitializeMission(MissionID);
    }

    public void RestartMission()
    {
        if (MissionID >= 0)
        {
            SceneController.Instance.LoadLevel();
        }
    }

    public void PlayNextMission()
    {
        MissionID++;
        if (MissionController.AvailableMissions.Count < MissionID)
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
        var allMissions = MissionController.AvailableMissions;
        var mission = allMissions.Find(x => x.name == missionName);
        var id = allMissions.FindIndex(x => x == mission);
        PlayMission(id);
    }
}

public class GameSettings
{
    public readonly float FuelCost = -50f;
    public readonly float FuelLoading = 150f;
}