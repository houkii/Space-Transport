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

    public void Initialize(string name, string destination)
    {
        astronautName.text = name;
        destinationName.text = destination;
    }
}
