using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public static class Authentication
{
    public static string PlayfabTitleId = "FC5FE";

    private static string _playFabPlayerIdCache;

    public delegate void SuccessfulPhotonAuthenticationHandler();

    public static event SuccessfulPhotonAuthenticationHandler OnPhotonAuthenticationSuccess;

    public delegate void FailedPhotonAuthenticationHandler();

    public static event FailedPhotonAuthenticationHandler OnPhotonAuthenticationFail;

    public delegate void LogInfoHandler(string info);

    public static event LogInfoHandler OnLogAuthenticationInfo;

    public static void Login()
    {
        AuthenticateWithPlayFab();
    }

    public static void LoginWithCredentials(string userName, string password)
    {
        LogMessage("PlayFab authenticating using credentials...");
        PlayFabClientAPI.LoginWithPlayFab(new LoginWithPlayFabRequest()
        {
            Username = userName,
            Password = password,
            TitleId = PlayfabTitleId
        }, OnPlayfabSuccess,
        error =>
        {
            Debug.LogError(error.ErrorMessage);
            LogMessage(error.ErrorMessage);
            if (error.ErrorMessage == "User not found")
            {
                CreateAccount(userName, password);
            }
        });
    }

    private static void CreateAccount(string userName, string password)
    {
        PlayFabClientAPI.RegisterPlayFabUser(new RegisterPlayFabUserRequest()
        {
            Username = userName,
            Password = password,
            RequireBothUsernameAndEmail = false
        },
        result =>
        {
            LoginWithCredentials(userName, password);
            LogMessage("Account Created!");
        },
        failure =>
        {
            Debug.LogError(failure.ErrorMessage);
            LogMessage(failure.ErrorMessage);
        });
    }

    private static void AuthenticateWithPlayFab()
    {
        LogMessage("PlayFab authenticating using Custom ID...");
        PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest()
        {
            CreateAccount = true,
            CustomId = PlayFabSettings.DeviceUniqueIdentifier
        }, OnPlayfabSuccess, OnPlayFabError);
    }

    private static void OnPlayfabSuccess(LoginResult obj)
    {
        Debug.Log("Playfab connected!");
    }

    private static void OnPlayFabError(PlayFabError obj)
    {
        Debug.LogError("Couldn't connect PlayFab. " + obj.ErrorMessage);
        LogMessage(obj.GenerateErrorReport());
    }

    public static void LogMessage(string message)
    {
        Debug.Log(message);
        OnLogAuthenticationInfo?.Invoke(message);
    }
}