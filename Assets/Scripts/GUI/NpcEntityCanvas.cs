using UnityEngine;
using TMPro;
using DG.Tweening;

public class NpcEntityCanvas : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI name;
    [SerializeField] private Vector3 offset = new Vector3(40, 80, 0);
    private NPCEntity target;

    public SpeechBubble SpeechBubble { get; private set; }

    void Awake()
    {
        SpeechBubble = GetComponentInChildren<SpeechBubble>();
    }

    void Update()
    {
        if(target != null)
        {
            this.transform.position = Camera.main.WorldToScreenPoint(target.transform.position) + offset;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void Initialize(NPCEntity target)
    {
        this.target = target;
        this.name.text = target.name;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        transform.DOScale(Vector3.zero, .35f).SetEase(Ease.InExpo).OnComplete(() =>
        {
            transform.localScale = Vector3.one * .7f;
            gameObject.SetActive(false);
        });
    }
}
