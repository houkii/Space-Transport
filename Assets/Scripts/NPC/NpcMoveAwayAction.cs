using DG.Tweening;
using UnityEngine;

namespace NpcActions
{
    public class NpcMoveAwayAction : Action
    {
        public override ActionType Type => ActionType.MoveAway;
        public override bool IsLocked => true;

        public override void Process(NPCEntity npc)
        {
            base.Process(npc);
            Debug.Log("Moving Away...");
            //npc.HostPlanet = npc.DestinationPlanet;
            //npc.ExitShip();
            npc.MoveTo(npc.HostPlanet.PlanetBack);
            npc.OnReachedDestination?.Invoke();
        }

        public override void ProcessTriggerCollision(Collider other)
        {
            if (other.gameObject.tag == "Waypoint")
            {
                if (other.gameObject.name == "PlanetBack")
                {
                    Debug.Log("A traveller reached destination :)");
                    if (npc.View != null)
                        GameObject.Destroy(npc.View.gameObject);
                    DestroySequence();
                }
                else
                {
                    npc.MoveTo(npc.HostPlanet.PlanetBack);
                }
            }
            if (other.gameObject.tag == "Player")
            {
                int index = Random.Range(0, npc.HostPlanet.Waypoints.Count);
                npc.MoveTo(npc.HostPlanet.Waypoints[index]);
            }
        }

        private void DestroySequence()
        {
            npc.transform.DOScale(0, .75f).SetEase(Ease.InBack)
                .OnComplete(() => GameObject.Destroy(npc.gameObject));
        }
    }
}