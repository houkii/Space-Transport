using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Attractor : MonoBehaviour
{
    //const float G = 667.4f;
    public Rigidbody rb;

    public bool isAffectedByPull = true;
    public bool isPulling = true;
    public List<Attractor> attractors = new List<Attractor>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Attract(Attractor objectToAttract)
    {
        Rigidbody rbToAttract = objectToAttract.rb;
        Vector3 direction = transform.position - rbToAttract.transform.position;
        float distance = direction.magnitude;
        float forceMagnitude = GameController.Instance.Settings.G * (rb.mass * rbToAttract.mass) / Mathf.Pow(GameController.Instance.Settings.DistanceScaler * distance, 2);
        Vector3 force = direction.normalized * forceMagnitude;
        rbToAttract.AddForce(force);
    }

    private void FixedUpdate()
    {
        foreach (Attractor attractor in attractors)
        {
            if (attractor != this)
            {
                Attract(attractor);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isPulling) return;

        var attractor = other.gameObject.GetComponent<Attractor>();
        if (attractor != null && attractor.isAffectedByPull && !attractors.Contains(attractor))
        {
            attractors.Add(attractor);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!isPulling) return;

        var attractor = other.gameObject.GetComponent<Attractor>();
        //DetachAttractor(attractor);
    }

    private void DetachAttractor(Attractor attractor)
    {
        if (attractor != null && attractors.Contains(attractor))
            attractors.Remove(attractor);
    }

    private void OnDestroy()
    {
        var allAttractors = FindObjectsOfType<Attractor>();
        foreach (Attractor attractor in allAttractors)
        {
            attractor.DetachAttractor(this);
        }
    }
}