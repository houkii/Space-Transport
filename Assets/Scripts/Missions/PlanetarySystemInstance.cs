using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class PlanetarySystemInstance
{
    public Vector3 Origin;
    public GameObject CentralObjectPrefab;
    public List<PlanetInstance> Planets;
}
