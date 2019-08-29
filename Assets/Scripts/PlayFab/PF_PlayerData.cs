using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using UnityEngine;

public class PF_PlayerData : MonoBehaviour
{
    public static string PlayerName;
    public static string PlayfabID;
    public static string PlayfabUsername;

    public static int Coins;
    public static readonly List<ItemInstance> Inventory = new List<ItemInstance>();

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
}