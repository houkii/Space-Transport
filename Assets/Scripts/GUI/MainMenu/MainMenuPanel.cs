using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : MovableCanvasElement
{
    [SerializeField]
    private Button playButton;
    [SerializeField]
    private Button quitButton;

    private Vector2 defaultDeltaSize;
    private GridLayoutGroup buttonGroup;
    private Vector2 defaultLayoutGroupSpacing;
    private Vector2 defaultLayoutGroupCellSize;
    private RectTransform RT;

    protected override void Awake()
    {
        base.Awake();

        RT = GetComponent<RectTransform>();
        defaultDeltaSize = RT.sizeDelta;
        RT.sizeDelta = new Vector2(250, 65);

        playButton.onClick.AddListener(SoundManager.Instance.PlayForwardButton);
        quitButton.onClick.AddListener(QuitGame);
        quitButton.onClick.AddListener(SoundManager.Instance.PlayBackButton);

        buttonGroup = GetComponent<GridLayoutGroup>();
        defaultLayoutGroupCellSize = buttonGroup.cellSize;
        defaultLayoutGroupSpacing = buttonGroup.spacing;
        buttonGroup.spacing = new Vector2(0,-45f);
        buttonGroup.cellSize = new Vector2(175,-45f);
    }

    private void StartGame()
    {
        SceneController.Instance.LoadLevel();
    }

    private void QuitGame()
    {
        Application.Quit();
    }

    public override Sequence Show()
    {
        var mainMenuPanelSequence = base.Show().Append(
            RT.DOSizeDelta(defaultDeltaSize, .5f).SetEase(Ease.InExpo))
            .Append(DOTween.To(() => buttonGroup.spacing,
                    x => buttonGroup.spacing = x,
                    defaultLayoutGroupSpacing,
                    .2f).SetEase(Ease.InExpo))

            .Append(DOTween.To(() => buttonGroup.cellSize,
                        x => buttonGroup.cellSize = x,
                        defaultLayoutGroupCellSize,
                        .3f).SetEase(Ease.OutExpo));

        return mainMenuPanelSequence;
    }
}
