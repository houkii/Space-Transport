using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TravellersPanelController : MonoBehaviour
{
    [SerializeField] private GameObject travellerUIEntryPrefab;
    [SerializeField] private float topOffset = 27.5f;
    [SerializeField] private float verticalSpacing = 15f;
    private List<TravellerUIEntry> travellerEntries = new List<TravellerUIEntry>();

    public void AddEntry(NPCEntity npc)
    {
        var entry = Instantiate(travellerUIEntryPrefab, this.transform);
        var entryCtrl = entry.GetComponent<TravellerUIEntry>();
        entryCtrl.Initialize(npc);
        var entryRT = entry.GetComponent<RectTransform>();
        this.SetPosition(ref entryRT);
        travellerEntries.Add(entryCtrl);
    }

    public void RemoveEntryOfNpc(NPCEntity entity)
    {
        var entryToRemove = travellerEntries.Find(x => x.Npc == entity);
        this.RemoveEntry(entryToRemove);
    }

    public void RemoveEntry(TravellerUIEntry entry)
    {
        var rt = entry.GetComponent<RectTransform>();
        travellerEntries.Remove(entry);
        rt.DOAnchorPos(new Vector2(-rt.rect.width, rt.anchoredPosition.y), 0.5f).SetEase(Ease.InBounce)
            .OnComplete(() =>
            {
                Destroy(entry.gameObject);
                RepositionEntries();
            });
    }

    private void SetPosition(ref RectTransform rt)
    {
        var pos = GetPosition(ref rt, travellerEntries.Count);
        rt.anchoredPosition = new Vector2(-rt.rect.width, pos.y);
        rt.DOAnchorPos(pos, .5f).SetEase(Ease.OutBounce).OnComplete(() => RepositionEntries());
    }

    private Vector2 GetPosition(ref RectTransform rt, int index)
    {
        var yPos = topOffset + ((rt.rect.height + verticalSpacing) * index);
        return new Vector2(0, -yPos);
    }

    private void RepositionEntries()
    {
        for(int i=0;i<travellerEntries.Count;i++)
        {
            var rt = travellerEntries[i].GetComponent<RectTransform>();
            var pos = GetPosition(ref rt, i);
            if (rt.anchoredPosition != pos)
            {
                rt.DOAnchorPos(pos, .5f + (i * 0.4f)).SetEase(Ease.InBack);
            }
        }
    }
}
