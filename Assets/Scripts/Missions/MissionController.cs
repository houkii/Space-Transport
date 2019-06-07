using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

[Serializable]
public class MissionController
{
    [SerializeField]
    private List<Mission> availableMissions;
    public List<Mission> AvailableMissions { get { return availableMissions; } }
    private Queue<TravellerInstance> NpcsToSpawn = new Queue<TravellerInstance>();

    public Mission CurrentMission { get; private set; }
    public Dictionary<string, PlanetController> MissionPlanets { get; private set; }
    public int CurrentMissionID { get; private set; }
    public UnityAction<NPCEntity> OnEntitySpawned { get; set; }
    public UnityEvent OnMissionCompleted = new UnityEvent();
    public UnityEvent OnMissionInitialized = new UnityEvent();
    public bool MissionCompleted => NpcsLeft == 0;

    public int TotalNpcs { get; private set; }
    private int npcsLeft;
    public int NpcsLeft
    {
        get { return npcsLeft; }
        private set
        {
            npcsLeft = value;
            if(npcsLeft <= 0)
            {
                OnMissionCompleted?.Invoke();
            }
        }
    }

    public void InitializeMission(int missionID)
    {
        CurrentMissionID = missionID;
        MissionPlanets = new Dictionary<string, PlanetController>();
        CurrentMission = GameObject.Instantiate(AvailableMissions[CurrentMissionID]);
        PlayerController.Instance.transform.position = CurrentMission.PlayerPosition;
        InitializePlanetarySystems(ref CurrentMission.PlanetarySystems);
        InitializePlanets(ref CurrentMission.Planets);
        InitializeNpcs();
        OnMissionInitialized?.Invoke();
    }

    //private void InitializePlanets()
    //{
    //    foreach(PlanetInstance planetData in CurrentMission.Planets)
    //    {
    //        var planetObject = GameObject.Instantiate(planetData.Prefab, planetData.Position, planetData.Rotation);
    //        planetObject.name = String.Format("Planet {0}", planetData.ID);
    //        var planetController = planetObject.GetComponent<PlanetController>().Initialize(planetData);
    //        MissionPlanets.Add(planetController);
    //    }
    //}

    private void InitializePlanetarySystems(ref List<PlanetarySystemInstance> systems)
    {
        foreach(PlanetarySystemInstance system in systems)
        {
            var centralObject = GameObject.Instantiate(system.CentralObjectPrefab, system.Origin, Quaternion.identity);
            InitializePlanets(ref system.Planets, centralObject.transform);
        }
    }

    private void InitializePlanets(ref List<PlanetInstance> planets, Transform centralPlanet = null)
    {
        foreach (PlanetInstance planetData in planets)
        {
            if(centralPlanet != null)
            {
                planetData.CentralObject = centralPlanet;
                planetData.Position += centralPlanet.position;
                planetData.Center = centralPlanet.position;
            }

            var planetObject = GameObject.Instantiate(planetData.Prefab, planetData.Position, planetData.Rotation);
            //planetObject.name = String.Format("Planet {0}", planetData.ID);
            planetObject.name = planetData.ID;
            var planetController = planetObject.GetComponent<PlanetController>().Initialize(planetData);
            MissionPlanets.Add(planetObject.name, planetController);

            if(planetData.Satellites.Count > 0)
            {
                InitializePlanets(ref planetData.Satellites, planetObject.transform);
            }
        }
    }

    private void InitializeNpcs()
    {
        NpcsToSpawn.Clear();
        foreach(TravellerInstance traveller in CurrentMission.NpcsToSpawn)
        {
            NpcsToSpawn.Enqueue(traveller);
        }
        this.TotalNpcs = NpcsToSpawn.Count;
        this.npcsLeft = this.TotalNpcs;
        this.Spawn();
    }

    private IEnumerator SpawnNPCs()
    {
        while(this.NpcsToSpawn.Count > 0)
        {
            var NpcToSpawn = NpcsToSpawn.Dequeue();
            yield return new WaitForSeconds(NpcToSpawn.SpawnDelay);
            this.SpawnNPC(NpcToSpawn);
        }
    }

    private void Spawn()
    {
        if(this.NpcsToSpawn.Count > 1)
        {
            var spawnedNpcController = this.SpawnNPC(NpcsToSpawn.Dequeue());
            spawnedNpcController.OnReachedDestination.AddListener(this.Spawn);
        }
        else if(this.NpcsToSpawn.Count > 0)
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
