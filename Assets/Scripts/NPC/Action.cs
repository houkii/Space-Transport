using UnityEngine;

namespace NpcActions
{
    public enum ActionType { None = 0, Wonder = 1, MoveToShip = 2, MoveAway = 3 };

    public abstract class Action
    {
        public virtual bool IsLocked => false;
        protected NPCEntity npc;

        public virtual void Initialize(NPCEntity npc)
        {
            this.npc = npc;
        }

        public abstract ActionType Type { get; }
        public abstract void ProcessTriggerCollision(Collider other);
    }
}