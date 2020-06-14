using DG.Tweening;
using TMPro;

[System.Serializable]
public class TweenableStat : TextMeshProUGUI
{
    public float TweenTime = 2.0f;
    public Ease Ease = Ease.OutExpo;

    private float value;
    public float Value
    {
        get { return this.value; }
        private set
        {
            this.value = value;
            text = string.Format(formatType, value);
        }
    }

    private string formatType;
    private Sequence activeSequence;

    public virtual void Set(float number, string formatType)
    {
        this.formatType = formatType;
        activeSequence.Kill();
        activeSequence = DOTween.Sequence();
        activeSequence.Append(DOTween.To(() => Value, x => Value = x, number, TweenTime).SetEase(Ease))
            .Join(transform.DOShakeScale(TweenTime, .125f, 3, 45));
    }
}