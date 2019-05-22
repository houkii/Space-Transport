using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class MenuGameTitle : MovableCanvasElement
{
    [SerializeField] private TextMeshProUGUI top;
    [SerializeField] private TextMeshProUGUI bottom;
    [SerializeField] private MainMenuPanel mainMenu;
    private GridLayoutGroup titleGroup;
    private Vector2 defaultCellSize = new Vector2(255, 45);

    protected override void Awake()
    {
        base.Awake();
        titleGroup = GetComponent<GridLayoutGroup>();
        this.GetTitleSequence();
    }

    private Sequence GetTitleSequence()
    {
        Sequence titleSequence = DOTween.Sequence();
        titleSequence.Append(top.GetComponent<RectTransform>().DOLocalRotate(Vector3.zero, 2f).SetEase(Ease.OutElastic))
                    .Join(bottom.GetComponent<RectTransform>().DOLocalRotate(Vector3.zero, 2f).SetEase(Ease.OutElastic))
                    .AppendCallback(() => mainMenu.Show())
                    .Join(DOTween.To(() => titleGroup.cellSize, x => titleGroup.cellSize = x, defaultCellSize, 15f).SetEase(Ease.InOutSine));

        return titleSequence;
    }
}
