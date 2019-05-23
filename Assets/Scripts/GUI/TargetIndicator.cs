using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TargetIndicator : MonoBehaviour
{
    private Camera mainCamera;
    private RectTransform m_icon;
    private Image m_iconImage;
    private GameObject targetsHolder;
    private Vector3 m_cameraOffsetUp;
    private Vector3 m_cameraOffsetRight;
    private Vector3 m_cameraOffsetForward;
    public Sprite m_targetIconOnScreen;
    public Sprite m_targetIconOffScreen;
    [Space]
    [Range(0, 100)]
    public float m_edgeBuffer;
    //public Vector3 m_targetIconScale;
    [Space]
    public bool ShowDebugLines;

    [SerializeField, Range(0f, 1f)] private float minScale;
    [SerializeField, Range(0f, 2f)] private float maxScale;
    private Vector3 screenCenter;

    private TextMeshProUGUI iconText;

    void Awake()
    {
        mainCamera = Camera.main;
        screenCenter = new Vector3(0.5f * Screen.height, 0.5f * Screen.width);
        targetsHolder = GameObject.Find("IndicatorsHolder");
        InstainateTargetIcon();
    }

    private void OnEnable()
    {
        this.SetIcon(mainCamera.WorldToViewportPoint(transform.position));
    }

    private void OnDisable()
    {
        m_iconImage.enabled = false;   
    }

    void Update()
    {
        if (ShowDebugLines)
            DrawDebugLines();

        UpdateTargetIcon();
    }


    private void InstainateTargetIcon()
    {
        m_icon = new GameObject().AddComponent<RectTransform>();
        m_icon.transform.SetParent(targetsHolder.transform);
        m_icon.localScale = Vector3.one * maxScale;
        m_icon.name = name + ": target icon";
        //iconText = m_icon.gameObject.AddComponent<TextMeshProUGUI>();
        //iconText.text = transform.name;
        m_iconImage = m_icon.gameObject.AddComponent<Image>();
        m_iconImage.sprite = m_targetIconOnScreen;
        m_iconImage.color = new Color(1, 1, 1, .6f);
    }

    private void UpdateTargetIcon()
    {
        Vector3 newPos = transform.position;
        newPos = mainCamera.WorldToViewportPoint(newPos);
        if (newPos.z < 0)
        {
            newPos.x = 1f - newPos.x;
            newPos.y = 1f - newPos.y;
            newPos.z = 0;
            newPos = Vector3Maxamize(newPos);
        }
        newPos = mainCamera.ViewportToScreenPoint(newPos);

        this.SetIcon(newPos);
        this.SetScale(newPos);
        this.SetColor(newPos);
        this.SetPosition(ref newPos);
    }

    private void SetPosition(ref Vector3 pos)
    {
        pos.x = Mathf.Clamp(pos.x, m_edgeBuffer, Screen.width - m_edgeBuffer);
        pos.y = Mathf.Clamp(pos.y, m_edgeBuffer, Screen.height - m_edgeBuffer);
        m_icon.transform.position = pos;
    }

    private void SetIcon(Vector3 pos)
    {
        if(Utils.IsOutOfView(pos))
        {
            this.ChangeSprite(m_targetIconOffScreen);
        }
        else
        {
            this.ChangeSprite(m_targetIconOnScreen);
        }
    }

    private void ChangeSprite(Sprite _sprite)
    {
        if(_sprite != null)
        {
            m_iconImage.sprite = _sprite;
            m_iconImage.enabled = true;
        }
        else
        {
            m_iconImage.enabled = false;
        }
    }

    private void SetScale(Vector3 pos)
    {
        if (Utils.IsOutOfView(pos))
        {
            float distance = Vector3.Distance(screenCenter, pos);
            float scaler = Screen.height / distance;
            Debug.Log(scaler);
            Vector3 newScale = Vector3.one * maxScale * Mathf.Clamp(scaler, minScale, maxScale);
            m_icon.localScale = Vector3.Lerp(m_icon.localScale, newScale, 0.1f);
        }
        else
        {
            m_icon.localScale = Vector3.Lerp(m_icon.localScale, Vector3.one * maxScale, 0.1f);
        }
    }

    private void SetColor(Vector3 pos)
    {
        if(Utils.IsOutOfView(pos))
        {
            m_iconImage.color = new Color(1, 1, 1, Mathf.Lerp(m_iconImage.color.a, .6f, .02f));
        }
        else
        {
            m_iconImage.color = new Color(1, 1, 1, Mathf.Lerp(m_iconImage.color.a, .25f, .02f));
        }
    }

    public void DrawDebugLines()
    {
        Vector3 directionFromCamera = transform.position - mainCamera.transform.position;
        Vector3 cameraForwad = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;
        Vector3 cameraUp = mainCamera.transform.up;
        cameraForwad *= Vector3.Dot(cameraForwad, directionFromCamera);
        cameraRight *= Vector3.Dot(cameraRight, directionFromCamera);
        cameraUp *= Vector3.Dot(cameraUp, directionFromCamera);
        Debug.DrawRay(mainCamera.transform.position, directionFromCamera, Color.magenta);
        Vector3 forwardPlaneCenter = mainCamera.transform.position + cameraForwad;
        Debug.DrawLine(mainCamera.transform.position, forwardPlaneCenter, Color.blue);
        Debug.DrawLine(forwardPlaneCenter, forwardPlaneCenter + cameraUp, Color.green);
        Debug.DrawLine(forwardPlaneCenter, forwardPlaneCenter + cameraRight, Color.red);
    }


    public Vector3 Vector3Maxamize(Vector3 vector)
    {
        Vector3 returnVector = vector;
        float max = 0;
        max = vector.x > max ? vector.x : max;
        max = vector.y > max ? vector.y : max;
        max = vector.z > max ? vector.z : max;
        returnVector /= max;
        return returnVector;
    }
}