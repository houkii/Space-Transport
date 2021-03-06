﻿using UnityEngine;

public class DestinationIndicator : MonoBehaviour
{
    public Transform Destination;

    [SerializeField] private float offsetRadius = 30f;
    private float platformPosOffset = 40f;

    private void Awake()
    {
        GameController.Instance.MissionController.OnEntitySpawned += HandleRoute;
    }

    private void Start()
    {
        PlayerController.Instance.OnPlayerLanded += (x) => { gameObject.SetActive(false); };
        PlayerController.Instance.OnPlayerTookOff += (x) => { gameObject.SetActive(true); };
    }

    private void FixedUpdate()
    {
        if (Destination != null)
        {
            var destinationVector = Destination.position;

            float distance = Vector3.Distance(PlayerController.Instance.transform.position, Destination.position);
            if (distance <= (200f + 100 * CameraController.Instance.currentToMaxCameraSizeRation))
            {
                var landingPlatform = Destination.GetComponent<PlanetController>().GetNearestPlantformTransform(PlayerController.Instance.transform.position);
                Vector3 platformPos = landingPlatform.transform.position + landingPlatform.forward * (platformPosOffset * landingPlatform.parent.localScale.x / 100);
                transform.position = Vector3.Lerp(transform.position, platformPos, 0.2f);
                Quaternion platformRot = Quaternion.LookRotation(destinationVector - transform.position, PlayerController.Instance.transform.up);
                transform.rotation = Quaternion.Lerp(transform.rotation, platformRot, 0.125f);

                if (transform.parent != null)
                {
                    transform.parent = null;
                }
            }
            else
            {
                transform.rotation = Quaternion.LookRotation(destinationVector - PlayerController.Instance.transform.position, PlayerController.Instance.transform.up);
                Vector3 destinationPos = PlayerController.Instance.transform.position + transform.forward * offsetRadius;
                transform.position = Vector3.Lerp(transform.position, destinationPos, 0.5f);

                if (transform.parent != PlayerController.Instance.transform)
                {
                    transform.parent = PlayerController.Instance.transform;
                }
            }
        }
    }

    private void HandleRoute(NPCEntity entity)
    {
        Destination = entity.HostPlanet.transform;
        entity.OnGotAboard.AddListener(() => Destination = entity.DestinationPlanet.transform);
    }
}