using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class MissionController
{
    [SerializeField]
    private List<Mission> AvailableMissions;

    public Mission CurrentMission { get; private set; }
    public List<PlanetController> MissionPlanets { get; private set; }
    public int CurrentMissionID { get; private set; }

    public void InitializeMission(int missionID)
    {
        CurrentMissionID = missionID;
        MissionPlanets = new List<PlanetController>();
        CurrentMission = GameObject.Instantiate(AvailableMissions[CurrentMissionID]);
        InitializePlanets();
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
}

