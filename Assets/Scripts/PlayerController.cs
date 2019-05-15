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
    private bool isLocked = false;
    private bool isMoving = false;
    private bool hasLanded = false;

    public UnityEvent OnPlayerDied;
    public UnityAction<PlanetController> OnPlayerLanded;
    public UnityAction<PlanetController> OnPlayerTookOff;

    private List<NPCEntity> Passengers = new List<NPCEntity>();

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

        if(this.isMoving)
        {
            this.Move();
        }

        if(transform.parent != null && Vector3.Distance(transform.position, transform.parent.transform.position) > 80f)
            transform.SetParent(null);
    }

    private void HandleJoystickInput()
    {
        if (this.transform.parent != null || (this.isLocked || this.hasLanded)) return;

        if (Joystick.Instance.Input.magnitude < .2f) return;
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
                this.Land(currentPlanetHost);
            }
            else
            {
                Debug.Log("Player Died");
                OnPlayerDied?.Invoke();
            }
        }
        else
        {
            Debug.Log("Player Died");
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
        transform.SetParent(HostPlanet.transform);
        Debug.Log("Player Landed on " + planet.name);

        if (Passengers.Count > 0)
            this.ReleasePassengers(HostPlanet);

        OnPlayerLanded?.Invoke(planet);
    }

    private void TakeOff(PlanetController planet)
    {
        this.hasLanded = false;
        this.HostPlanet = null;
        Debug.Log("Player Took off " + planet.name);
        OnPlayerTookOff?.Invoke(planet);
    }

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
        this.Passengers.Add(entity);
    }

    public void RemovePassenger(NPCEntity entity)
    {
        this.Passengers.Remove(entity);
    }

    private void ReleasePassengers(PlanetController planet)
    {
        var leavers = Passengers.FindAll(x => x.DestinationPlanet == planet);
        if(leavers.Count > 0)
        {
            StartCoroutine(ReleasePassengersCR(leavers, planet));
        }
    }

    private IEnumerator ReleasePassengersCR(List<NPCEntity> leavers, PlanetController planet)
    {
        this.isLocked = true;
        yield return new WaitForSeconds(0.5f);
        foreach (NPCEntity leaver in leavers)
        {
            leaver.ExitShip(planet);
            this.RemovePassenger(leaver);
            yield return new WaitForSeconds(1.5f);
        }
        this.isLocked = false;
    }

    private void Move()
    {
        if (rigidbody.velocity.magnitude < maxVelocityMagnitude)
        {
            rigidbody.AddForce(transform.forward * acceleration * Time.deltaTime, ForceMode.Acceleration);
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