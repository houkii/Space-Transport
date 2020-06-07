using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlanetInstance
{
    [Space(10), Header("Basic")]

    public GameObject Prefab;
    public Vector3 Position;
    public float Mass;
    public float Scale;
    public string ID;
    public Quaternion Rotation => Quaternion.Euler(rotationAngles);
    public float RotationSpeed;
    [SerializeField] private Vector3 rotationAngles;

    [Space(10), Header("Orbit")]

    [HideInInspector] public Transform CentralObject;
    public Vector3 Center;
    public float Period;
    public float Radius => Vector3.Distance(Position, Center);
    public float AngularFrequency => 2 * Mathf.PI * (1 / Period);

    public List<PlanetInstance> Satellites = new List<PlanetInstance>();
}