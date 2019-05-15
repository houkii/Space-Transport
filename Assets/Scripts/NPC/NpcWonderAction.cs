using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NpcActions
{
    public class NpcWonderAction : Action
    {
        private int CurrentWaypointID;
        public override ActionType Type => ActionType.Wonder;

        public override void Process(NPCEntity npc)
        {
            base.Process(npc);
            CurrentWaypointID = Random.Range(0, npc.HostPlanet.Waypoints.Count);
            npc.MoveTo(npc.HostPlanet.Waypoints[CurrentWaypointID]);
            Debug.Log("Wondering...");
        }

        public override void ProcessTriggerCollision(Collider other)
        {
            if (other.gameObject.tag == "Waypoint")
            {
                if (other.gameObject.transform == npc.HostPlanet.Waypoints[CurrentWaypointID])
                {
                    CurrentWaypointID++;
                    if (CurrentWaypointID >= npc.HostPlanet.Waypoints.Count)
                        CurrentWaypointID = 0;

                    npc.MoveTo(npc.HostPlanet.Waypoints[CurrentWaypointID]);
                }
            }
        }
    }
}
