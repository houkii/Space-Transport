using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class GameController : Singleton<GameController>
{
    [SerializeField]
    public MissionController MissionController;
    [SerializeField]
    public bool DevModeEnabled = true;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            SceneController.Instance.LoadMainMenu();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneController.Instance.LoadLevel();
        }
        if(Input.GetKeyDown(KeyCode.Z))
        {
            PlayerController.Instance.Stats.AddScore(1000);
        }
    }
}