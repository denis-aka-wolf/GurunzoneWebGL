using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Main manager script for dialogues and quests
/// </summary>

namespace Gurunzone.Events
{
    public class EventManager : MonoBehaviour
    {
        public UnityAction<EventTrigger> onEventStart;
        public UnityAction<EventTrigger> onEventEnd;

        public UnityAction<EventLine, DialogueMessage> onDialogueMessageStart;
        public UnityAction<EventLine, DialogueMessage> onDialogueMessageEnd;

        public UnityAction onPauseGameplay;
        public UnityAction onUnpauseGameplay;

        [HideInInspector]
        public bool use_custom_audio = false; //Set this to true to use your own audio system (and connect to the audio 3 events)

        private EventTrigger current_event = null;
        private EventLine current_event_line = null;
        private DialogueMessage current_message = null;

        private List<EventTrigger> trigger_list = new List<EventTrigger>();
        private Queue<EventLine> event_line_queue = new Queue<EventLine>();

        //Targets
        private GameObject last_created = null;
        private Dictionary<string, GameObject> custom_targets = new Dictionary<string, GameObject>();

        private float event_timer = 0f;
        private float pause_duration = 0f;
        private bool is_paused = false;
        private bool should_unpause = false;

        private static EventManager _instance;

        private void Awake()
        {
            _instance = this;
        }

        void Start()
        {
            string current_event = SaveEventData.Get().current_event;
            if (!string.IsNullOrEmpty(current_event))
            {
                EventTrigger evt = EventTrigger.Get(current_event);
                evt?.Trigger();
            }
        }

        void Update()
        {
            //Events
            event_timer += Time.deltaTime;

            if (current_event != null)
            {
                //Stop dialogue
                if (current_message == null && current_event_line != null)
                {
                    if (event_timer > pause_duration)
                    {
                        StopEventLine();
                    }
                }
            }

            if (event_timer > pause_duration)
            {
                if (current_event_line == null && event_line_queue.Count > 0)
                {
                    EventLine next = event_line_queue.Dequeue();
                    next.TriggerLineIfMet();
                }
                else if (current_event_line == null && current_event != null && event_line_queue.Count == 0)
                {
                    StopEvent();
                }
                else if (current_event == null && trigger_list.Count > 0)
                {
                    EventTrigger next = GetPriorityTriggerList();
                    if (next != null)
                    {
                        trigger_list.Remove(next);
                        StartEvent(next);
                    }
                }
            }

            if (should_unpause && event_timer > 0.1f)
            {
                should_unpause = false;
                is_paused = false;
                onUnpauseGameplay?.Invoke();
                TheGame.Get().UnpauseScript();
            }
        }

        public void AddToTriggerList(EventTrigger narrative_event)
        {
            if (!IsInTriggerList(narrative_event))
                trigger_list.Add(narrative_event);
        }

        public bool IsInTriggerList(EventTrigger narrative_event)
        {
            return trigger_list.Contains(narrative_event);
        }

        private EventTrigger GetPriorityTriggerList()
        {
            if (trigger_list.Count > 0) {
                EventTrigger priority = trigger_list[0];
                foreach (EventTrigger evt in trigger_list)
                {
                    if (evt.GetPriority() < priority.GetPriority())
                        priority = evt;
                }
                return priority;
            }
            return null;
        }

        public void StartEvent(EventTrigger event_trigger)
        {
            if (current_event != event_trigger)
            {
                StopEvent();

                //Debug.Log("Start Cinematic: " + cinematic_trigger.gameObject.name);

                current_event = event_trigger;
                current_message = null;
                event_timer = 0f;
                should_unpause = is_paused && !current_event.pause_gameplay;

                pause_duration = current_event.TriggerEffects();
                SaveEventData.Get().current_event = event_trigger.id;

                foreach (EventLine line in current_event.GetLines())
                    event_line_queue.Enqueue(line);

                if (onEventStart != null)
                    onEventStart.Invoke(event_trigger);
                if (event_trigger.onStart != null)
                    event_trigger.onStart.Invoke();

                if (event_trigger.pause_gameplay && !is_paused)
                {
                    is_paused = true;
                    onPauseGameplay?.Invoke();
                    TheGame.Get().PauseScript();
                }
            }
        }

