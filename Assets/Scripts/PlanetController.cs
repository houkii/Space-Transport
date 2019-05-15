using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetController : MonoBehaviour
{
    [SerializeField]
    private GameObject TravellerPrefab;
    public GameObject CurrentTraveller;

    public GameObject AsteroidPrefab;

    public Transform LandingPlatform;
    public Transform PlanetBack;
    public Transform SpawnPosition;
    public Transform ReleaseSpot;

    public List<Transform> Waypoints;

    public Vector3 spawnPosition;

    public float rotationSpeed;

    private PlanetInstance Data;
    private bool PlanetFree => CurrentTraveller == null;

    public TMPro.TextMeshPro miniMapIndex;

    void Start()
    {
        rotationSpeed = Random.Range(-5, 5);
        miniMapIndex.text = Data.ID.ToString();
        spawnPosition = SpawnPosition.position;
    }

    private void Spawn()
    {
        StartCoroutine(SpawnTravelers());
    }

    private IEnumerator SpawnTravelers()
    {
        var travellerToSpawn = Data.GetNextTraveller();
        if(travellerToSpawn != null)
        {
            yield return new WaitForSeconds(travellerToSpawn.SpawnDelay);
            this.CurrentTraveller = InstantiateTraveller(travellerToSpawn);
            yield return new WaitUntil(() => this.PlanetFree);
            yield return SpawnTravelers();
        }
        else
        {
            yield break;
        }
        yield break;
    }

    private GameObject InstantiateTraveller(TravellerInstance traveller)
    {
        var spawnedTraveller = Instantiate(traveller.TravelerPrefab, spawnPosition, Quaternion.identity);

        // Bind planet ID to Planet Controller
        spawnedTraveller.GetComponent<NPCEntity>().DestinationPlanet =
            GameController.Instance.MissionController.MissionPlanets[traveller.DestinationPlanet];

        return spawnedTraveller;
    }

    public PlanetController Initialize(PlanetInstance data)
    {
        this.Data = data;
        this.Spawn();
        return this;
    }

    private void FixedUpdate()
    {
        transform.Rotate(transform.forward, rotationSpeed * Time.fixedDeltaTime);
    }

    //private void LateUpdate()
    //{
    //    miniMapIndex.transform.rotation = Quaternion.identity;
    //}
}
