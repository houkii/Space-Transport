using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MissionData", menuName = "Mission", order = 1)]
public class Mission : ScriptableObject
{
    public string Name;
    public Vector3 PlayerPosition;
    public List<PlanetarySystemInstance> PlanetarySystems;
    public List<PlanetInstance> Planets;
    public List<TravellerInstance> NpcsToSpawn;
    public int BoundsSize;
    public Tutorial tutorial;

    public void Initialize()
    {
        tutorial.OnTutorialCompleted += SaveToPrefs;
        if (PlayerPrefs.HasKey(TutorialString))
        {
            if (GameController.Instance.Settings.InfoActive)
                return;

            var tutorialCompleteInt = PlayerPrefs.GetInt(TutorialString);
            if (tutorialCompleteInt == 1)
                tutorial.Complete = true;
        }
    }

    private void SaveToPrefs()
    {
        PlayerPrefs.SetInt(Name + "Tutorial", 1);
    }

    private string TutorialString => Name + "Tutorial";
}



