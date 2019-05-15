using System.Collections;
using System.Collections.Generic;
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
                    GameObject.Destroy(npc.View.gameObject);
                    GameObject.Destroy(npc.gameObject);
                }
            }
        }
    }
}