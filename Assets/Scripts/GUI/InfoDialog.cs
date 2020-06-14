using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InfoDialog : MovableCanvasElement
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI message;
    [SerializeField] private Button okButton;

    public void Show(string title, string message, UnityAction onClickedOK)
    {
        if (gameObject.activeSelf)
        {
            Debug.LogWarning("Multiple info dialogs were called!");
            return;
        }

        this.title.text = title;
        this.message.text = message;
        okButton.onClick.RemoveAllListeners();
        if (onClickedOK != null)
            okButton.onClick.AddListener(onClickedOK);
        okButton.onClick.AddListener(() => base.Hide());
        base.Show();
    }
}