        public void StopEvent()
        {
            StopDialogue();

            if (current_event != null)
            {
                //Debug.Log("Stop Cinematic");
                EventTrigger trigger = current_event;
                current_event = null;
                current_event_line = null;
                current_message = null;
                event_timer = 0f;
                pause_duration = 0f;
                should_unpause = is_paused;

                trigger.SaveTrigger();
                event_line_queue.Clear();
                SaveEventData.Get().current_event = "";

                if (onEventEnd != null)
                    onEventEnd.Invoke(trigger);
                if (trigger.onEnd != null)
                    trigger.onEnd.Invoke();
            }
        }

        public void StartEventLine(EventLine line)
        {
            if (current_event_line != line)
            {
                current_event_line = line;
                current_message = null;
                event_timer = 0f;
                pause_duration = 0f;

                pause_duration = current_event_line.TriggerEffects();

                if (line.dialogue != null)
                {
                    StartDialogue(line.dialogue);
                }
            }
        }

        public void StopEventLine()
        {
            if (current_event_line != null)
            {
                current_event_line = null;
                current_message = null;
                event_timer = 0f;
                pause_duration = 0f;
            }
        }

        public void StartDialogue(DialogueMessage dialogue)
        {
            StopDialogue();

            //Debug.Log("Start Dialogue " + dialogue_index);
            current_message = dialogue;
            pause_duration = current_message.pause;
            event_timer = 0f;

            if (onDialogueMessageStart != null)
                onDialogueMessageStart.Invoke(current_event_line, current_message);

            if (current_message.onStart != null)
                current_message.onStart.Invoke();
        }

        public void SelectChoice(int choice_index)
        {
            if (current_event_line != null && current_event_line.GetChoice(choice_index) != null) {

                StopDialogue();

                DialogueChoice choice = current_event_line.GetChoice(choice_index);
                if (choice.onSelect != null)
                    choice.onSelect.Invoke(choice.choice_index);

                if (choice.go_to != null)
                    choice.go_to.TriggerImmediately();
            }
        }

        public void StopDialogue()
        {
            if (current_message != null) {

                if (current_message.onEnd != null)
                    current_message.onEnd.Invoke();
                if (onDialogueMessageEnd != null)
                    onDialogueMessageEnd.Invoke(current_event_line, current_message);

                pause_duration = current_message.pause;
                current_message = null;
                event_timer = 0f;
            }
        }

        public void RollRandomValue(string id, float min, float max)
        {
            float value = Random.Range(min, max);
            SaveEventData.Get().SetCustomFloat(id, value);
        }

        public float GetRandomValue(string id)
        {
            return SaveEventData.Get().GetCustomFloat(id);
        }

        public void SelectRandomTargets(string target_id, GroupData group, int quantity)
        {
            if (!string.IsNullOrWhiteSpace(target_id))
            {
                List<Selectable> list = Selectable.GetRandom(group, quantity);
                if(list.Count > 0)
                    custom_targets[target_id] = list[Random.Range(0, list.Count)]?.gameObject;
            }
        }

        public string GetTargetTitle(string target_id)
        {
            GameObject target = GetCustomTarget(target_id);
            CSObject spawn = target?.GetComponent<CSObject>();
            if (spawn != null)
            {
                CSData data = spawn.GetData();
                if (data != null && data is CraftData)
                {
                    CraftData cdata = (CraftData)data;
                    return cdata.title;
                }
            }
            return "";
        }

        //Set custom target for retrieving in another event/effect (short term only, not saved)
        public void SetCustomTarget(string id, GameObject targ)
        {
            if (!string.IsNullOrEmpty(id) && targ != null)
                custom_targets[id] = targ;
        }

        public GameObject GetCustomTarget(string id)
        {
            if (custom_targets.ContainsKey(id))
                return custom_targets[id];
            return null;
        }

        public void DeleteCustomTarget(string id)
        {
            custom_targets.Remove(id);
        }

        public void SetLastCreated(GameObject last) { last_created = last; }
        public GameObject GetLastCreated() { return last_created; }

        public GameObject GetEventTriggerer()
        {
            if (current_event != null)
                return current_event.GetTriggerer();
            return null;
        }

        //Currently has an event/dialogue running
        public bool IsRunning()
        {
            return (current_event != null);
        }

        public EventTrigger GetCurrent()
        {
            return current_event;
        }

        public EventLine GetCurrentLine()
        {
            return current_event_line;
        }

        public DialogueMessage GetCurrentMessage()
        {
            return current_message;
        }

        //Is enabled
        public static bool IsActive()
        {
            return _instance != null && _instance.enabled;
        }

        //Is ready to receive events (not already one running)
        public static bool IsReady()
        {
            return IsActive() && !_instance.IsRunning() && !TheGame.Get().IsPaused();
        }

        public static EventManager Get()
        {
            return _instance;
        }
    }

}