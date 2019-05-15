using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;


/// <summary>
/// Virtual Joystick class.
/// </summary>

public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public enum JoyType { Trigger, Continous };
    public static Joystick Instance;

    [SerializeField]
    private Image VJoyImage;
    [SerializeField]
    private Image VJoyContainerImage;
    [SerializeField]
    private float LerpSpeed = 15.0f;
    [SerializeField]
    private JoyType type;
    private RectTransform rectTransform;

    private Vector3 _Input;
    public Vector2 Input
    {
        get { return _Input; }
        set { _Input = value; }
    }

    public Vector2 CompensatedInput => CompensateByAngle(this.Input, Camera.main.transform.rotation.eulerAngles.z);

    public bool InUse { get; private set; }
    public bool isSwapped = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        this.rectTransform = GetComponent<RectTransform>();
        this.InUse = false;
        this.Input = Vector2.zero;
    }

    private void OnDisable()
    {
        OnPointerUp(null);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        InUse = true;
        OnDrag(eventData);
    }

    private Vector2 ConvertPointToLocalSpace(Vector2 point)
    {
        Vector2 position = point;
        RectTransformUtility.ScreenPointToLocalPointInRectangle
            (this.rectTransform,
            point,
            null,
            out position);

        float xMin = this.VJoyContainerImage.rectTransform.sizeDelta.x / 2;
        float xMax = this.rectTransform.rect.width;
        float yMin = this.VJoyContainerImage.rectTransform.sizeDelta.y / 2;
        float yMax = this.rectTransform.rect.height - this.VJoyContainerImage.rectTransform.sizeDelta.y / 2;

        position = new Vector2(Mathf.Clamp(position.x, xMin, xMax), Mathf.Clamp(position.y, yMin, yMax));

        return position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Input = GetVirtualInput(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        InUse = false;

        if (type == JoyType.Continous)
            Input = Vector2.zero;

        this.VJoyImage.rectTransform.anchoredPosition = Vector2.zero;
    }

    private Vector2 GetVirtualInput(PointerEventData eventData)
    {

        Vector2 position = Vector2.zero;
        Vector3 _VI;

        position.x = eventData.position.x - VJoyContainerImage.rectTransform.position.x;
        position.y = eventData.position.y - VJoyContainerImage.rectTransform.position.y;

        position.x = (position.x / VJoyContainerImage.rectTransform.sizeDelta.x);
        position.y = (position.y / VJoyContainerImage.rectTransform.sizeDelta.y);

        float x = position.x * 2;
        float y = position.y * 2;

        _VI = new Vector3(x, y, 0);
        _VI = (_VI.magnitude > 1) ? _VI.normalized : _VI;

        VJoyImage.rectTransform.anchoredPosition = new Vector3(_VI.x * (VJoyContainerImage.rectTransform.sizeDelta.x / 2)
                                                               , _VI.y * VJoyContainerImage.rectTransform.sizeDelta.y / 2);

        if (this.isSwapped)
        {
            _VI = -_VI;
        }

        return new Vector2(_VI.x, _VI.y);
    }

    private Vector2 CompensateByAngle(Vector2 direction, float angleInDegrees)
    {
        float angleRad = Mathf.Deg2Rad * angleInDegrees;
        float cos = Mathf.Cos(angleRad);
        float sin = Mathf.Sin(angleRad);
        float x = direction.x * cos - direction.y * sin;
        float y = direction.x * sin + direction.y * cos;
        return new Vector2(x, y);
    }
}

