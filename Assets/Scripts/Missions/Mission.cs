using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "MissionData", menuName = "Mission", order = 1)]
public class Mission : ScriptableObject
{
    public string Name;
    public List<PlanetInstance> Planets;
    public List<TravellerInstance> NpcsToSpawn;
}
