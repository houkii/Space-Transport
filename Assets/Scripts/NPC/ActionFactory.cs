using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System;

namespace NpcActions
{
    public static class ActionFactory
    {
        private static Dictionary<ActionType, Type> ActionsDictionary;

        static ActionFactory()
        {
            var ActionTypes = Assembly.GetAssembly(typeof(NpcActions.Action)).GetTypes().
                Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(NpcActions.Action)));

            // Dictinary for finding action types
            ActionsDictionary = new Dictionary<ActionType, Type>();

            // Get ActionTypes and put them into the dictionary
            foreach (var type in ActionTypes)
            {
                var tempAction = Activator.CreateInstance(type) as Action;
                ActionsDictionary.Add(tempAction.Type, type);
            }
        }

        public static Action GetAction(NpcActions.ActionType actionType)
        {
            if (ActionsDictionary.ContainsKey(actionType))
            {
                Type type = ActionsDictionary[actionType];
                var currentAction = Activator.CreateInstance(type) as NpcActions.Action;
                return currentAction;
            }
            else
            {
                return null;
            }
        }

        internal static IEnumerable<NpcActions.ActionType> GetNpcActionTypes()
        {
            return ActionsDictionary.Keys;
        }
    }
}