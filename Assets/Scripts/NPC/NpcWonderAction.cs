using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NpcActions
{
    public class NpcWonderAction : Action
    {
        private int currentWaypointID;
        public override ActionType Type => ActionType.Wonder;

        public override void Process(NPCEntity npc)
        {
            base.Process(npc);
            currentWaypointID = Random.Range(0, npc.HostPlanet.Waypoints.Count);
            npc.MoveTo(npc.HostPlanet.Waypoints[currentWaypointID]);
            Debug.Log("Wondering...");
        }

        public override void ProcessTriggerCollision(Collider other)
        {
            if (other.gameObject.tag == "Waypoint")
            {
                if (other.gameObject.transform == npc.HostPlanet.Waypoints[currentWaypointID])
                {
                    currentWaypointID++;
                    if (currentWaypointID >= npc.HostPlanet.Waypoints.Count)
                        currentWaypointID = 0;

                    npc.MoveTo(npc.HostPlanet.Waypoints[currentWaypointID]);
                }
            }
        }
    }
}
