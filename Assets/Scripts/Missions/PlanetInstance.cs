using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class PlanetInstance
{
    [Space(10), Header("Basic")]
    public GameObject Prefab;
    public Vector3 Position;
    public float Mass;
    public float Scale;
    public int ID;
    [SerializeField] private Vector3 rotationAngles;
    public Quaternion Rotation => Quaternion.Euler(rotationAngles);
    public float RotationSpeed;

    [Space(10), Header("Orbit")]
    [HideInInspector] public Transform CentralObject;
    public Vector3 Center;
    public float Period;
    public float Radius => Vector3.Distance(Position, Center);
    public float AngularFrequency => 2 * Mathf.PI * (1 / Period);

    public List<PlanetInstance> Satellites = new List<PlanetInstance>();

    //[Space(10), Header("Npcs")]
    //[SerializeField] private List<TravellerInstance> TravellerInstances = new List<TravellerInstance>();
    //private Queue<TravellerInstance> TravellersQueue;
    //private bool Initialized = false;

    //private void Initialize()
    //{
    //    TravellersQueue = new Queue<TravellerInstance>();

    //    // Copy List that is visible in editor to queue
    //    foreach (TravellerInstance traveler in TravellerInstances)
    //    {
    //        TravellersQueue.Enqueue(traveler);
    //    }
    //    this.Initialized = true;
    //}

    //public TravellerInstance GetNextTraveller()
    //{
    //    if (!Initialized)
    //        Initialize();

    //    if (TravellersQueue.Count > 0)
    //    {
    //        return TravellersQueue.Dequeue();
    //    }
    //    else
    //    {
    //        return null;
    //    }
    //}
}