using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    public float CurrentToMaximumVelocityMagnitudeRatio => (rigidbody.velocity.magnitude / maxVelocityMagnitude);
    public float VelocityToDirectionAngle => Vector3.Angle(rigidbody.velocity.normalized, transform.forward);
    public float VelocityToDirectionSignedAngle => Vector3.SignedAngle(transform.forward, rigidbody.velocity.normalized, transform.up);
    public PlanetController HostPlanet { get; private set; }

    [SerializeField] private float maxVelocityMagnitude = 30f;
    [SerializeField] private GameObject propulsionPS;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private ShipThruster shipThruster;

    private ParticleSystem.EmissionModule propulsionEmission;
    private Rigidbody rigidbody;
    private CameraController cameraController;
    private Vector3 defaultRotationAngles;
    private LandingRewardArgs landingData;
    private bool isLocked = false;
    private bool isMoving = false;
    private bool hasLanded = false;
    private bool isDead = false;
    private bool joystickAvailable => !(transform.parent != null || (isLocked || hasLanded) || Joystick.Instance.Input.magnitude < .2f);

    public UnityEvent OnPlayerDied;
    public UnityAction<PlanetController> OnPlayerLanded;
    public UnityAction<PlanetController> OnPlayerTookOff;

    public PlayerStatistics Stats = new PlayerStatistics(1000, 1200);

    private List<NPCEntity> Passengers = new List<NPCEntity>();
    private ShipModelController shipModelController;
    private Coroutine fuelLoading;
    //private Dictionary<NPCEntity, DeliveryRewardArgs> PassengerRewardDict = new Dictionary<NPCEntity, DeliveryRewardArgs>()

    private AudioSource Audio;
    private AudioSource Audio2;

    [SerializeField] private PlayerEffects playerEffects;
    [SerializeField] private ShipSounds Sounds;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        Audio = gameObject.AddComponent<AudioSource>();
        Audio2 = gameObject.AddComponent<AudioSource>();
        rigidbody = GetComponent<Rigidbody>();
        cameraController = GetComponentInChildren<CameraController>();
        shipModelController = GetComponentInChildren<ShipModelController>();
        propulsionEmission = propulsionPS.GetComponent<ParticleSystem>().emission;
        defaultRotationAngles = transform.rotation.eulerAngles;
        shipModelController.CurrentState = ShipModelController.ShipModelState.Idle;
    }

    private void Start()
    {
        GameController.Instance.MissionController.OnMissionCompleted.AddListener(() =>
        {
            var endgameLandingScore = GameController.Instance.Rewards.GetReward(Reward.RewardType.FuelReward,
                new FuelRewardArgs(Stats.MaxFuel, Stats.Fuel, Stats.TotalFuelUsed));
            Stats.AddScore(endgameLandingScore);
            StopLoadingFuel();
            playerEffects.PlayOutroSequence();
        });

        Stats.OnFuelFull.AddListener(StopLoadingFuel);

        playerEffects.PlayIntroSequence();
    }

    private void Update()
    {
        HandleInput();
    }

    private void FixedUpdate()
    {
        HandleJoystickInput();
        ProcessMovementBuffer();
        Debug.DrawLine(transform.position, transform.position + averagedMovementVector * 100, Color.red);
        Debug.DrawLine(transform.position, transform.position + transform.forward);

        if (isMoving)
        {
            Move();
        }
    }

    #region private methods

    private void HandleInput()
    {
        if (isLocked) return;

        if (Input.GetKey(KeyCode.Space))
        {
            Move();
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(-Vector3.up * 5);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(Vector3.up * 5);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartEngine();
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            StopEngine();
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            foreach (NPCEntity trav in Passengers)
            {
                trav.DestinationPlanet = HostPlanet;
            }
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ReleasePassengers(HostPlanet, true);
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            VFX.PlayTeleportEffect(transform);
        }

        //if(transform.parent != null && Vector3.Distance(transform.position, transform.parent.transform.position) > 80f)
        //{
        //    //rigidbody.velocity += transform.parent.GetComponent<Rigidbody>().velocity;
        //    //rigidbody.angularVelocity += transform.parent.GetComponent<Rigidbody>().angularVelocity;
        //    transform.SetParent(null);

        //}
    }

    private void HandleJoystickInput()
    {
        if (joystickAvailable)
        {
            JoystickRotate();
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
        if (colName == "PlayerLander" && collision.gameObject.tag == "Landable" && gameObject.activeSelf)
        {
            var currentPlanetHost = collision.gameObject.GetComponentInParent<PlanetController>();
            GetLandingData(ref landingData, collision.contacts[0], currentPlanetHost);
            Land(currentPlanetHost);
        }
        else if (!hasLanded && !isDead)
        {
            Kill();
        }
    }

    private void Kill()
    {
        playerEffects.ShowExplosion(transform.position, transform.rotation);
        OnPlayerDied?.Invoke();
        var colliders = transform.GetComponents<Collider>();
        foreach (Collider coll in colliders) Destroy(coll);
        isDead = true;
        SoundManager.Instance.PlayExplosion();
        SoundManager.Instance.PlayMissionFailedTheme();
        gameObject.SetActive(false);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Landable")
        {
            var previousPlanetHost = collision.gameObject.GetComponentInParent<PlanetController>();
            TakeOff(previousPlanetHost);
        }
    }

    private void Land(PlanetController planet)
    {
        hasLanded = true;
        HostPlanet = planet;
        LoadFuel();
        transform.SetParent(HostPlanet.transform);
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;

        if (Passengers.Count > 0)
        {
            ReleasePassengers(HostPlanet);
        }

        shipModelController.CurrentState = ShipModelController.ShipModelState.Landed;
        PlaySceneCanvasController.Instance.ShowLandingInfo(landingData);
        playerEffects.ShowLandingFX(shipThruster.transform.position, transform.rotation);
        Audio2.PlayOneShot(Sounds.LandingSound);
        OnPlayerLanded?.Invoke(planet);
    }

    private void GetLandingData(ref LandingRewardArgs landingData, ContactPoint landingPoint, PlanetController planet)
    {
        var angle = Mathf.Abs(90 - Vector3.Angle(landingPoint.normal, transform.right));
        landingData = new LandingRewardArgs(angle, rigidbody.velocity.magnitude);
    }

    private void LoadFuel()
    {
        playerEffects.ShowFuelLoading(shipThruster.transform.position, transform.rotation);
        fuelLoading = StartCoroutine(FuelLoadingCR());
    }

    private void StopLoadingFuel()
    {
        playerEffects.HideFuelLoading();
        StopCoroutine(fuelLoading);
    }

    private IEnumerator FuelLoadingCR()
    {
        while (hasLanded)
        {
            Stats.AddFuel(GameController.Instance.Settings.FuelLoading * Time.deltaTime);
            yield return null;
        }
    }

    private void TakeOff(PlanetController planet)
    {
        hasLanded = false;
        HostPlanet = null;
        StopLoadingFuel();
        OnPlayerTookOff?.Invoke(planet);
    }

    private void ReleasePassengerToPlanet(NPCEntity entity, PlanetController planet)
    {
        Audio2.PlayOneShot(Sounds.DestinationReached);
        entity.ExitShip(planet);
        RemovePassenger(entity);
    }

    private void ReleasePassengers(PlanetController planet, bool releaseAll = false)
    {
        if (releaseAll)
        {
            for (int i = Passengers.Count - 1; i >= 0; i--)
            {
                ReleasePassengerToPlanet(Passengers[i], HostPlanet);
            }
        }
        else
        {
            var leavers = Passengers.FindAll(x => x.DestinationPlanet == planet);
            if (leavers.Count > 0)
            {
                AddScore(Reward.RewardType.LandingReward, landingData);
                AddScore(Reward.RewardType.FuelReward, new FuelRewardArgs(Stats.MaxFuel, Stats.Fuel));
                //PlaySceneCanvasController.Instance.ShowLandingInfo(this.landingData);
                StartCoroutine(ReleasePassengersCR(leavers, planet));
            }
        }
    }

    private IEnumerator ReleasePassengersCR(List<NPCEntity> leavers, PlanetController planet)
    {
        isLocked = true;
        yield return new WaitForSeconds(0.5f);
        foreach (NPCEntity leaver in leavers)
        {
            ReleasePassengerToPlanet(leaver, planet);
            yield return new WaitForSeconds(1.5f);
        }
        isLocked = false;
    }

    private void Move()
    {
        if (rigidbody.velocity.magnitude < maxVelocityMagnitude)
        {
            rigidbody.AddForce(transform.forward * GameController.Instance.Settings.PlayerAccel * Time.deltaTime, ForceMode.Acceleration);
            Stats.AddFuel(GameController.Instance.Settings.FuelCost * Time.fixedDeltaTime);
        }
    }

    private void StartEngine()
    {
        //propulsionEmission.enabled = true;
        shipModelController.CurrentState = ShipModelController.ShipModelState.Moving;
        shipThruster.SetActive(true);
        Audio.Stop();
        Audio.pitch = Sounds.engineStartPitch + Sounds.RandomPitch;
        Sounds.pitchTween.Kill();
        Sounds.pitchTween = Audio.DOPitch(Sounds.engineEndPitch + Sounds.RandomPitch, Sounds.tweenTime);
        Audio.PlayOneShot(Sounds.Running);
    }

    private void StopEngine()
    {
        //propulsionEmission.enabled = false;
        shipModelController.CurrentState = ShipModelController.ShipModelState.Idle;
        shipThruster.SetActive(false);
        Sounds.pitchTween.Kill();
        Audio.Stop();
        Audio.PlayOneShot(Sounds.StopEngine);
    }

    private void AddScore(Reward.RewardType type, IRewardArgs entityRewardData)
    {
        var score = GameController.Instance.Rewards.GetReward(type, entityRewardData);
        Stats.AddScore(score);
    }

    private void PlayTeleportEffect()
    {
    }

    #endregion private methods

    #region public methods

    public void StartMovement()
    {
        if (!isMoving)
        {
            StartEngine();
            isMoving = true;
        }
    }

    public void StopMovement()
    {
        if (isMoving)
        {
            StopEngine();
            isMoving = false;
        }
    }

    public void AddPassenger(NPCEntity entity)
    {
        Audio2.PlayOneShot(Sounds.GetOnBoard);
        entity.OnReachedDestination.AddListener(() =>
            AddScore(Reward.RewardType.DeliveryReward, entity.DeliveryRewardData));
        Passengers.Add(entity);
    }

    public void RemovePassenger(NPCEntity entity)
    {
        Passengers.Remove(entity);
    }

    #endregion public methods

    #region trashcoding

    public float averagedVelocityMagnitude;
    public Vector3 averagedMovementVector;
    private static readonly int averagedMovementVectorArraySize = 30;
    private static Vector3[] emptyMovementVectorArray = new Vector3[averagedMovementVectorArraySize];
    private CircularBuffer<Vector3> movementBuff = new CircularBuffer<Vector3>(averagedMovementVectorArraySize, emptyMovementVectorArray);

    private void ProcessMovementBuffer()
    {
        movementBuff.PopBack();
        movementBuff.PushFront(transform.forward);
        averagedVelocityMagnitude = GetAveragedVelocityMagnitude();
        averagedMovementVector = GetAveragedMovementVector();
    }

    private float GetAveragedVelocityMagnitude()
    {
        float averagedVel = 0;
        foreach (Vector3 each in movementBuff)
        {
            averagedVel += each.magnitude;
        }
        return (averagedVel / movementBuff.Capacity);
    }

    private Vector3 GetAveragedMovementVector()
    {
        Vector3 vectorSum = Vector3.zero;
        foreach (Vector3 movementVec in movementBuff)
        {
            vectorSum += movementVec;
        }
        return (vectorSum / movementBuff.Capacity);
    }

    public float DelayedForwardAngle => Vector3.SignedAngle(transform.forward, averagedMovementVector, transform.up);

    #endregion trashcoding
}

