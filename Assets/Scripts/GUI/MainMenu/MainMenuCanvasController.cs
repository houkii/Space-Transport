using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;

public class MainMenuCanvasController : Singleton<MainMenuCameraController>
{
    [SerializeField] private MainMenuPanel menuPanel;
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
        menuPanel.Hide().AppendCallback(() => missionChoosePanel.Show());
    }

    public void ShowMain()
    {
        missionChoosePanel.Hide().AppendCallback(() =>
        {
            menuGameTitle.Show();
            menuPanel.Show();
        });
    }
}
