using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TravellerUIEntry : MonoBehaviour
{
    public NPCEntity Npc { get; private set; }

    [SerializeField] private TextMeshProUGUI astronautName;
    [SerializeField] private TextMeshProUGUI destinationName;
    private Slider timeSlider;

    private void Awake()
    {
        timeSlider = GetComponentInChildren<Slider>();
    }

    public void Initialize(NPCEntity entity)
    {
        Npc = entity;
        astronautName.text = Npc.name;
        destinationName.text = Npc.DestinationPlanet.name;
    }

    private void Update()
    {
        if (Npc.DeliveryRewardData != null)
        {
            Npc.DeliveryRewardData.Process();
            timeSlider.value = Npc.DeliveryRewardData.CurrentToMaxTimeRatio;
        }
    }
}