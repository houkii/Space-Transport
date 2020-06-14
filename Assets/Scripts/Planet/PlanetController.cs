using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlanetController : MonoBehaviour
{
    public GameObject CurrentTraveller;
    public PlatformController[] LandingPlatforms;
    public Transform PlanetBack;
    public Transform SpawnPosition;
    public Transform ReleaseSpot;
    public List<Transform> Waypoints;

    [SerializeField] private GameObject TravellerPrefab;
    [SerializeField] private TextMeshPro miniMapIndex;
    private PlanetInstance Data;
    private TargetIndicator targetIndicator;
    private Material planetMaterial;
    private Vector3 previousPos;
    private List<Vector3> chachedPositions = new List<Vector3> { Vector3.zero, Vector3.zero };
    private float angle = 0;

    public Vector3 CurrentVelocity => (chachedPositions[1] - chachedPositions[0]) / Time.fixedDeltaTime;


    private void Awake()
    {
        GameController.Instance.MissionController.OnEntitySpawned += SetCallbacks;
        SetRandomColor();
    }

    private void Start()
    {
        LandingPlatforms = GetComponentsInChildren<PlatformController>();
        targetIndicator = GetComponent<TargetIndicator>();
        targetIndicator.enabled = false;
        angle = Vector3.SignedAngle(Vector3.right, transform.position.normalized, Vector3.forward) * Mathf.Deg2Rad;
        GetComponent<Rigidbody>().mass *= GameController.Instance.Settings.PlanetMassScale;
        angle = GetInitialAngle();
    }

    public Transform GetNearestPlantformTransform(Vector3 position)
    {
        Transform nearestPlatform = transform;
        float currentLowestDistance = float.MaxValue;
        foreach (var platform in LandingPlatforms)
        {
            var distanceToPlatform = Vector3.Distance(position, platform.transform.position);
            if (distanceToPlatform < currentLowestDistance)
            {
                currentLowestDistance = distanceToPlatform;
                nearestPlatform = platform.transform;
            }
        }
        return nearestPlatform;
    }

    public PlanetController Initialize(PlanetInstance data)
    {
        Data = data;
        GetComponent<Rigidbody>().mass = Data.Mass;
        transform.localScale = Vector3.one * Data.Scale;

        if (data.Prefab.name.Contains("Moon"))
            miniMapIndex.text = "M";
        else
            miniMapIndex.text = Data.ID.ToString();

        if (Data.CentralObject != null)
            Data.Center = Data.CentralObject.position;

        return this;
    }

    private void FixedUpdate()
    {
        if (GameController.Instance.MissionController.CurrentMission.tutorial.Complete == false)
            return;

        transform.Rotate(transform.forward, Data.RotationSpeed * Time.fixedDeltaTime);
        OrbitalMove();
        HandleCachedPositions();
    }

    private void HandleCachedPositions()
    {
        chachedPositions.Add(transform.position);
        if (chachedPositions.Count == 3)
            chachedPositions.RemoveAt(0);
    }

    private void OrbitalMove()
    {
        float x = Mathf.Cos(angle * Mathf.Deg2Rad) * Data.Radius;
        float y = Mathf.Sin(angle * Mathf.Deg2Rad) * Data.Radius;
        transform.position = new Vector3(x, y, transform.position.z);
        transform.position += Data.CentralObject != null ? Data.CentralObject.position : Vector3.zero;
        angle += Data.AngularFrequency * Mathf.Rad2Deg * Time.fixedDeltaTime;
    }

    private float GetInitialAngle()
    {
        if (Data.CentralObject == null)
            return 0;

        var localPos = transform.position - Data.CentralObject.position;
        float angle = Mathf.Atan2(localPos.y, localPos.x) * Mathf.Rad2Deg;
        return angle;
    }

    private void SetCallbacks(NPCEntity entity)
    {
        if (entity.DestinationPlanet == this)
        {
            entity.OnGotAboard.AddListener(() => targetIndicator.enabled = true);
            entity.OnReachedDestination.AddListener(() => targetIndicator.enabled = false);
        }
    }

    private void SetRandomColor()
    {
        var renderer = transform.GetComponent<MeshRenderer>();
        planetMaterial = new Material(renderer.sharedMaterials[0]);
        Color color = PlanetColors.Colors[Random.Range(0, PlanetColors.Colors.Count)];
        planetMaterial.SetVector("_BaseColor", new Vector4(color.r, color.g, color.b, color.a));
        renderer.sharedMaterials = new Material[] { planetMaterial };
    }
}

