using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Script for narrative event (triggering quests or other)
/// Author: Indie Marc (Marc-Antoine Desbiens)
/// </summary>

namespace Gurunzone.Events
{

    public enum EventType 
    {
        None = 0, //Triggered by external condition
        AutoTrigger = 1, //As soon as conditions are met
        AtStart = 5, //After scene load
        Interval = 7, //Every X game hours
        EnterRegion = 10, //Enter a region
        LeaveRegion = 11, //Exit a resion
        AfterEvent = 20, //After another event
    }
    
    public class EventTrigger : MonoBehaviour
    {
        [Tooltip("Optional: Only use if you plan to save the NarrativeData, so it can reference to it.")]
        public string id;

        [Header("Trigger")]
        [Tooltip("How this event will be triggered")]
        public EventType trigger_type;
        [Tooltip("Which object will trigger this event")]
        public GameObject trigger_target;
        [Tooltip("Interval in game-hours for the Interval Trigger")]
        public float trigger_value;
        [Tooltip("Number of times it can trigger. Put 0 for Infinity")]
        public int trigger_limit = 1; //0 means infinite

        [Tooltip("Should it pause gameplay?")]
        public bool pause_gameplay = false;

        [Header("Comment, no effect")]
        [TextArea(3, 5)]
        public string comment;

        public UnityAction onStart;
        public UnityAction onEnd;

        private GameObject triggerer = null;
        private int trigger_count = 0;
        private int priority = 0;

        private static List<EventTrigger> event_list = new List<EventTrigger>();

        private List<EventCondition> conditions = new List<EventCondition>();
        private List<EventEffect> effects = new List<EventEffect>();
        private List<EventLine> event_lines = new List<EventLine>();

        void Awake()
        {
            event_list.Add(this);
            conditions.AddRange(GetComponents<EventCondition>());
            effects.AddRange(GetComponents<EventEffect>());
            priority = transform.GetSiblingIndex(); //First one has priority over other ones

            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject child_obj = transform.GetChild(i).gameObject;
                if (child_obj.GetComponent<EventEffect>() || child_obj.GetComponent<DialogueMessage>())
                {
                    EventLine child = new EventLine();
                    child.game_obj = child_obj;
                    child.parent = this;
                    child.dialogue = child_obj.GetComponent<DialogueMessage>();
                    child.conditions.AddRange(child_obj.GetComponents<EventCondition>());
                    child.effects.AddRange(child_obj.GetComponents<EventEffect>());

                    int index = 0;
                    foreach (DialogueChoice choice in child_obj.GetComponents<DialogueChoice>())
                    {
                        child.choices.Add(choice);
                        choice.choice_index = index;
                        index++;
                    }

                    event_lines.Add(child);
                }
            }
        }

        void OnDestroy()
        {
            event_list.Remove(this);
        }

        void Start()
        {
            if (trigger_type == EventType.AtStart)
            {
                OnTriggerEvent();
            }

            if (trigger_type == EventType.EnterRegion && trigger_target != null)
            {
                Region region_trigger = trigger_target.GetComponent<Region>();
                region_trigger.onEnterRegion += OnTriggerEvent;
            }
            if (trigger_type == EventType.LeaveRegion && trigger_target != null)
            {
                Region region_trigger = trigger_target.GetComponent<Region>();
                region_trigger.onExitRegion += OnTriggerEvent;
            }

            if (trigger_type == EventType.AfterEvent)
            {
                EventTrigger event_trigger = trigger_target.GetComponent<EventTrigger>();
                if (event_trigger)
                {
                    event_trigger.onEnd += OnTriggerEvent;
                }
            }

        }

        void Update()
        {
            if (!EventManager.IsActive())
                return;

            //Auto trigger
            if (EventManager.IsReady() && trigger_type == EventType.AutoTrigger)
            {
                TriggerIfConditionsMet();
            }

            //Interval trigger
            if (EventManager.IsReady() && trigger_type == EventType.Interval)
            {
                float timestamp = TheGame.Get().GetTimestamp();
                float next_timestamp = SaveEventData.Get().GetLastInterval(id) + trigger_value;
                if (timestamp > next_timestamp && AreConditionsMet())
                {
                    Trigger();
                }
            }
        }

        private void OnTriggerEvent()
        {
            TriggerIfConditionsMet();
        }

        private void OnTriggerEvent(Character character)
        {
            TriggerIfConditionsMet(character?.gameObject);
        }

        public void AddConditions(EventCondition[] group_conditions)
        {
            conditions.AddRange(group_conditions);
        }
		
        public void Trigger(GameObject triggerer = null)
        {
            if (EventManager.Get().GetCurrent() != this)
            {
                this.triggerer = triggerer;
                EventManager.Get().AddToTriggerList(this);
            }
        }

        public void TriggerIfConditionsMet(GameObject triggerer = null)
        {
            if (AreConditionsMet(triggerer))
            {
                Trigger(triggerer);
            }
        }

        public void TriggerImmediately(GameObject triggerer = null)
        {
            if (EventManager.Get().GetCurrent() != this)
            {
                this.triggerer = triggerer;
                EventManager.Get().StartEvent(this);
            }
        }

        public void SaveTrigger()
        {
            //Increment
            int cur_val = SaveEventData.Get().GetTriggerCount(id);
            SaveEventData.Get().SetTriggerCount(id, cur_val + 1);
            float timestamp = TheGame.Get().GetTimestamp();
            SaveEventData.Get().SetLastInterval(id, timestamp);
            trigger_count++;
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

        public bool AreConditionsMet(GameObject triggerer = null)
        {
            bool conditions_met = true;
            foreach (EventCondition condition in conditions)
            {
                if (condition.enabled && !condition.IsMet())
                {
                    conditions_met = false;
                }
            }

            int game_trigger_count = SaveEventData.Get().GetTriggerCount(id);
            bool below_max = (trigger_limit == 0 || game_trigger_count < trigger_limit)
                && (trigger_limit == 0 || trigger_count < trigger_limit);

            return conditions_met && below_max && gameObject.activeSelf;
        }

        public GameObject GetTriggerer()
        {
            return triggerer;
        }

        public int GetTriggerCount()
        {
            return trigger_count;
        }

        public int GetPriority()
        {
            return priority;
        }

        public List<EventLine> GetLines()
        {
            return event_lines;
        }
		
		public static EventTrigger Get(string event_id)
		{
			foreach (EventTrigger evt in event_list)
            {
                if (evt.id == event_id)
                    return evt;
            }
			return null;
		}

        //Call if you want to reset the trigger count of all events
        public static void ResetAll()
        {
            foreach (EventTrigger evt in GetAll())
            {
                evt.trigger_count = 0;
                SaveEventData.Get().SetTriggerCount(evt.id, 0);
            }
        }

        public static EventTrigger[] GetAll()
        {
            return event_list.ToArray();
        }
    }
}