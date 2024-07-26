using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gurunzone.Events
{
    public enum NarrativeEffectType
    {
        None = 0,

        Show = 10,
        Hide = 11,
        Spawn = 14,
        Create = 15,
        Destroy = 16,

        GainItem=20,
        PayItemGroup=22,
        GainTech=24,

        AddColonistAttribute=30,
        SetColonistAttribute = 32,
        AddGlobalAttribute = 34,
        SetGlobalAttribute = 36,


        CharacterOrder =40,
        CharacterMoveTo=42,

        StartEvent = 50,
        StartEventIfMet = 51,
        StartRandomEvent = 52,

        StartQuest = 60,
        CancelQuest = 61,
        CompleteQuest = 62,
        FailQuest = 63,
        QuestProgress = 67,

        SelectRandomTarget = 70,
        SelectRandomValue = 72,

        PlaySFX = 80,
        PlayMusic = 82,
        StopMusic = 84,

        SetCustomTarget = 90,
        SetCustomInt = 91,
        SetCustomFloat = 92,
        SetCustomString = 93,

        Wait = 95,
        CustomEffect = 97,
        CallFunction = 99,
    }

    public enum NarrativeEffectOperator
    {
        Add = 0,
        Set = 1,
    }

    [System.Serializable]
    public class NarrativeEffectCallback : UnityEvent<int> { }

    public class EventEffect : MonoBehaviour
    {
        public NarrativeEffectType type;
        public string target_id = "";
        public NarrativeEffectOperator oper;
        public GameObject value_object;
        public ScriptableObject value_data;
        public GroupData value_group;
        public AudioClip value_audio;
        public CustomEffect value_custom;
        public AttributeType value_attr;
        public EventTarget value_target;
        public EventTarget value_target2;
        public int value_int = 0;
        public float value_float = 1f;
        public string value_string = "";

        [SerializeField]
        public UnityEvent callfunc_evt;

        private void Start()
        {
            if (value_custom != null && !value_custom.inited)
            {
                value_custom.inited = true;
                value_custom.Start();
            }
        }

        public void Trigger()
        {
            if (type == NarrativeEffectType.SetCustomInt)
            {
                if(oper == NarrativeEffectOperator.Set)
                    SaveEventData.Get().SetCustomInt(target_id, value_int);

                if (oper == NarrativeEffectOperator.Add)
                {
                    int value = SaveEventData.Get().GetCustomInt(target_id);
                    SaveEventData.Get().SetCustomInt(target_id, value + value_int);
                }
            }

            if (type == NarrativeEffectType.SetCustomFloat)
            {
                if (oper == NarrativeEffectOperator.Set)
                    SaveEventData.Get().SetCustomFloat(target_id, value_float);

                if (oper == NarrativeEffectOperator.Add)
                {
                    float value = SaveEventData.Get().GetCustomFloat(target_id);
                    SaveEventData.Get().SetCustomFloat(target_id, value + value_float);
                }
            }

            if (type == NarrativeEffectType.SetCustomString)
            {
                SaveEventData.Get().SetCustomString(target_id, value_string);
            }

            if (type == NarrativeEffectType.SetCustomTarget)
            {
                GameObject target = value_target?.GetTarget();
                EventManager.Get().SetCustomTarget(target_id, target);
            }

            if (type == NarrativeEffectType.Show)
            {
                GameObject targ = value_target?.GetTarget();
                if(targ != null){
                    UniqueID uid = targ.GetComponent<UniqueID>();
                    if(uid != null)
                        uid.Show();
                    else
                        targ.SetActive(true);
                }
            }

            if (type == NarrativeEffectType.Hide)
            {
                GameObject targ = value_target?.GetTarget();
                if (targ != null)
                {
                    UniqueID uid = targ.GetComponent<UniqueID>();
                    if (uid != null)
                        uid.Hide();
                    else
                        targ.SetActive(false);
                }
            }

            if (type == NarrativeEffectType.Spawn)
            {
                GameObject location = value_target.GetTarget();
                if (location != null)
                {
                    Vector3 pos = value_target.GetTargetPos();
                    GameObject obj = Instantiate(value_object, pos, Quaternion.identity);
                    EventManager.Get().SetLastCreated(obj);
                }
            }

            if (type == NarrativeEffectType.Create)
            {
                GameObject location = value_target.GetTarget();
                if (location != null)
                {
                    Vector3 pos = value_target.GetTargetPos();
                    GameObject obj = CSObject.Create((CSData)value_data, pos);
                    EventManager.Get().SetLastCreated(obj);
                }
            }

            if (type == NarrativeEffectType.Destroy)
            {
                GameObject targ = value_target?.GetTarget();
                if (targ != null)
                {
                    Selectable select = targ.GetComponent<Selectable>();
                    if (select != null)
                        select.Kill();
                    else
                        Destroy(targ);
                }
            }

            if(type == NarrativeEffectType.GainItem){
                ItemData item = (ItemData)value_data;
                Inventory global = Inventory.GetGlobal();
                global.AddItem(item, value_int);
            }

            if (type == NarrativeEffectType.PayItemGroup)
            {
                Inventory global = Inventory.GetGlobal();
                global.PayItemGroup(value_group, value_int);
            }

            if (type == NarrativeEffectType.GainTech)
            {
                TechData tech = (TechData)value_data;
                TechManager techs = TechManager.Get();
                if (techs != null && tech != null)
                    techs.CompleteTech(tech);
            }

            if (type == NarrativeEffectType.AddColonistAttribute)
            {
                ColonistData cdata = (ColonistData)value_data;
                GroupData gdata = value_group;
                if (cdata != null)
                {
                    Colonist colonist = Colonist.Get((ColonistData)value_data);
                    if (colonist?.Attributes != null)
                        colonist.Attributes.AddAttribute(value_attr, value_float);
                }
                else if (gdata != null)
                {
                    foreach (Colonist colonist in Colonist.GetAllGroup(gdata))
                    {
                        if (colonist?.Attributes != null)
                            colonist.Attributes.AddAttribute(value_attr, value_float);
                    }
                }
            }

            if (type == NarrativeEffectType.SetColonistAttribute)
            {
                ColonistData cdata = (ColonistData)value_data;
                GroupData gdata = value_group;
                if (cdata != null)
                {
                    Colonist colonist = Colonist.Get((ColonistData)value_data);
                    if (colonist?.Attributes != null)
                        colonist.Attributes.SetAttribute(value_attr, value_float);
                }
                else if (gdata != null)
                {
                    foreach (Colonist colonist in Colonist.GetAllGroup(gdata))
                    {
                        if (colonist?.Attributes != null)
                            colonist.Attributes.SetAttribute(value_attr, value_float);
                    }
                }
            }

            if (type == NarrativeEffectType.AddGlobalAttribute)
            {
                AttributeData attribute = AttributeData.Get(value_attr);
                SaveData.Get().AddAttributeValue(value_attr, value_float, attribute.max_value);
            }

            if (type == NarrativeEffectType.SetGlobalAttribute)
            {
                AttributeData attribute = AttributeData.Get(value_attr);
                SaveData.Get().SetAttributeValue(value_attr, value_float, attribute.max_value);
            }

            if (type == NarrativeEffectType.CharacterOrder)
            {
                ActionBasic action = (ActionBasic)value_data;
                GameObject cobj = value_target.GetTarget();
                GameObject target = value_target2.GetTarget();
                Character character = cobj?.GetComponent<Character>();
                Interactable tselect = target?.GetComponent<Interactable>();
                if (character != null && tselect != null && action != null)
                {
                    character.Order(action, tselect);
                }
            }

            if (type == NarrativeEffectType.CharacterMoveTo)
            {
                GameObject cobj = value_target.GetTarget();
                GameObject target = value_target2.GetTarget();
                Character character = cobj?.GetComponent<Character>();
                Vector3 tpos = value_target2.GetTargetPos();
                if (character != null && target != null)
                {
                    character.MoveTo(tpos);
                }
            }

            if (type == NarrativeEffectType.SelectRandomTarget)
            {
                EventManager.Get().SelectRandomTargets(target_id, value_group, value_int);
            }

            if (type == NarrativeEffectType.SelectRandomValue)
            {
                EventManager.Get().RollRandomValue(target_id, 0f, value_float);
            }

            if (type == NarrativeEffectType.StartEvent)
            {
                EventTrigger nevent = value_object.GetComponent<EventTrigger>();
                if (nevent != null)
                {
                    nevent.TriggerImmediately();
                }
            }

            if (type == NarrativeEffectType.StartEventIfMet)
            {
                EventTrigger nevent = value_object.GetComponent<EventTrigger>();
                if (nevent != null)
                {
                    nevent.TriggerIfConditionsMet();
                }
            }

            if (type == NarrativeEffectType.StartRandomEvent)
            {
                EventGroup nevent = value_object.GetComponent<EventGroup>();
                if (nevent != null)
                {
                    EventTrigger evt = nevent.GetRandom();
                    if(evt != null)
                        evt.TriggerIfConditionsMet();
                }
            }

            if (type == NarrativeEffectType.StartQuest)
            {
                QuestData quest = (QuestData)value_data;
                QuestManager.Get().StartQuest(quest);
            }

            if (type == NarrativeEffectType.CancelQuest)
            {
                QuestData quest = (QuestData)value_data;
                QuestManager.Get().CancelQuest(quest);
            }

            if (type == NarrativeEffectType.CompleteQuest)
            {
                QuestData quest = (QuestData)value_data;
                QuestManager.Get().CompleteQuest(quest);
            }

            if (type == NarrativeEffectType.FailQuest)
            {
                QuestData quest = (QuestData)value_data;
                QuestManager.Get().FailQuest(quest);
            }

            if (type == NarrativeEffectType.QuestProgress)
            {
                QuestData quest = (QuestData)value_data;
                if (oper == NarrativeEffectOperator.Add)
                    QuestManager.Get().AddQuestProgress(quest, target_id, value_int);
                else if (oper == NarrativeEffectOperator.Set)
                    QuestManager.Get().SetQuestProgress(quest, target_id, value_int);
            }

            if (type == NarrativeEffectType.PlaySFX)
            {
                TheAudio.Get().PlaySFX(value_string, value_audio, value_float);
            }

            if (type == NarrativeEffectType.PlayMusic)
            {
                TheAudio.Get().PlayMusic(value_string, value_audio, value_float);
            }

            if (type == NarrativeEffectType.StopMusic)
            {
                TheAudio.Get().StopMusic(value_string);
            }

            if (type == NarrativeEffectType.CustomEffect)
            {
                if (value_custom != null)
                {
                    value_custom.DoEffect();
                }
            }

            if (type == NarrativeEffectType.CallFunction)
            {
                if (callfunc_evt != null)
                {
                    callfunc_evt.Invoke();
                }
            }

        }

        public float GetWaitTime()
        {
            if (type == NarrativeEffectType.Wait)
            {
                return value_float;
            }
            return 0f;
        }
    }

}