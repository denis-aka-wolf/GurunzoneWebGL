using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone.Events
{

    public class EventGroup : MonoBehaviour
    {
        private List<EventTrigger> event_list = new List<EventTrigger>();

        void Awake()
        {
            foreach (EventTrigger evt in GetComponentsInChildren<EventTrigger>())
            {
                event_list.Add(evt);
                evt.AddConditions(GetComponents<EventCondition>());
            }
        }

        public EventTrigger GetRandom()
        {
            List<EventTrigger> valid_list = new List<EventTrigger>();
            foreach (EventTrigger trigger in event_list)
            {
                if (trigger.AreConditionsMet())
                    valid_list.Add(trigger);
            }

            if (valid_list.Count > 0)
                return valid_list[Random.Range(0, valid_list.Count)];
            return null;
        }
    }

}
