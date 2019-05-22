﻿using UnityEngine;
using System.Collections;

public class Utils : MonoBehaviour
{
    public static Vector2 GetRotatedPosition(Vector2 vec, float angle)
    {
        float angleRad = Mathf.Deg2Rad * angle;
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);
        float x = vec.x * cos - vec.y * sin;
        float y = vec.x * sin + vec.y * cos;
        return new Vector2(x, y);
    }
}
