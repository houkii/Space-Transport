using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlaySceneCanvasController : Singleton<PlaySceneCanvasController>
{
    [SerializeField]
    private GameObject npcEntityCanvasPrefab;
    [SerializeField]
    private GameObject npcCanvasHolder;
    [SerializeField]
    private SummaryPanelController summaryWindow;
    private List<MovableCanvasElement> movableElements = new List<MovableCanvasElement>();

    public TravellersPanelController TravellersPanelController { get; private set; }

    public override void Awake()
    {
        base.Awake();
        TravellersPanelController = GetComponentInChildren<TravellersPanelController>();
        movableElements = GetComponentsInChildren<MovableCanvasElement>().ToList();
        PlayerController.Instance.OnPlayerDied.AddListener(ShowEndGameUI);
        GameController.Instance.MissionController.OnMissionCompleted.AddListener(ShowEndGameUI);
    }

    public NpcEntityCanvas AddNpcCanvas(NPCEntity Target)
    {
        var npcCanvasObj = Instantiate(npcEntityCanvasPrefab, npcCanvasHolder.transform, false);
        var npcCanvasCtrl = npcCanvasObj.GetComponent<NpcEntityCanvas>();
        npcCanvasCtrl.Initialize(Target);
        return npcCanvasCtrl;
    }

    private void HideAllMovableElements()
    {
        foreach(MovableCanvasElement elem in movableElements)
        {
            if(elem.gameObject.activeSelf)
            {
                elem.Hide();
            }
        }
    }

    private void ShowSummary()
    {
        bool missionCompleted = GameController.Instance.MissionController.MissionCompleted;
        summaryWindow.Show(missionCompleted);
    }

    private void ShowEndGameUI()
    {
        StartCoroutine(ShowEndGameUICR());
    }

    private IEnumerator ShowEndGameUICR()
    {
        HideAllMovableElements();
        yield return new WaitForSeconds(2f);
        ShowSummary();
    }
}
