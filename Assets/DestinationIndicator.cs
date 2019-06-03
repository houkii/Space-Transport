using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationIndicator : MonoBehaviour
{
    [SerializeField] private float offsetRadius = 30f;
    public Transform Destination;

    private void Awake()
    {
        GameController.Instance.MissionController.OnEntitySpawned += HandleRoute;
    }

    private void FixedUpdate()
    {
        if(Destination != null)
        {
            var destinationVector = Destination.position;
            transform.rotation = Quaternion.LookRotation(destinationVector - PlayerController.Instance.transform.position, PlayerController.Instance.transform.up);
            transform.position = PlayerController.Instance.transform.position + transform.forward * offsetRadius;
        }
    }

    private void HandleRoute(NPCEntity entity)
    {
        gameObject.SetActive(true);
        Destination = entity.HostPlanet.transform;
        entity.OnGotAboard.AddListener(() => Destination = entity.DestinationPlanet.transform);
        entity.OnExitShip += () => gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        GameController.Instance.MissionController.OnEntitySpawned -= HandleRoute;
    }
}
