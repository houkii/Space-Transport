using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySceneCanvasController : Singleton<PlaySceneCanvasController>
{
    [SerializeField]
    private GameObject NpcEntityCanvasPrefab;
    [SerializeField]
    private GameObject NpcCanvasHolder;

    public TravellersPanelController TravellersPanelController { get; private set; }

    public override void Awake()
    {
        base.Awake();
        TravellersPanelController = GetComponentInChildren<TravellersPanelController>();
    }

    public NpcEntityCanvas AddNpcCanvas(NPCEntity Target)
    {
        var npcCanvasObj = Instantiate(NpcEntityCanvasPrefab, NpcCanvasHolder.transform, false);
        var npcCanvasCtrl = npcCanvasObj.GetComponent<NpcEntityCanvas>();
        npcCanvasCtrl.Initialize(Target);
        return npcCanvasCtrl;
    }
}