[Serializable]
public class ShipSounds
{
    public AudioClip StartEngine;
    public AudioClip Running;
    public AudioClip StopEngine;
    public AudioClip LandingSound;

    public AudioClip GetOnBoard;
    public AudioClip DestinationReached;

    public float engineStartPitch = 0.75f;
    public float engineEndPitch = 1.1f;
    public float tweenTime = 4f;

    public float RandomPitch => UnityEngine.Random.Range(-.05f, .05f);

    public Tween pitchTween;
}

public static class VFX
{
    public static void PlayTeleportEffect(Transform transform)
    {
        Rigidbody[] rbs = transform.GetComponentsInChildren<Rigidbody>();
        Collider[] colls = transform.GetComponentsInChildren<Collider>();

        foreach (var rb in rbs)
        {
            rb.isKinematic = true;
        }

        foreach (var coll in colls)
        {
            coll.enabled = false;
        }

        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(0, 1f).SetEase(Ease.OutSine));
    }
}

[Serializable]
public class PlayerEffects
{
    public GameObject LandingFX;
    public GameObject FuelLoadingFX;
    public GameObject ExplosionFX;
    public GameObject BlackHoleFX;

    private GameObject fuelLoadingFX;

    public void ShowLandingFX(Vector3 position, Quaternion rotation)
    {
        var obj = GameObject.Instantiate(LandingFX, position, rotation);
        obj.transform.SetParent(PlayerController.Instance.transform);
        GameObject.Destroy(obj, 2.5f);
    }

