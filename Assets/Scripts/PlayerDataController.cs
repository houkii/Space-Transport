using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerDataController
{
    public enum MissionState { Unavailable = 0, Available = 1, Skipped = 2 }
    public Dictionary<string, MissionState> missionCompletes = new Dictionary<string, MissionState>();
    public int NumSkips = 0;

    public void Init()
    {
        var missions = GameController.Instance.MissionController.AvailableMissions;
        for (int i = 0; i < missions.Count; i++)
        {
            int missionState = PlayerPrefs.GetInt(missions[i].name + "Completed");
            missionCompletes.Add(missions[i].name, (MissionState)missionState);
            Debug.Log(missions[i].name + ": " + (MissionState)missionState);

            if ((MissionState)missionState == MissionState.Skipped)
            {
                NumSkips++;
            }
        }

        missionCompletes[missions[0].name] = MissionState.Available;
    }

    public bool IsMissionAvailable(string name) => missionCompletes[name] == MissionState.Available;

    public bool IsNextMissionAvailable()
    {
        var nextMissionName = GameController.Instance.MissionController.NextMissionName;
        if (nextMissionName != GameController.Instance.MissionController.CurrentMission.name)
        {
            if (missionCompletes[nextMissionName] == MissionState.Available)
                return true;
        }

        return false;
    }

    public void MakeNextMissionAvailable()
    {
        var nextMissionName = GameController.Instance.MissionController.NextMissionName;
        if (missionCompletes[nextMissionName] != MissionState.Available)
        {
            missionCompletes[nextMissionName] = MissionState.Available;
            PlayerPrefs.SetInt(nextMissionName + "Completed", (int)MissionState.Available);
        }
    }
}
