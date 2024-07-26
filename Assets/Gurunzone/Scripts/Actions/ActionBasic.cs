using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Basic action class, inherit this class to create actions
    /// </summary>
    
    public class ActionBasic : ScriptableObject
    {
        public string id;  //Id for the save file

        private static List<ActionBasic> list = new List<ActionBasic>();

        public virtual void StartAction(Character character, Interactable target)
        {
            //Inherit this function to run code when the character start the action
        }

        public virtual void StopAction(Character character, Interactable target)
        {
            //Inherit this function to run code when the character stop the action
        }

        public virtual void UpdateAction(Character character, Interactable target)
        {
            //Inherit this function to run code every frame while doing this action
        }

        public virtual void TriggerAction(Character character, Interactable target, string trigger)
        {
            //Custom action trigger
        }

        public virtual bool CanDoAction(Character character, Interactable target)
        {
            return true; //Inherit this function put a condition on if the action can be performed or not
        }

        public static void Load(string folder = "")
        {
            list.Clear();
            list.AddRange(Resources.LoadAll<ActionBasic>(folder));
        }

        public static ActionBasic Get(string id)
        {
            foreach (ActionBasic action in list)
            {
                if (action.id == id)
                    return action;
            }
            return null;
        }

        public static T Get<T>() where T : ActionBasic
        {
            foreach (ActionBasic action in list)
            {
                if (action is T)
                    return (T) action;
            }
            return null;
        }

        public static List<ActionBasic> GetAll()
        {
            return list;
        }

    }
}
