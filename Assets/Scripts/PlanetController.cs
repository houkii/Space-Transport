using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlanetController : MonoBehaviour
{
    [SerializeField]
    private GameObject TravellerPrefab;
    [SerializeField]
    private GameObject AsteroidPrefab;
    [SerializeField]
    private TextMeshPro miniMapIndex;
    private PlanetInstance Data;

    public GameObject CurrentTraveller;
    public Transform LandingPlatform;
    public Transform PlanetBack;
    public Transform SpawnPosition;
    public Transform ReleaseSpot;
    public List<Transform> Waypoints;

    private float angle = 0;
    private float speed = (2 * Mathf.PI) / 25;
    private float radius;


    void Start()
    {
        miniMapIndex.text = Data.ID.ToString();
        radius = Vector3.Distance(transform.position, Vector3.zero);
        angle = Vector3.SignedAngle(Vector3.right, transform.position.normalized, Vector3.right);
        //speed = (2 * Mathf.PI) / Random.Range(75, 150);
        speed = (2 * Mathf.PI) / (radius/5);
        //speed = (2 * Mathf.PI) / 3;
        Debug.Log(gameObject.name + "    angle: " + angle);
    }

    public PlanetController Initialize(PlanetInstance data)
    {
        this.Data = data;
        return this;
    }

    private void FixedUpdate()
    {
        transform.Rotate(transform.forward, Data.RotationSpeed * Time.fixedDeltaTime);
        this.OrbitalMove();
    }

    private void OrbitalMove()
    {
        float x = Mathf.Cos(angle) * radius;
        float y = Mathf.Sin(angle) * radius;
        transform.position = new Vector3(x, y, transform.position.z);
        angle += speed * Time.fixedDeltaTime;
    }

    //private void LateUpdate()
    //{
    //    miniMapIndex.transform.rotation = Quaternion.identity;
    //}
}
