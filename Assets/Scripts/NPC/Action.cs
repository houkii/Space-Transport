using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NpcActions
{
    public enum ActionType { Wonder = 0, MoveToShip = 1, MoveAway = 2 };

    public abstract class Action
    {
        protected NPCEntity npc;
        public virtual bool IsLocked => false;
        public virtual void Process(NPCEntity npc) { this.npc = npc; }
        public abstract ActionType Type { get; }
        public abstract void ProcessTriggerCollision(Collider other);
    }
}