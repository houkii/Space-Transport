using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : MonoBehaviour
{
    [SerializeField]
    private Button PlayButton;
    [SerializeField]
    private Button QuitButton;

    private void Awake()
    {
        PlayButton.onClick.AddListener(StartGame);
        QuitButton.onClick.AddListener(QuitGame);
    }

    private void StartGame()
    {
        SceneController.Instance.LoadLevel();
    }

    private void QuitGame()
    {
        Application.Quit();
    }

}
