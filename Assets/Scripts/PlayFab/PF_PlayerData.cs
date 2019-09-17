using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PF_PlayerData : MonoBehaviour
{
    public static string PlayerName;
    public static string PlayfabID;
    public static string PlayfabUsername;

    public static int Coins;
    public static readonly List<ItemInstance> Inventory = new List<ItemInstance>();

    public static Dictionary<string, int> Statistics = new Dictionary<string, int>();
    public static Dictionary<string, int> RankPositions = new Dictionary<string, int>();
    public static Dictionary<string, int> TopScores = new Dictionary<string, int>();

    public static void GetAccountInfo()
    {
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(), (GetAccountInfoResult result) =>
        {
            PlayerName = result.AccountInfo.TitleInfo.DisplayName;
            PlayfabID = result.AccountInfo.PlayFabId;
            PlayfabUsername = result.AccountInfo.Username;

            PF_Bridge.RaiseCallbackSuccess(string.Empty, PlayFabAPIMethods.GetAccountInfo, MessageDisplayStyle.none);
        }, PF_Bridge.PlayFabErrorCallback);
    }

    public static void GetInventory()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), (GetUserInventoryResult result) =>
        {
            Coins = result.VirtualCurrency["GM"];
            //Debug.Log("USER COINS: " + Coins);
            foreach (ItemInstance item in result.Inventory)
            {
                Inventory.Add(item);
            }
            PF_Bridge.RaiseCallbackSuccess("", PlayFabAPIMethods.GetUserInventory, MessageDisplayStyle.success);
        },
        (PlayFabError error) => PF_Bridge.RaiseCallbackError(error.ErrorMessage, PlayFabAPIMethods.GetUserInventory, MessageDisplayStyle.error));
    }

    public static void UpdateUserScore(string mapName, int scoreValue)
    {
        PlayFabClientAPI.UpdatePlayerStatistics(new UpdatePlayerStatisticsRequest()
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate { StatisticName = mapName, Value=scoreValue},
            }
        },
        result => { Debug.Log("Score updated"); },
        error => { Debug.LogError(error.GenerateErrorReport()); });
    }

    public static void GetPlayerStatistics()
    {
        PlayFabClientAPI.GetPlayerStatistics(new GetPlayerStatisticsRequest()
        {
            StatisticNames = GameController.Instance.MissionController.AvailableMissions.Select(mission => mission.name).ToList()
        },
        result =>
        {
            result.Statistics.ForEach(stat => Statistics.Add(stat.StatisticName, stat.Value));
        },
        error => { });
    }

    public static void GetHighScore(string mapName)
    {
        PlayFabClientAPI.GetLeaderboard(new GetLeaderboardRequest()
        {
            MaxResultsCount = 1,
            StatisticName = mapName
        },
            result =>
            {
                if (TopScores.ContainsKey(mapName))
                    TopScores[mapName] = result.Leaderboard[0].StatValue;
                else
                    TopScores.Add(mapName, result.Leaderboard[0].StatValue);

                PF_Bridge.RaiseCallbackSuccess(mapName, PlayFabAPIMethods.GetFriendsLeaderboard, MessageDisplayStyle.success);
            },
            error => { }
        );
    }

    public static void GetPlayerLeaderboardPosition(string mapName)
    {
        PlayFabClientAPI.GetLeaderboardAroundPlayer(new GetLeaderboardAroundPlayerRequest()
        {
            StatisticName = mapName,
            MaxResultsCount = 1
        },
        result =>
        {
            RankPositions[mapName] = result.Leaderboard[0].Position;
            if (Statistics.ContainsKey(mapName))
                Statistics[mapName] = result.Leaderboard[0].StatValue;
            else
                Statistics.Add(mapName, result.Leaderboard[0].StatValue);

            PF_Bridge.RaiseCallbackSuccess(mapName, PlayFabAPIMethods.GetPlayerLeaderboard, MessageDisplayStyle.success);
        },
        error => { }
        );
    }
}