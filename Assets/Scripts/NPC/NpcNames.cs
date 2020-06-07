using System;
using System.Collections.Generic;

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