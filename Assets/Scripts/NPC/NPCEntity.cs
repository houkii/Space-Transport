using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class NPCEntity : MonoBehaviour
{
    [SerializeField]
    private float speedModifier = 0.2f;

    protected Animator Animator;
    //protected Vector3 MovementTarget = Vector3.zero;
    private Transform movementTarget = null;

    protected bool IsGrounded = false;
    public PlanetController HostPlanet;
    public PlanetController DestinationPlanet;

    NpcActions.Action currentAction;
    NpcActions.Action CurrentAction
    {
        get { return currentAction; }
        set
        {
            if(currentAction == null || (!currentAction.IsLocked && currentAction.Type != value.Type))
            {
                currentAction = value;
                currentAction.Process(this);
            }
        }
    }

    public NpcEntityCanvas View { get; private set; }

    public UnityEvent OnGotAboard;
    public UnityEvent OnReachedDestination;
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
        this.View.Hide();
    }

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
        //else if(this.IsAboard && planetPlayerLandedOn == DestinationPlanet)
        //{
        //    CurrentAction = NpcActions.ActionFactory.GetAction(NpcActions.ActionType.MoveAway);
        //}
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Planet")
        {
            Animator.enabled = true;
            HostPlanet = collision.gameObject.GetComponent<PlanetController>();
            transform.SetParent(HostPlanet.transform);

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
        if(CurrentAction != null)
        {
            CurrentAction.ProcessTriggerCollision(other);
        }
    }

    public void MoveTo(Transform destination)
    {
        StopAllCoroutines();
        StartCoroutine(MoveToCR(destination));
    }

    protected IEnumerator MoveToCR(Transform destination)
    {
        Animator.SetTrigger("Idle");
        yield return new WaitForSeconds(1.5f);
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

        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, .15f);
    }

    public void EnterShip()
    {
        StopAllCoroutines();
        Debug.Log("Entering ship...\nGet Me to: " + DestinationPlanet.name);
        PlayerController.Instance.AddPassenger(this);
        this.HostPlanet.CurrentTraveller = null;
        this.HostPlanet = null;
        this.IsGrounded = false;
        this.Animator.enabled = false;
        this.OnGotAboard?.Invoke();
        gameObject.SetActive(false);
    }

    public void ExitShip(PlanetController planet)
    {
        gameObject.SetActive(true);
        HostPlanet = planet;
        //transform.rotation = Quaternion.FromToRotation(Vector3.up, (HostPlanet.ReleaseSpot.transform.up - HostPlanet.ReleaseSpot.position));
        transform.position = HostPlanet.ReleaseSpot.position;
    }
}