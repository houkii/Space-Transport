using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class TravellerInstance
{
    public GameObject TravelerPrefab;
    public string HostPlanet;
    public string DestinationPlanet;
    public float SpawnDelay;
}

public static class NpcNames
{
    public static string RandomName => GetName();

    private static List<String> npcNames = new List<string>
    {
        "Zwicky",
        "Young",
        "Webb",
        "Tyson",
        "Sagan",
        "Paczynski",
        "Newton",
        "Armstrong",
        "Kuiper",
        "Herschel",
        "Einstein",
        "Copernicus",
        "Kepler",
        "Ptolemy",
        "Galilei",
        "Eddington",
        "Laplace",
        "Hubble",
        "Huygens",
        "Messier",
        "Hawking",
        "Penzias",
        "Wilson",
        "Bode",
        "Olbers",
        "Schwarzchild",
        "Wolf",
        "Nakajima"
    };

    private static string GetName()
    {
        int index = UnityEngine.Random.Range(0, npcNames.Count);
        return npcNames[index];
    }
}