using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NpcActions
{
    public class NpcMoveToShipAction : Action
    {
        public override ActionType Type => ActionType.MoveToShip;

        public override void Process(NPCEntity npc)
        {
            base.Process(npc);
            npc.MoveTo(npc.HostPlanet.LandingPlatform);
            npc.View.SpeechBubble.ShowInfo(npc.DestinationPlanet.transform.name);
        }

        public override void ProcessTriggerCollision(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                PlaySceneCanvasController.Instance.TravellersPanelController.AddEntry(npc);
                npc.EnterShip();
            }
        }
    }
}
