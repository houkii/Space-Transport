using UnityEngine;

namespace NpcActions
{
    public class NpcMoveToShipAction : Action
    {
        public override ActionType Type => ActionType.MoveToShip;

        public override void Initialize(NPCEntity npc)
        {
            base.Initialize(npc);
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

    public class NpcNoneAction : Action
    {
        public override ActionType Type => ActionType.None;

        public override void ProcessTriggerCollision(Collider other)
        {
        }
    }
}