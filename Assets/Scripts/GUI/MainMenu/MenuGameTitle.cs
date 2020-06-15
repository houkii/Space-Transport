using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    }

    public Sequence ShowIntro()
    {
        return GetTitleSequence();
    }

    private Sequence GetTitleSequence()
    {
        Sequence titleSequence = DOTween.Sequence();
        titleSequence.Append(top.GetComponent<RectTransform>().DOLocalRotate(Vector3.zero, 3.5f).SetEase(Ease.OutElastic))
                .Join(bottom.GetComponent<RectTransform>().DOLocalRotate(Vector3.zero, 3.5f).SetEase(Ease.OutElastic))
                .AppendCallback(() => mainMenu.Show())
                .Join(DOTween.To(() => titleGroup.cellSize,
                                x => titleGroup.cellSize = x,
                                defaultCellSize, 12f)
                                .SetEase(Ease.InOutSine)
                                .SetLoops(int.MaxValue, LoopType.Yoyo));

        return titleSequence;
    }
}