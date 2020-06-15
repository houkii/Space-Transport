using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MainMenuCameraController : MonoBehaviour
{
    public void Update()
    {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * 0.75f);
    }
}
