using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone.Events
{

    public class EventLine
    {
        public GameObject game_obj;
        public EventTrigger parent;
        public DialogueMessage dialogue = null;
        public List<DialogueChoice> choices = new List<DialogueChoice>();
        public List<EventCondition> conditions = new List<EventCondition>();
        public List<EventEffect> effects = new List<EventEffect>();

        public bool AreConditionsMet()
        {
            bool conditions_met = true;
            foreach (EventCondition condition in conditions)
            {
                if (condition.enabled && !condition.IsMet())
                {
                    conditions_met = false;
                }
            }
            return conditions_met && game_obj.activeSelf;
        }

        public DialogueChoice GetChoice(int index)
        {
            if (index >= 0 && index < choices.Count)
                return choices[index];
            return null;
        }

        public void TriggerLine()
        {
            EventManager.Get().StartEventLine(this);
        }

        public void TriggerLineIfMet()
        {
            if (AreConditionsMet())
            {
                EventManager.Get().StartEventLine(this);
            }
        }

        public float TriggerEffects()
        {
            float wait_timer = 0f;
            foreach (EventEffect effect in effects)
            {
                if (effect.enabled)
                {
                    effect.Trigger();
                    wait_timer += effect.GetWaitTime();
                }
            }
            return wait_timer;
        }
    }

}
