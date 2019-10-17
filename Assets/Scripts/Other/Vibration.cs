using UnityEngine;

public static class Vibration
{
    public static AndroidJavaClass unityPlayer;
    public static AndroidJavaObject currentActivity;
    public static AndroidJavaObject vibrator;
    public static AndroidJavaClass vibrationEffectClass;
    public static int defaultAmplitude;
    public static bool isEnabled = true;

    public static void Initialize(int apiLevel)
    {
#if UNITY_ANDROID && !UNITY_EDITOR

    unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");

    if(apiLevel > 25)
    {
        vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect");
        defaultAmplitude = vibrationEffectClass.GetStatic<int>("DEFAULT_AMPLITUDE");
    }
#endif
    }

    public static void Vibrate(long milliseconds)
    {
        if (CanVibrate() && isEnabled)
        {
            if (GameController.Instance.androidApiLevel > 25)
            {
                CreateVibrationEffect("createOneShot", new object[] { milliseconds, defaultAmplitude });
            }
            else
            {
                vibrator.Call("vibrate", milliseconds);
            }
        }
    }

    private static void CreateVibrationEffect(string function, params object[] args)
    {
        AndroidJavaObject vibrationEffect = vibrationEffectClass.CallStatic<AndroidJavaObject>(function, args);
        vibrator.Call("vibrate", vibrationEffect);
    }

    public static bool HasVibrator()
    {
        return vibrator.Call<bool>("hasVibrator");
    }

    private static bool isAndroid()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
	    return true;
#else
        return false;
#endif
    }

    private static bool CanVibrate()
    {
        if (isAndroid())
            return HasVibrator();
        else
            return false;
    }
}