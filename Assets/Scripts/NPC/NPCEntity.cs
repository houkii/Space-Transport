using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using System;

public class NPCEntity : MonoBehaviour
{
    [SerializeField]
    private float speedModifier = 0.2f;
    private Animator Animator;
    private Transform movementTarget = null;

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

    public PlanetController DestinationPlanet { get; set; }
    public NpcEntityCanvas View { get; private set; }

    public DeliveryRewardArgs DeliveryRewardData { get; private set; }

    // Npc events
    public UnityEvent OnGotAboard;
    public UnityEvent OnReachedDestination;
    public event Action OnExitShip;

    private bool IsAboard => HostPlanet == null;

    private void Awake()
    {
        gameObject.name = NpcNames.RandomName;
        Animator = GetComponent<Animator>();
        Animator.enabled = false;
        this.View = PlaySceneCanvasController.Instance.AddNpcCanvas(this);
    }

    private void OnEnable()
    {
        PlayerController.Instance.OnPlayerLanded += HandlePlayerLanding;
        PlayerController.Instance.OnPlayerTookOff += HandlePlayerTakingOff;
        this.View.Show();
    }

    private void OnDisable()
    {
        PlayerController.Instance.OnPlayerTookOff -= HandlePlayerTakingOff;
        PlayerController.Instance.OnPlayerLanded -= HandlePlayerLanding;

        if(this.View != null)
            this.View.Hide();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Planet")
        {
            if (HostPlanet == DestinationPlanet)
            {
                CurrentAction = NpcActions.ActionFactory.GetAction(NpcActions.ActionType.MoveAway);
            }
            else if (PlayerController.Instance.HostPlanet == this.HostPlanet)
            {
                CurrentAction = NpcActions.ActionFactory.GetAction(NpcActions.ActionType.MoveToShip);
            }
            else
            {
                CurrentAction = NpcActions.ActionFactory.GetAction(NpcActions.ActionType.Wonder);
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Planet")
        {
            this.ResolveNPCRotation(collision.contacts[0].normal);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (CurrentAction != null)
        {
            CurrentAction.ProcessTriggerCollision(other);
        }
    }

    #region private methods

    private void HandlePlayerTakingOff(PlanetController planetPlayerTookOffFrom)
    {
        if(planetPlayerTookOffFrom == HostPlanet && CurrentAction.Type != NpcActions.ActionType.Wonder)
        {
            CurrentAction = NpcActions.ActionFactory.GetAction(NpcActions.ActionType.Wonder);
        }
    }

    private void HandlePlayerLanding(PlanetController planetPlayerLandedOn)
    {
        if(planetPlayerLandedOn == HostPlanet)
        {
            CurrentAction = NpcActions.ActionFactory.GetAction(NpcActions.ActionType.MoveToShip);
        }
    }

    private IEnumerator MoveToCR(Transform destination)
    {
        Animator.ResetTrigger("Idle");
        yield return new WaitForSeconds(.5f);
        Animator.enabled = true;
        movementTarget = destination;
        Animator.ResetTrigger("Idle");
        Animator.SetTrigger("Run");
        while (true)
        {
            yield return new WaitForFixedUpdate();
            transform.position += transform.forward * speedModifier * Time.fixedDeltaTime;
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
        }
    }

    #endregion

    #region public methods

    public void EnterShip()
    {
        StopAllCoroutines();
        PlaySceneCanvasController.Instance.TravellersPanelController.AddEntry(this);
        PlayerController.Instance.AddPassenger(this);
        this.DeliveryRewardData = new DeliveryRewardArgs(Time.time + 120);
        this.HostPlanet.CurrentTraveller = null;
        this.Animator.enabled = false;
        this.OnGotAboard?.Invoke();
        
        gameObject.SetActive(false);
    }

    public void ExitShip(PlanetController planet)
    {
        PlaySceneCanvasController.Instance.TravellersPanelController.RemoveEntryOfNpc(this);
        HostPlanet = planet;
        transform.rotation = Quaternion.identity;
        transform.position = HostPlanet.ReleaseSpot.position;
        gameObject.SetActive(true);
        this.DeliveryRewardData.DeliveryTime = Time.time;
        this.OnExitShip?.Invoke();
    }

    public void Initialize(PlanetController host, PlanetController destination)
    {
        this.HostPlanet = host;
        this.DestinationPlanet = destination;
    }

    public void MoveTo(Transform destination)
    {
        StopAllCoroutines();
        StartCoroutine(MoveToCR(destination));
    }

    #endregion
}