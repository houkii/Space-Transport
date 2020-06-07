using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : Singleton<GameController>
{
    public MissionController MissionController;
    public RewardFactory Rewards { get; private set; }
    public GameSettings Settings;
    public PlayerData PlayerData;
    public int MissionID { get; private set; }
    public bool DevModeEnabled = true;
    public ColorPalette ColorPaletteDarkPink;
    public ColorPalette ColorPaletteLightPink;

    public int androidApiLevel { get; private set; }

    private void Start()
    {
        Rewards = new RewardFactory();
        PlayerPrefs.SetInt("score", 0);
        Settings.Init();
        PlayerData.Init();
        StartCoroutine(ConnectToPlayfab());

#if UNITY_ANDROID && !UNITY_EDITOR
        androidApiLevel = int.Parse(SystemInfo.operatingSystem.Substring(SystemInfo.operatingSystem.IndexOf("-") + 1, 3));
        Debug.LogError("Running on Android API: " + androidApiLevel);
        Vibration.Initialize(androidApiLevel);
#endif
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
        if (Input.GetKeyDown(KeyCode.P))
        {
            MissionController.OnMissionCompleted?.Invoke();
        }
    }

    public void InitializePlayScene()
    {
        MissionController.InitializeMission(MissionID);
        MissionController.OnMissionCompleted.AddListener(PlayerData.MakeNextMissionAvailable);
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
        if (MissionController.AvailableMissions.Count > MissionID
            && PlayerData.IsMissionAvailable(MissionController.AvailableMissions[MissionID].name))
        {
            PlayMission(MissionID);
        }
        else
        {
            DialogCanvasManager.Instance.midInfo.Show("Mission Locked!");
        }
    }

    public void PlayMission(int missionID)
    {
        MissionID = missionID;
        SceneController.Instance.LoadLevel();
    }

    public void PlayMissionIfUnlocked(int missionID)
    {
    }

    public void PlayMission(string missionName)
    {
        var allMissions = MissionController.AvailableMissions;
        var mission = allMissions.Find(x => x.name == missionName);
        var id = allMissions.FindIndex(x => x == mission);
        DialogCanvasManager.Instance.overlapping.Show("Loading", () => PlayMission(id));
    }

    private IEnumerator ConnectToPlayfab()
    {
        while (!PlayFabClientAPI.IsClientLoggedIn())
        {
            Authentication.Login();
            yield return new WaitForSeconds(Settings.ConnectionRetryTime);
        }
    }

    public void SetG(float val) => Settings.G = val;
    public void SetDistanceScaler(float val) => Settings.DistanceScaler = val;
}

