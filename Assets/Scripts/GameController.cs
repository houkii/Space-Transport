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
        //Settings = new GameSettings();
        //PlayerPrefs.DeleteAll();
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
            //SceneController.Instance.LoadMainMenu();
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
        //PlayMission(id);
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

[System.Serializable]
public class GameSettings
{
    public int MaxSkips = 2;
    public float FuelCost = -50f;
    public float FuelLoading = 150f;
    public float G = 800.7f;
    public float DistanceScaler = 1.0f;
    public float PlayerDrag = 0.2f;
    public float PlayerMass = 1f;
    public float PlayerAccel = 7000f;
    public float PlanetMassScale = 1f;
    public float MaxFuel = 800;
    public float StartingFuel = 800;
    public float MaxLandingAngle = 15f;
    public float OxygenTimer = 45f;

    public Vector3 PlayerScaleMiniMap1 = new Vector3(45, 55, 25);
    public Vector3 PlayerScaleMiniMap2 = new Vector3(90, 110, 50);

    public int MaxRewardForTotalFuelUsed = 1000000;
    public int MaxRewardForRemainingFuel = 1500;
    public int DeliveryRewardMultiplier = 2000;
    public int LandingRewardMultiplier = 5;

    public float ConnectionRetryTime = 5.0f;

    public bool InfoActive = true;

    public Slider GSlider;
    public Slider DistanceSlider;
    public Slider DragSlider;
    public Slider PlayerMassSlider;
    public Slider PlanetMassSlider;

    public void Init()
    {
        GSlider.value = G;
        DistanceSlider.value = DistanceScaler;
        DragSlider.value = PlayerDrag;
        PlayerMassSlider.value = PlayerMass;
        PlanetMassSlider.value = PlanetMassScale;

        GSlider.onValueChanged.AddListener(delegate { G = GSlider.value; });
        DistanceSlider.onValueChanged.AddListener(delegate { DistanceScaler = DistanceSlider.value; });
        DragSlider.onValueChanged.AddListener(delegate { PlayerController.Instance.GetComponent<Rigidbody>().drag = DragSlider.value; });
        PlayerMassSlider.onValueChanged.AddListener(delegate { PlayerController.Instance.GetComponent<Rigidbody>().mass = PlayerMassSlider.value; });
        PlanetMassSlider.onValueChanged.AddListener(delegate { PlanetMassScale = PlanetMassSlider.value; });

        LoadSettings();
    }

    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey("InfoActive"))
        {
            InfoActive = PlayerPrefs.GetInt("InfoActive") == 1 ? true : false;
        }
    }
}

[System.Serializable]
public class PlayerData
{
    public enum MissionState { Unavailable = 0, Available = 1, Skipped = 2 }

    public Dictionary<string, MissionState> missionCompletes = new Dictionary<string, MissionState>();

    public int NumSkips = 0;

    public void Init()
    {
        var missions = GameController.Instance.MissionController.AvailableMissions;
        for (int i = 0; i < missions.Count; i++)
        {
            int missionState = PlayerPrefs.GetInt(missions[i].name + "Completed");
            missionCompletes.Add(missions[i].name, (MissionState)missionState);
            Debug.Log(missions[i].name + ": " + (MissionState)missionState);

            if ((MissionState)missionState == MissionState.Skipped)
            {
                NumSkips++;
            }
        }

        missionCompletes[missions[0].name] = MissionState.Available;
    }

    //public bool CanMissionBeSkipped (string name) => missionCompletes[name] == MissionState.
    public bool IsMissionAvailable(string name) => missionCompletes[name] == MissionState.Available;

    public bool IsNextMissionAvailable()
    {
        var nextMissionName = GameController.Instance.MissionController.NextMissionName;
        if (nextMissionName != GameController.Instance.MissionController.CurrentMission.name)
        {
            if (missionCompletes[nextMissionName] == MissionState.Available)
                return true;
        }

        return false;
    }

    public void MakeNextMissionAvailable()
    {
        var nextMissionName = GameController.Instance.MissionController.NextMissionName;
        if (missionCompletes[nextMissionName] != MissionState.Available)
        {
            missionCompletes[nextMissionName] = MissionState.Available;
            PlayerPrefs.SetInt(nextMissionName + "Completed", (int)MissionState.Available);
        }
    }
}