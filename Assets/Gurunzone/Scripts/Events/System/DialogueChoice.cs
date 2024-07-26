using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gurunzone.Events
{

    public class DialogueChoice : MonoBehaviour
    {
        [TextArea(1, 1)]
        public string text;
        public EventTrigger go_to;


        [HideInInspector]
        public int choice_index;

        public UnityAction<int> onSelect;

        private EventCondition[] conditions;

        private void Awake()
        {
            conditions = GetComponents<EventCondition>();
        }

        public string GetText()
        {
            return EventTool.Translate(text);
        }

        public bool AreConditionsMet()
        {
            bool met = true;
            foreach (EventCondition condition in conditions)
            {
                met = met && condition.IsMet();
            }
            return met;
        }
    }

}
