using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

[Serializable]
public class MissionController
{
    [SerializeField]
    private List<Mission> AvailableMissions;
    private Queue<TravellerInstance> NpcsToSpawn = new Queue<TravellerInstance>();

    public Mission CurrentMission { get; private set; }
    public List<PlanetController> MissionPlanets { get; private set; }
    public int CurrentMissionID { get; private set; }
    public UnityAction<NPCEntity> OnEntitySpawned { get; set; }

    public void InitializeMission(int missionID)
    {
        CurrentMissionID = missionID;
        MissionPlanets = new List<PlanetController>();
        CurrentMission = GameObject.Instantiate(AvailableMissions[CurrentMissionID]);
        InitializePlanets();
        InitializeNpcs();
    }

    private void InitializePlanets()
    {
        foreach(PlanetInstance planetData in CurrentMission.Planets)
        {
            var planetObject = GameObject.Instantiate(planetData.Prefab, planetData.Position, planetData.Rotation);
            planetObject.name = String.Format("Planet {0}", planetData.ID);
            var planetController = planetObject.GetComponent<PlanetController>().Initialize(planetData);
            MissionPlanets.Add(planetController);
        }
    }

    private void InitializeNpcs()
    {
        foreach(TravellerInstance traveller in CurrentMission.NpcsToSpawn)
        {
            NpcsToSpawn.Enqueue(traveller);
        }
        //GameController.Instance.StartCoroutine(SpawnNPCs());
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
            Debug.Log("No more npcs to spawn!");
        }
    }

    private NPCEntity SpawnNPC(TravellerInstance npc, PlanetController planet = null)
    {
        var planetToSpawnOn = planet != null ? planet : MissionPlanets[npc.HostPlanet];
        var spawnedNpc = GameObject.Instantiate(npc.TravelerPrefab, planetToSpawnOn.SpawnPosition.position, Quaternion.identity);
        var npcController = spawnedNpc.GetComponent<NPCEntity>();
        npcController.Initialize(planetToSpawnOn, MissionPlanets[npc.DestinationPlanet]);
        OnEntitySpawned?.Invoke(npcController);
        return npcController;
    }

}
