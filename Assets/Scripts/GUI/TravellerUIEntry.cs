using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class TravellerUIEntry : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI astronautName;
    [SerializeField]
    private TextMeshProUGUI destinationName;

    public NPCEntity Npc { get; private set; }

    public void Initialize(NPCEntity entity)
    {
        Npc = entity;
        astronautName.text = Npc.name;
        destinationName.text = Npc.DestinationPlanet.name;
    }
}