    public void ShowFuelLoading(Vector3 position, Quaternion rotation)
    {
        if (fuelLoadingFX == null)
        {
            fuelLoadingFX = GameObject.Instantiate(FuelLoadingFX, position, rotation);
            fuelLoadingFX.transform.SetParent(PlayerController.Instance.transform);
        }
        else
        {
            fuelLoadingFX.SetActive(true);
            fuelLoadingFX.transform.position = position;
            fuelLoadingFX.transform.rotation = rotation;
        }
    }

    public void HideFuelLoading()
    {
        fuelLoadingFX.SetActive(false);
    }

    public void ShowExplosion(Vector3 position, Quaternion rotation)
    {
        var obj = GameObject.Instantiate(ExplosionFX, position, rotation);
        GameObject.Destroy(obj, 2.5f);
    }

    public void PlayIntroSequence()
    {
        var player = PlayerController.Instance.transform;
        var defaultPlayerScale = player.localScale;
        var obj = GameObject.Instantiate(BlackHoleFX, player.position + new Vector3(0, 0, 75f), player.rotation);
        obj.transform.localScale = Vector3.zero;
        player.localScale = Vector3.zero;

        Sequence BHSeq = DOTween.Sequence();
        BHSeq.Append(obj.transform.DOScale(220, .75f).SetEase(Ease.OutBack))
            .AppendInterval(.35f)
            .Append(obj.transform.DOScale(0, .45f).SetEase(Ease.InBack))
            .AppendCallback(() => GameObject.Destroy(obj));

        Sequence playerSeq = DOTween.Sequence();
        playerSeq.AppendInterval(1.2f)
            .Append(player.DOScale(defaultPlayerScale.x, 1f).SetEase(Ease.OutElastic));
    }

