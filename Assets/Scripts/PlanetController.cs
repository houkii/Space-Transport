﻿using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlanetController : MonoBehaviour
{
    [SerializeField]
    private GameObject TravellerPrefab;

    [SerializeField]
    private TextMeshPro miniMapIndex;

    private PlanetInstance Data;
    private TargetIndicator targetIndicator;

    public GameObject CurrentTraveller;
    public PlatformController[] LandingPlatforms;
    public Transform PlanetBack;
    public Transform SpawnPosition;
    public Transform ReleaseSpot;
    public List<Transform> Waypoints;

    private Material planetMaterial;

    private float angle = 0;

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
    }

    public PlanetController Initialize(PlanetInstance data)
    {
        Data = data;
        GetComponent<Rigidbody>().mass = Data.Mass;
        transform.localScale = Vector3.one * Data.Scale;
        miniMapIndex.text = Data.ID.ToString();
        if (Data.CentralObject != null)
        {
            Data.Center = Data.CentralObject.position;
        }
        return this;
    }

    private void FixedUpdate()
    {
        transform.Rotate(transform.forward, Data.RotationSpeed * Time.fixedDeltaTime);
        OrbitalMove();
    }

    private void OrbitalMove()
    {
        float x = Mathf.Cos(angle) * Data.Radius;
        float y = Mathf.Sin(angle) * Data.Radius;
        transform.position = new Vector3(x, y, transform.position.z);
        transform.position += Data.CentralObject != null ? Data.CentralObject.position : Vector3.zero;
        angle += Data.AngularFrequency * Time.fixedDeltaTime;
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

    public Transform GetNearestPlantformTransform(Vector3 position)
    {
        Transform nearestPlatform = transform;
        float currentLowestDistance = 9999;
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

    //private void LateUpdate()
    //{
    //    miniMapIndex.transform.rotation = Quaternion.identity;
    //}
}

public static class PlanetColors
{
    public static Color32 Grey = new Color32(0x8A, 0xFF, 0xB7, 0xFF);
    public static Color32 Yellow = new Color32(0xFF, 0xE8, 0xB7, 0xFF);
    public static Color32 Orange = new Color32(0xFF, 0xBE, 0x87, 0xFF);
    public static Color32 LightRed = new Color32(0xE3, 0x8A, 0x74, 0xFF);
    public static Color32 DarkRed = new Color32(0xBF, 0x59, 0x67, 0xFF);

    public static List<Color32> Colors = new List<Color32> { Grey, Yellow, Orange, LightRed, DarkRed };
}