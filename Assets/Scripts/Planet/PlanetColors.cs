using System.Collections.Generic;
using UnityEngine;

public static class PlanetColors
{
    public static Color32 Grey = new Color32(0x8A, 0xFF, 0xB7, 0xFF);
    public static Color32 Yellow = new Color32(0xFF, 0xE8, 0xB7, 0xFF);
    public static Color32 Orange = new Color32(0xFF, 0xBE, 0x87, 0xFF);
    public static Color32 LightRed = new Color32(0xE3, 0x8A, 0x74, 0xFF);
    public static Color32 DarkRed = new Color32(0xBF, 0x59, 0x67, 0xFF);

    public static List<Color32> Colors = new List<Color32> { Grey, Yellow, Orange, LightRed, DarkRed };
}