    public void PlayOutroSequence()
    {
        var player = PlayerController.Instance.transform;

        Rigidbody[] rbs = player.GetComponentsInChildren<Rigidbody>();
        Collider[] colls = player.GetComponentsInChildren<Collider>();
        var attractor = player.GetComponent<Attractor>();
        attractor.isAffectedByPull = false;
        attractor.isPulling = false;

        foreach (var rb in rbs)
        {
            rb.isKinematic = true;
        }

        foreach (var coll in colls)
        {
            coll.enabled = false;
        }

        var scaler = player.transform.localScale.x * player.transform.parent.transform.localScale.x;
        var obj = GameObject.Instantiate(BlackHoleFX, player.position + new Vector3(0, 0, 25f), player.rotation);

        obj.transform.SetParent(player);
        obj.transform.localScale = Vector3.zero;
        CameraViews.ActiveView.Disable();
        Camera.main.transform.SetParent(null);

        Sequence BHSeq = DOTween.Sequence();
        BHSeq.Append(obj.transform.DOScale(220 / scaler, 1f).SetEase(Ease.OutBack))
            .AppendInterval(.35f)
            .Append(obj.transform.DOScale(0, .45f).SetEase(Ease.InBack))
            .AppendCallback(() => GameObject.Destroy(obj));

        Sequence playerSeq = DOTween.Sequence();
        playerSeq.AppendInterval(.6f)
            //.AppendCallback(() =>
            //{
            //    CameraController.Instance.StopAllCoroutines();
            //    CameraViews.ActiveView.Disable();
            //})
            .Append(player.DOScale(0, 1f).SetEase(Ease.InElastic));
    }
}