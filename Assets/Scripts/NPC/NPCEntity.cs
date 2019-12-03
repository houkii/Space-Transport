using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class NPCEntity : MonoBehaviour
{
    [SerializeField]
    private float speedModifier = 0.2f;

    private Animator Animator;
    private Transform movementTarget = null;
    private TargetIndicator targetIndicator;

    private PlanetController hostPlanet;

    public PlanetController HostPlanet
    {
        get { return hostPlanet; }
        set { SetHostPlanet(value); }
    }

    private NpcActions.Action currentAction;

    public NpcActions.Action CurrentAction
    {
        get { return currentAction; }
        set { SetCurrentAction(value); }
    }

    public Transform MovementDestination { get; private set; }

    public PlanetController DestinationPlanet { get; set; }
    public NpcEntityCanvas View { get; private set; }

    public DeliveryRewardArgs DeliveryRewardData { get; private set; }

    // Npc events
    public UnityEvent OnGotAboard;

    public UnityEvent OnReachedDestination;

    public event Action OnExitShip;

    private bool IsAboard => HostPlanet == null;
    private bool isAttached;
    public string CurrentActionString;

    private Vector3 defaultScale;

    [SerializeField] private AudioClip getOnBoard;
    [SerializeField] private AudioClip destinationReached;

    private void Awake()
    {
        gameObject.name = NpcNames.RandomName;
        Animator = GetComponent<Animator>();
        Animator.enabled = false;
        View = PlaySceneCanvasController.Instance.AddNpcCanvas(this);
        //targetIndicator = GetComponent<TargetIndicator>();
    }

    private void Start()
    {
        defaultScale = transform.localScale;
        //ShowNPC();
    }

    private void OnEnable()
    {
        PlayerController.Instance.OnPlayerLanded += HandlePlayerLanding;
        PlayerController.Instance.OnPlayerTookOff += HandlePlayerTakingOff;
        View.Show();
    }

    private void OnDisable()
    {
        PlayerController.Instance.OnPlayerTookOff -= HandlePlayerTakingOff;
        PlayerController.Instance.OnPlayerLanded -= HandlePlayerLanding;

        if (View != null)
            View.Hide();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Animator.enabled = true;
        if (collision.gameObject.layer.Equals(LayerMask.NameToLayer("Planet")) && !isAttached)
        {
            if (HostPlanet == DestinationPlanet)
            {
                //speedModifier *= 1.2f;
                CurrentAction = NpcActions.ActionFactory.GetAction(NpcActions.ActionType.MoveAway);
                //targetIndicator.DestroySelf();
            }
            else if (PlayerController.Instance.HostPlanet == HostPlanet)
            {
                CurrentAction = NpcActions.ActionFactory.GetAction(NpcActions.ActionType.MoveToShip);
            }
            else
            {
                CurrentAction = NpcActions.ActionFactory.GetAction(NpcActions.ActionType.Wonder);
            }

            isAttached = true;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer.Equals(LayerMask.NameToLayer("Planet")))
        {
            ResolveNPCRotation(collision.contacts[0].normal);

            if (!isAttached)
                OnCollisionEnter(collision);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (CurrentAction != null)
        {
            CurrentAction.ProcessTriggerCollision(other);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        Animator.enabled = false;
    }

    #region private methods

    private void HandlePlayerTakingOff(PlanetController planetPlayerTookOffFrom)
    {
        if (planetPlayerTookOffFrom == HostPlanet && CurrentAction.Type != NpcActions.ActionType.Wonder)
        {
            CurrentAction = NpcActions.ActionFactory.GetAction(NpcActions.ActionType.Wonder);
        }
    }

    private void HandlePlayerLanding(PlanetController planetPlayerLandedOn)
    {
        if (planetPlayerLandedOn == HostPlanet)
        {
            CurrentAction = NpcActions.ActionFactory.GetAction(NpcActions.ActionType.MoveToShip);
        }
    }

    private IEnumerator MoveToCR(Transform destination)
    {
        Animator.ResetTrigger("Idle");
        yield return new WaitForSeconds(1f);
        Animator.enabled = true;
        movementTarget = destination;
        Animator.ResetTrigger("Idle");
        Animator.SetTrigger("Run");
        while (true)
        {
            yield return new WaitForFixedUpdate();
            transform.position += transform.forward * speedModifier * Time.deltaTime;
            yield return null;
        }
    }

    private void ResolveNPCRotation(Vector3 contactPointNormal)
    {
        Quaternion newRotation;
        if (movementTarget != null)
        {
            // allign object to face destination
            newRotation = Quaternion.LookRotation(movementTarget.position - transform.position, contactPointNormal);
        }
        else
        {
            // allign object rotation to surface normal
            newRotation = Quaternion.FromToRotation(Vector3.up, contactPointNormal);
        }

        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, .4f);
    }

    private void SetHostPlanet(PlanetController planet)
    {
        hostPlanet = planet;
        transform.SetParent(planet.transform);
    }

    private void SetCurrentAction(NpcActions.Action action)
    {
        if (currentAction == null || (!currentAction.IsLocked && currentAction.Type != action.Type))
        {
            currentAction = action;
            currentAction.Process(this);
            CurrentActionString = currentAction.ToString();
        }
    }

    private void HideNPC()
    {
        SetColliders(false);
        transform.DOScale(0, .65f).SetEase(Ease.OutBack).OnComplete(() => gameObject.SetActive(false));
    }

    private void ShowNPC()
    {
        SetColliders(true);
        gameObject.SetActive(true);
        transform.localScale = defaultScale;
        //transform.DOScale(defaultScale.x, .65f).SetEase(Ease.OutCirc);
    }

    private void SetColliders(bool active)
    {
        GetComponent<Rigidbody>().isKinematic = !active;
        foreach (var coll in transform.GetComponents<Collider>())
            coll.enabled = active;
    }

    #endregion private methods

    #region public methods

    public void EnterShip()
    {
        StopAllCoroutines();
        PlaySceneCanvasController.Instance.TravellersPanelController.AddEntry(this);
        PlayerController.Instance.AddPassenger(this);
        DeliveryRewardData = new DeliveryRewardArgs(
            Time.time + GameController.Instance.Settings.OxygenTimer,
            GameController.Instance.Settings.OxygenTimer);

        HostPlanet.CurrentTraveller = null;
        Animator.enabled = false;
        OnGotAboard?.Invoke();
        isAttached = false;
        CurrentAction = NpcActions.ActionFactory.GetAction(NpcActions.ActionType.None);

        //gameObject.SetActive(false);
        HideNPC();
    }

    public void ExitShip(PlanetController planet)
    {
        PlaySceneCanvasController.Instance.TravellersPanelController.RemoveEntryOfNpc(this);
        HostPlanet = planet;
        ShowNPC();
        transform.rotation = Quaternion.identity;
        transform.position = HostPlanet.Waypoints[UnityEngine.Random.Range(0, HostPlanet.Waypoints.Count)].position;
        //transform.position = HostPlanet.SpawnPosition.position;
        //gameObject.SetActive(true);
        //DeliveryRewardData.DeliveryTime = Time.time;
        OnExitShip?.Invoke();
    }

    public void Initialize(PlanetController host, PlanetController destination)
    {
        HostPlanet = host;
        DestinationPlanet = destination;
    }

    public void MoveTo(Transform destination)
    {
        StopAllCoroutines();
        MovementDestination = destination;
        StartCoroutine(MoveToCR(destination));
    }

    private void Update()
    {
        if (HostPlanet == DestinationPlanet && CurrentAction.Type != NpcActions.ActionType.MoveAway)
        {
            StopAllCoroutines();
            CurrentAction = NpcActions.ActionFactory.GetAction(NpcActions.ActionType.MoveAway);
        }
    }

    #endregion public methods
}