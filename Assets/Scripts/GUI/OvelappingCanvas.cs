using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class OvelappingCanvas : MovableCanvasElement
{
    [SerializeField] private TextMeshProUGUI message;
    private Tween textTween;

    private void Start()
    {
        SceneManager.sceneLoaded += (a, b) => Hide();
    }

    public void Show(string message, UnityAction onShown = null)
    {
        this.message.text = message;
        textTween.Kill();
        this.message.color = new Color(this.message.color.r, this.message.color.g, this.message.color.b, 1);
        textTween = this.message.DOFade(.55f, .5f).SetEase(Ease.OutExpo).SetLoops(int.MaxValue, LoopType.Yoyo);
        //if (onShown != null)
        //    OnShown.AddListener(onShown);
        base.Show().OnComplete(() => onShown?.Invoke());
    }

    public override Sequence Hide()
    {
        OnShown.RemoveAllListeners();
        return base.Hide();
    }
}