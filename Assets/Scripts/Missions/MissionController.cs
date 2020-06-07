using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class MissionController
{
    public List<Mission> AvailableMissions { get { return availableMissions; } }
    public Mission CurrentMission { get; private set; }
    public Dictionary<string, PlanetController> MissionPlanets { get; private set; }
    public UnityAction<NPCEntity> OnEntitySpawned { get; set; }
    public int CurrentMissionID { get; private set; }
    public int TotalNpcs { get; private set; }
    public bool MissionCompleted => NpcsLeft == 0;
    public UnityEvent OnMissionCompleted = new UnityEvent();
    public UnityEvent OnMissionInitialized = new UnityEvent();

    [SerializeField] private bool spawnRandomTravellers = true;
    [SerializeField] private List<GameObject> availableTravellers;
    [SerializeField] private List<Mission> availableMissions;
    private int npcsLeft;
    private Queue<TravellerInstance> NpcsToSpawn = new Queue<TravellerInstance>();


    public int NpcsLeft
    {
        get { return npcsLeft; }
        private set
        {
            npcsLeft = value;
            if (npcsLeft <= 0)
            {
                OnMissionCompleted?.Invoke();
            }
        }
    }

    public string NextMissionName =>
        availableMissions[(CurrentMissionID + 1) <= availableMissions.Count ? (CurrentMissionID + 1) : CurrentMissionID].name;

    public void InitializeMission(int missionID)
    {
        CurrentMissionID = missionID;
        MissionPlanets = new Dictionary<string, PlanetController>();
        CurrentMission = GameObject.Instantiate(AvailableMissions[CurrentMissionID]);
        CurrentMission.Name = AvailableMissions[CurrentMissionID].name;
        CurrentMission.Initialize();
        PlayerController.Instance.transform.position = CurrentMission.PlayerPosition;
        InitializePlanetarySystems(ref CurrentMission.PlanetarySystems);
        InitializePlanets(ref CurrentMission.Planets);
        InitializeNpcs();
        DialogCanvasManager.Instance.midInfo.Show(CurrentMission.Name, ShowStartInfo);
        OnMissionInitialized?.Invoke();
    }

    private void ShowStartInfo()
    {
        GameController.Instance.StartCoroutine(CurrentMission.tutorial.Show());
    }

    private void InitializePlanetarySystems(ref List<PlanetarySystemInstance> systems)
    {
        foreach (PlanetarySystemInstance system in systems)
        {
            var centralObject = GameObject.Instantiate(system.CentralObjectPrefab, system.Origin, Quaternion.identity);
            InitializePlanets(ref system.Planets, centralObject.transform);
        }
    }

    private void InitializePlanets(ref List<PlanetInstance> planets, Transform centralPlanet = null)
    {
        foreach (PlanetInstance planetData in planets)
        {
            if (centralPlanet != null)
            {
                planetData.CentralObject = centralPlanet;
                planetData.Position += centralPlanet.position;
                planetData.Center = centralPlanet.position;
            }

            var planetObject = GameObject.Instantiate(planetData.Prefab, planetData.Position, planetData.Rotation);
            planetObject.name = planetData.ID;

            var planetController = planetObject.GetComponent<PlanetController>().Initialize(planetData);
            MissionPlanets.Add(planetObject.name, planetController);

            if (planetData.Satellites.Count > 0)
            {
                InitializePlanets(ref planetData.Satellites, planetObject.transform);
            }
        }
    }

    private void InitializeNpcs()
    {
        NpcsToSpawn.Clear();
        foreach (TravellerInstance traveller in CurrentMission.NpcsToSpawn)
        {
            if (spawnRandomTravellers)
                traveller.TravelerPrefab = availableTravellers[UnityEngine.Random.Range(0, availableTravellers.Count)];

            NpcsToSpawn.Enqueue(traveller);
        }
        TotalNpcs = NpcsToSpawn.Count;
        npcsLeft = TotalNpcs;
        Spawn();
    }

    private IEnumerator SpawnNPCs()
    {
        while (NpcsToSpawn.Count > 0)
        {
            var NpcToSpawn = NpcsToSpawn.Dequeue();
            yield return new WaitForSeconds(NpcToSpawn.SpawnDelay);
            SpawnNPC(NpcToSpawn);
        }
    }

    private void Spawn()
    {
        if (NpcsToSpawn.Count > 1)
        {
            var spawnedNpcController = SpawnNPC(NpcsToSpawn.Dequeue());
            spawnedNpcController.OnReachedDestination.AddListener(Spawn);
        }
        else if (NpcsToSpawn.Count > 0)
        {
            SpawnNPC(NpcsToSpawn.Dequeue());
        }
    }

    private NPCEntity SpawnNPC(TravellerInstance npc, PlanetController planet = null)
    {
        var planetToSpawnOn = planet != null ? planet : MissionPlanets[npc.HostPlanet];
        var spawnedNpc = GameObject.Instantiate(npc.TravelerPrefab, planetToSpawnOn.SpawnPosition.position, Quaternion.identity);
        var npcController = spawnedNpc.GetComponent<NPCEntity>();
        npcController.Initialize(planetToSpawnOn, MissionPlanets[npc.DestinationPlanet]);
        npcController.OnReachedDestination.AddListener(() => --NpcsLeft);
        OnEntitySpawned?.Invoke(npcController);
        return npcController;
    }
}