using UnityEngine;

namespace NpcActions
{
    public enum ActionType { None = 0, Wonder = 1, MoveToShip = 2, MoveAway = 3 };

    public abstract class Action
    {
        protected NPCEntity npc;
        public virtual bool IsLocked => false;

        public virtual void Process(NPCEntity npc)
        {
            this.npc = npc;
        }

        public abstract ActionType Type { get; }

        public abstract void ProcessTriggerCollision(Collider other);
    }
}