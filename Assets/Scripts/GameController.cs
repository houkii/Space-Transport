using PlayFab;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameController : Singleton<GameController>
{
    public MissionController MissionController;
    public RewardFactory Rewards { get; private set; }
    public GameSettings Settings;
    public int MissionID { get; private set; }
    public bool DevModeEnabled = true;

    public ColorPalette ColorPaletteDarkPink;
    public ColorPalette ColorPaletteLightPink;

    private void Start()
    {
        Rewards = new RewardFactory();
        //Settings = new GameSettings();
        PlayerPrefs.SetInt("score", 0);
        Settings.Init();
        StartCoroutine(ConnectToPlayfab());
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
        if (MissionController.AvailableMissions.Count > MissionID)
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
    public float FuelCost = -50f;
    public float FuelLoading = 150f;
    public float G = 800.7f;
    public float DistanceScaler = 1.0f;
    public float PlayerDrag = 0.2f;
    public float PlayerMass = 1f;
    public float PlayerAccel = 7000f;
    public float PlanetMassScale = 1f;

    public int MaxRewardForTotalFuelUsed = 1000000;
    public int MaxRewardForRemainingFuel = 1000;
    public int DeliveryRewardMultiplier = 5;
    public int LandingRewardMultiplier = 5;

    public float ConnectionRetryTime = 5.0f;

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
    }
}