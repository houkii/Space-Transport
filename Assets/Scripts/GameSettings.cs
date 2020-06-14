using UnityEngine;
using UnityEngine.UI;

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
    public float ConnectionRetryTime = 5.0f;
    public Vector3 PlayerScaleMiniMap1 = new Vector3(45, 55, 25);
    public Vector3 PlayerScaleMiniMap2 = new Vector3(90, 110, 50);
    public int MaxRewardForTotalFuelUsed = 1000000;
    public int MaxRewardForRemainingFuel = 1500;
    public int DeliveryRewardMultiplier = 2000;
    public int LandingRewardMultiplier = 5;
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

