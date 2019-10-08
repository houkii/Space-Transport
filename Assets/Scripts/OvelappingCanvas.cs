using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class OvelappingCanvas : MovableCanvasElement
{
    [SerializeField] private TextMeshProUGUI message;

    private void Start()
    {
        SceneManager.sceneLoaded += (a, b) => Hide();
    }

    public void Show(string message, UnityAction onShown = null)
    {
        this.message.text = message;
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