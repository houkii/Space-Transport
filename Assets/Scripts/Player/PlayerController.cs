﻿using DG.Tweening;
using PlayFab;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    public float averagedVelocityMagnitude;
    public Vector3 averagedMovementVector;
    public PlanetController HostPlanet { get; private set; }
    public Vector3 DefaultPlayerScale { get; private set; }
    public UnityEvent OnPlayerDied;
    public UnityAction<PlanetController> OnPlayerLanded;
    public UnityAction<PlanetController> OnPlayerTookOff;
    public UnityEvent OnFuelExhausted;
    public PlayerStatistics Stats;
    public UnityEvent OnNewPersonalHighScore = new UnityEvent();
    public UnityEvent OnNewGlobalHighScore = new UnityEvent();
    public PlanetController PreviousPlanetHost;

    [SerializeField] private float maxVelocityMagnitude = 30f;
    [SerializeField] private GameObject propulsionPS;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private ShipThruster shipThruster;
    [SerializeField] private PlayerEffects playerEffects;
    [SerializeField] private ShipSounds Sounds;
    private ParticleSystem.EmissionModule propulsionEmission;
    private Rigidbody rigidbody;
    private CameraController cameraController;
    private Vector3 defaultRotationAngles;
    private LandingRewardArgs landingData;
    private bool isLocked = false;
    private bool isMoving = false;
    private bool hasLanded = false;
    private bool isDead = false;
    private List<NPCEntity> Passengers = new List<NPCEntity>();
    private ShipModelController shipModelController;
    private Coroutine fuelLoading;
    private AudioSource Audio;
    private AudioSource Audio2;
    private static readonly int averagedMovementVectorArraySize = 30;
    private static Vector3[] emptyMovementVectorArray = new Vector3[averagedMovementVectorArraySize];
    private CircularBuffer<Vector3> movementBuff = new CircularBuffer<Vector3>(averagedMovementVectorArraySize, emptyMovementVectorArray);

    public float CurrentToMaximumVelocityMagnitudeRatio => (rigidbody.velocity.magnitude / maxVelocityMagnitude);
    public float VelocityToDirectionAngle => Vector3.Angle(rigidbody.velocity.normalized, transform.forward);
    public float VelocityToDirectionSignedAngle => Vector3.SignedAngle(transform.forward, rigidbody.velocity.normalized, transform.up);
    public float DelayedForwardAngle => Vector3.SignedAngle(transform.forward, averagedMovementVector, transform.up);
    public bool IsDead => isDead;
    private bool joystickAvailable => !(transform.parent != null || (isLocked || hasLanded) || Joystick.Instance.Input.magnitude < .2f);


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

        DefaultPlayerScale = transform.localScale;
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
            StartCoroutine(MissionEndedCR());
        });

        CameraController.Instance.OnStandardViewSet.AddListener(CorrectVelocity);
        Stats.OnFuelFull.AddListener(StopLoadingFuel);
        StartCoroutine(WaitForTutorialCompletion());
    }

    private void Update()
    {
        HandleInput();
        ProximityCheck();
    }

    private void FixedUpdate()
    {
        HandleJoystickInput();
        ProcessMovementBuffer();

        if (isMoving)
            Move();
    }

    public void Kill()
    {
        playerEffects.ShowExplosion(transform.position, transform.rotation);
        Camera.main.transform.DOShakeRotation(.45f, 5f, 30).OnComplete(() => OnPlayerDied?.Invoke());
        var colliders = transform.GetComponents<Collider>();
        foreach (Collider coll in colliders) Destroy(coll);
        isDead = true;
        SoundManager.Instance.PlayExplosion();
        SoundManager.Instance.PlayMissionFailedTheme();
        Vibration.Vibrate(50);
        gameObject.SetActive(false);
    }

    public void StartMovement()
    {
        if (!isMoving && Stats.Fuel > 0)
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
        Vibration.Vibrate(15);
        playerEffects.ShowTravellerEnterFX(shipThruster.transform.position, transform.rotation);
    }

    public void RemovePassenger(NPCEntity entity)
    {
        Passengers.Remove(entity);
    }

    private void ProximityCheck()
    {
        if (transform.position.magnitude > GameController.Instance.MissionController.CurrentMission.BoundsSize * 1.1f)
        {
            if (DialogCanvasManager.Instance.midInfo.gameObject.activeSelf == false)
                DialogCanvasManager.Instance.midInfo.Show("You're too far away!");
        }
    }

    private void HandleInput()
    {
        if (isLocked) return;

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
            if (Stats.Fuel > 0)
            {
                StartMovement();
            }
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            StopMovement();
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
            GameController.Instance.MissionController.OnMissionCompleted?.Invoke();
        }
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
        var colName = collision.GetContact(0).thisCollider.name;
        if (colName == "PlayerLander" && collision.gameObject.tag == "Landable" && gameObject.activeSelf)
        {
            var currentPlanetHost = collision.gameObject.GetComponentInParent<PlanetController>();
            float angle;
            GetLandingData(ref landingData, collision.gameObject.transform.forward, currentPlanetHost, out angle);
            if (angle > GameController.Instance.Settings.MaxLandingAngle)
            {
                Kill();
                return;
            }

            Land(currentPlanetHost);
        }
        else if (!hasLanded && !isDead)
        {
            Kill();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Landable")
        {
            PreviousPlanetHost = collision.gameObject.GetComponentInParent<PlanetController>();
            TakeOff(PreviousPlanetHost);
        }
    }

    private void Land(PlanetController planet)
    {
        StartCoroutine(LockInput());
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
        Vibration.Vibrate(20);
        OnPlayerLanded?.Invoke(planet);
    }

    private IEnumerator LockInput()
    {
        isLocked = true;
        yield return new WaitForSeconds(.5f);
        isLocked = false;
    }

    private void GetLandingData(ref LandingRewardArgs landingData, Vector3 landingObjUpVector, PlanetController planet, out float angle)
    {
        angle = Mathf.Abs(90 - Vector3.Angle(landingObjUpVector, transform.right));
        Debug.Log(angle);
        landingData = new LandingRewardArgs(angle, rigidbody.velocity.magnitude);
    }

    private void LoadFuel()
    {
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
            playerEffects.ShowFuelLoading(shipThruster.transform.position, transform.rotation);
            Stats.AddFuel(GameController.Instance.Settings.FuelLoading * Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator SetScore()
    {
        yield return new WaitForSeconds(2);
        string missionName = GameController.Instance.MissionController.CurrentMission.Name;
        string scoreName = "score" + missionName;
        Debug.Log(Stats.Score);

        if (PlayerPrefs.HasKey(scoreName))
        {
            if (PlayerPrefs.GetInt(scoreName) < Stats.Score)
                PlayerPrefs.SetInt(scoreName, Stats.Score);
        }
        else
            PlayerPrefs.SetInt(scoreName, Stats.Score);

        if (PlayFabClientAPI.IsClientLoggedIn() && PF_PlayerData.Statistics[missionName] < Stats.Score)
        {
            PF_PlayerData.UpdateUserScore(missionName, Stats.Score);
            PF_PlayerData.Statistics[missionName] = Stats.Score;
            if (Stats.Score > PF_PlayerData.TopScores[missionName])
            {
                PF_PlayerData.TopScores[missionName] = Stats.Score;
                OnNewGlobalHighScore?.Invoke();
            }
            else
            {
                OnNewPersonalHighScore?.Invoke();
            }
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
        RemovePassenger(entity);
        Audio2.PlayOneShot(Sounds.DestinationReached);
        entity.ExitShip(planet);
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
        if (Stats.Fuel <= 0 && isMoving)
        {
            OnFuelExhausted?.Invoke();
            StopMovement();
            playerEffects.PlayLostFuelSequence();
            return;
        }

        if (rigidbody.velocity.magnitude < maxVelocityMagnitude)
        {
            rigidbody.AddForce(transform.forward * GameController.Instance.Settings.PlayerAccel * Time.deltaTime, ForceMode.Acceleration);
            Stats.AddFuel(GameController.Instance.Settings.FuelCost * Time.fixedDeltaTime);
        }
    }

    private void StartEngine()
    {
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

    private void CorrectVelocity()
    {
        if (PreviousPlanetHost != null)
        {
            rigidbody.velocity += PreviousPlanetHost.CurrentVelocity;
            Debug.Log(PreviousPlanetHost.CurrentVelocity);
        }
    }

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

    private IEnumerator WaitForTutorialCompletion()
    {
        rigidbody.isKinematic = true;
        transform.localScale = Vector3.zero;
        yield return new WaitUntil(() => GameController.Instance.MissionController.CurrentMission.tutorial.Complete);
        rigidbody.isKinematic = false;
        playerEffects.PlayIntroSequence();
    }

    private IEnumerator MissionEndedCR()
    {
        yield return new WaitForSeconds(1.25f);
        var endgameLandingScore = GameController.Instance.Rewards.GetReward(Reward.RewardType.FuelReward,
                new FuelRewardArgs(Stats.MaxFuel, Stats.Fuel, Stats.TotalFuelUsed));
        Stats.AddScore(endgameLandingScore);
        StopLoadingFuel();
        playerEffects.PlayOutroSequence();
        StartCoroutine(SetScore());
    }
}