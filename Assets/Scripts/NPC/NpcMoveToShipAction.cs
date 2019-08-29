using UnityEngine;

namespace NpcActions
{
    public class NpcMoveToShipAction : Action
    {
        public override ActionType Type => ActionType.MoveToShip;

        public override void Process(NPCEntity npc)
        {
            base.Process(npc);
            //npc.MoveTo(npc.HostPlanet.LandingPlatform);
            npc.MoveTo(PlayerController.Instance.transform);
            npc.View.SpeechBubble.ShowInfo(npc.DestinationPlanet.transform.name);
        }

        public override void ProcessTriggerCollision(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                npc.EnterShip();
            }
        }
    }
}