using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    public float CurrentToMaximumVelocityMagnitudeRatio => (rigidbody.velocity.magnitude / maxVelocityMagnitude);
    public float VelocityToDirectionAngle => Vector3.Angle(rigidbody.velocity.normalized, transform.forward);
    public PlanetController HostPlanet { get; private set; }

    [SerializeField]
    private float maxVelocityMagnitude = 30f;
    [SerializeField]
    private float acceleration = 70f;
    [SerializeField]
    private GameObject propulsionPS;

    private ParticleSystem.EmissionModule propulsionEmission;
    private Rigidbody rigidbody;
    private CameraController cameraController;
    private Vector3 defaultRotationAngles;
    private LandingRewardArgs landingData;
    private bool isLocked = false;
    private bool isMoving = false;
    private bool hasLanded = false;
    private bool joystickAvailable => !(this.transform.parent != null || (this.isLocked || this.hasLanded) || Joystick.Instance.Input.magnitude < .2f);

    public UnityEvent OnPlayerDied;
    public UnityAction<PlanetController> OnPlayerLanded;
    public UnityAction<PlanetController> OnPlayerTookOff;

    public PlayerStatistics Stats = new PlayerStatistics(1000,1200);

    private List<NPCEntity> Passengers = new List<NPCEntity>();
    private Coroutine fuelLoading;
    //private Dictionary<NPCEntity, DeliveryRewardArgs> PassengerRewardDict = new Dictionary<NPCEntity, DeliveryRewardArgs>()

    private AudioSource Audio;
    [SerializeField]
    private ShipSounds Sounds;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        Audio = gameObject.AddComponent<AudioSource>();
        rigidbody = GetComponent<Rigidbody>();
        cameraController = GetComponentInChildren<CameraController>();
        propulsionEmission = propulsionPS.GetComponent<ParticleSystem>().emission;
        defaultRotationAngles = transform.rotation.eulerAngles;
    }

    private void Update()
    {
        this.HandleInput();
    }

    private void FixedUpdate()
    {
        this.HandleJoystickInput();
    }

    #region private methods

    private void HandleInput()
    {
        if (this.isLocked) return;

        if (Input.GetKey(KeyCode.Space))
        {
            this.Move();
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(-Vector3.up);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(Vector3.up);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            this.StartEngine();
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            this.StopEngine();
        }
        if(Input.GetKeyDown(KeyCode.O))
        {
            foreach(NPCEntity trav in Passengers)
            {
                trav.DestinationPlanet = this.HostPlanet;
            }
        }
        if(Input.GetKeyDown(KeyCode.Q))
        {
            this.ReleasePassengers(this.HostPlanet, true);
        }

        if(this.isMoving)
        {
            this.Move();
        }

        if(transform.parent != null && Vector3.Distance(transform.position, transform.parent.transform.position) > 80f)
            transform.SetParent(null);
    }

    private void HandleJoystickInput()
    {
        if (joystickAvailable)
        {
            this.JoystickRotate();
        }
    }

    private void JoystickRotate()
    {
        float angle = Vector2.SignedAngle(Vector2.right, Joystick.Instance.CompensatedInput.normalized);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(angle, -90, 90), .12f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        var colName = collision.contacts[0].thisCollider.name;
        if(colName == "PlayerLander")
        {
            if(collision.gameObject.tag == "Landable")
            {  
                var currentPlanetHost = collision.gameObject.GetComponentInParent<PlanetController>();
                this.GetLandingData(ref this.landingData, collision.contacts[0]);
                this.Land(currentPlanetHost);
            }
            else
            {
                OnPlayerDied?.Invoke();
            }
        }
        else
        {
            OnPlayerDied?.Invoke();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.tag == "Landable")
        {
            var previousPlanetHost = collision.gameObject.GetComponentInParent<PlanetController>();
            this.TakeOff(previousPlanetHost);
        }
    }

    private void Land(PlanetController planet)
    {
        this.hasLanded = true;
        this.HostPlanet = planet;
        fuelLoading = StartCoroutine(FuelLoadingCR());
        transform.SetParent(HostPlanet.transform);    

        if (Passengers.Count > 0)
        {
            this.ReleasePassengers(HostPlanet);
        }

        OnPlayerLanded?.Invoke(planet);
    }

    private void GetLandingData(ref LandingRewardArgs landingData, ContactPoint landingPoint)
    {
        var angle = Mathf.Abs(90 - Vector3.Angle(landingPoint.normal, transform.right));
        landingData = new LandingRewardArgs(angle, 0, 0);
    }

    private IEnumerator FuelLoadingCR()
    {
        while(this.hasLanded)
        {
            Stats.AddFuel(GameController.Instance.Settings.FuelLoading * Time.deltaTime);
            yield return null;
        }
    }

    private void TakeOff(PlanetController planet)
    {
        this.hasLanded = false;
        this.HostPlanet = null;
        StopCoroutine(this.fuelLoading);
        OnPlayerTookOff?.Invoke(planet);
    }

    private void ReleasePassengerToPlanet(NPCEntity entity, PlanetController planet)
    {
        entity.ExitShip(planet);
        this.RemovePassenger(entity);
    }

    private void ReleasePassengers(PlanetController planet, bool releaseAll = false)
    {
        if(releaseAll)
        {
            for(int i = Passengers.Count-1; i >= 0; i--)
            { 
                ReleasePassengerToPlanet(Passengers[i], this.HostPlanet);
            }
        }
        else
        {
            var leavers = Passengers.FindAll(x => x.DestinationPlanet == planet);
            if (leavers.Count > 0)
            {
                this.AddScore(Reward.RewardType.LandingReward, this.landingData);
                StartCoroutine(ReleasePassengersCR(leavers, planet));
            }
        }
    }

    private IEnumerator ReleasePassengersCR(List<NPCEntity> leavers, PlanetController planet)
    {
        this.isLocked = true;
        yield return new WaitForSeconds(0.5f);
        foreach (NPCEntity leaver in leavers)
        {
            ReleasePassengerToPlanet(leaver, planet);
             yield return new WaitForSeconds(1.5f);
        }
        this.isLocked = false;
    }

    private void Move()
    {
        if (rigidbody.velocity.magnitude < maxVelocityMagnitude)
        {
            rigidbody.AddForce(transform.forward * acceleration * Time.deltaTime, ForceMode.Acceleration);
            Stats.AddFuel(GameController.Instance.Settings.FuelCost * Time.deltaTime);
        }
    }

    private void StartEngine()
    {
        propulsionEmission.enabled = true;
        Audio.Stop();
        Audio.pitch = Sounds.engineStartPitch;
        Sounds.pitchTween.Kill();
        Sounds.pitchTween = Audio.DOPitch(Sounds.engineEndPitch, Sounds.tweenTime);
        Audio.PlayOneShot(Sounds.Running);
    }

    private void StopEngine()
    {
        propulsionEmission.enabled = false;
        Sounds.pitchTween.Kill();
        Audio.Stop();
        Audio.PlayOneShot(Sounds.StopEngine);
    }

    private void AddScore(Reward.RewardType type, IRewardArgs entityRewardData)
    {
        var score = GameController.Instance.Rewards.GetReward(type, entityRewardData);
        Stats.AddScore(score);
    }

    #endregion

    #region public methods

    public void StartMovement()
    {
        if (!this.isMoving)
        {
            this.StartEngine();
            this.isMoving = true;
        }
    }

    public void StopMovement()
    {
        if (this.isMoving)
        {
            this.StopEngine();
            this.isMoving = false;
        }
    }

    public void AddPassenger(NPCEntity entity)
    {
        entity.OnReachedDestination.AddListener(() => 
            AddScore(Reward.RewardType.DeliveryReward, entity.DeliveryRewardData));
        this.Passengers.Add(entity);
    }

    public void RemovePassenger(NPCEntity entity)
    {
        this.Passengers.Remove(entity);
    }

    #endregion
}

[Serializable]
public class ShipSounds
{
    public AudioClip StartEngine;
    public AudioClip Running;
    public AudioClip StopEngine;

    public float engineStartPitch = 0.75f;
    public float engineEndPitch = 1.1f;
    public float tweenTime = 4f;

    public Tween pitchTween;
}