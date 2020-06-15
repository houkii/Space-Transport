using DG.Tweening;
using TMPro;
using UnityEngine;

public class UIRewardEntry : MonoBehaviour
{
    public int direction;

    [SerializeField] private TextMeshProUGUI rewardName;
    [SerializeField] private TextMeshProUGUI rewardValue;
    private RectTransform rt;
    private Reward reward;

    public void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Initialize(Reward reward, int direction)
    {
        this.reward = reward;
        this.direction = direction;
    }

    public void Show()
    {
        var rewardTypeName = reward.GetType().ToString();
        string rewName = rewardTypeName.Remove(rewardTypeName.IndexOf("Reward"), "Reward".Length);

        gameObject.SetActive(true);
        rewardName.text = rewName + ":";
        rewardValue.text = "+" + reward.Value.ToString();
        SetRandomColor();
        rt = transform.GetComponent<RectTransform>();

        transform.localScale = Vector3.zero;
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(Vector3.one, .25f).SetEase(Ease.OutExpo))
            .Append(rt.DOAnchorPosY(rt.anchoredPosition.y + 40f, 1.25f).SetEase(Ease.OutCirc))
            .Join(rt.DOAnchorPosX(rt.anchoredPosition.x + direction * 55f, 1.5f).SetEase(Ease.OutCirc))
            .Join(rt.DORotate(new Vector3(0, 0, -direction * 17f), 1f).SetEase(Ease.OutCubic))
            .Join(rewardName.DOFade(0, 2).SetEase(Ease.InExpo))
            .Join(rewardValue.DOFade(0, 2).SetEase(Ease.InExpo))
            .Join(transform.DOScale(Vector3.zero, 2.25f).SetEase(Ease.InOutQuint))
            .AppendCallback(() => Destroy(gameObject));
    }

    private void SetRandomColor()
    {
        rewardName.color = GameController.Instance.ColorPaletteDarkPink.RandomColor;
        rewardValue.color = GameController.Instance.ColorPaletteLightPink.RandomColor;
    }
}