using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class PlanetInstance
{
    public GameObject Prefab;
    public Vector3 Position;
    [SerializeField]
    private Vector3 rotationAngles;
    public Quaternion Rotation => Quaternion.Euler(rotationAngles);
    public int ID;

    [SerializeField]
    private List<TravellerInstance> TravellerInstances = new List<TravellerInstance>();
    private Queue<TravellerInstance> TravellersQueue;
    private bool Initialized = false;

    private void Initialize()
    {
        TravellersQueue = new Queue<TravellerInstance>();

        // Copy List that is visible in editor to queue
        foreach (TravellerInstance traveler in TravellerInstances)
        {
            TravellersQueue.Enqueue(traveler);
        }
        this.Initialized = true;
    }

    public TravellerInstance GetNextTraveller()
    {
        //if (!Initialized)
            Initialize();

        if (TravellersQueue.Count > 0)
        {
            return TravellersQueue.Dequeue();
        }
        else
        {
            return null;
        }
    }
}