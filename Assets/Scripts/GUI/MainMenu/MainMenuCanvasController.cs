using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MainMenuCanvasController : Singleton<MainMenuCameraController>
{
    [SerializeField] private MainMenuPanel mainMenuPanel;
    [SerializeField] private MenuGameTitle menuGameTitle;
    [SerializeField] private MissionChoosePanelController missionChoosePanel;

    public void Start()
    {
        if(GameController.Instance.DevModeEnabled)
        {
            menuGameTitle.ShowIntro().Complete(true);
        }
        else
        {
            menuGameTitle.ShowIntro();
        }
    }

    public void ShowMissions()
    {
        menuGameTitle.Hide();
        mainMenuPanel.Hide().AppendCallback(() => missionChoosePanel.Show());
    }

    public void ShowMain()
    {
        missionChoosePanel.Hide().AppendCallback(() =>
        {
            menuGameTitle.Show();
            mainMenuPanel.Show();
        });
    }
}
