using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gurunzone.Events
{
    public enum NarrativeConditionType
    {
        None = 0,
        
        IsVisible = 10,
        InsideRegion = 12,
        CountSceneObjects = 15,

        HasItem = 20,
        HasItemGroup=21,
        HasConstruction = 22,
        HasTech = 24,

        ColonistAttribute = 30,
        AverageAttribute = 32,

        Day =40,
        WeekDay=41,
        DayTime=42,

        EventTriggered = 50,

        QuestStarted = 60, //Either active or completed
        QuestActive = 62, //Started but not completed
        QuestCompleted = 64, //Quest is completed
        QuestFailed = 65, //Quest is failed
        QuestProgress = 67,

        Probability = 70,
        RandomValue = 72,

        CustomInt = 91,
        CustomFloat = 92,
        CustomString = 93,

        CustomCondition = 99,
    }

    public enum NarrativeConditionOperator
    {
        Equal = 0,
        NotEqual = 1,
        GreaterEqual = 2,
        LessEqual = 3,
        Greater = 4,
        Less = 5,
    }

    public enum NarrativeConditionOperator2
    {
        IsTrue = 0,
        IsFalse = 1,
    }

    public enum NarrativeConditionTargetType
    {
        Value = 0,
        Target = 1,
    }
    
    public class EventCondition : MonoBehaviour
    {
        public NarrativeConditionType type;
        public NarrativeConditionOperator oper;
        public NarrativeConditionOperator2 oper2;
        public NarrativeConditionTargetType target_type;
        public string target_id = "";
        public string other_target_id;

        public GameObject value_object;
        public ScriptableObject value_data;
        public GroupData value_group;
        public CustomCondition value_custom;
        public AttributeType value_attr;
        public int value_int = 0;
        public float value_float = 0f;
        public string value_string = "";

        private void Start()
        {
            if (value_custom != null && !value_custom.inited)
            {
                value_custom.inited = true;
                value_custom.Start();
            }
        }

        public bool IsMet()
        {
            bool condition_met = false;

            if (type == NarrativeConditionType.None)
            {
                condition_met = true;
                if (oper2 == NarrativeConditionOperator2.IsFalse)
                    condition_met = !condition_met;
            }

            if (type == NarrativeConditionType.CustomInt)
            {
                int i1 = SaveEventData.Get().GetCustomInt(target_id);
                int i2 = target_type == NarrativeConditionTargetType.Target ? SaveEventData.Get().GetCustomInt(other_target_id) : value_int;
                condition_met = CompareInt(oper, i1, i2);
            }

            if (type == NarrativeConditionType.CustomFloat)
            {
                float f1 = SaveEventData.Get().GetCustomFloat(target_id);
                float f2 = target_type == NarrativeConditionTargetType.Target ? SaveEventData.Get().GetCustomFloat(other_target_id) : value_float;
                condition_met = CompareFloat(oper, f1, f2);
            }

            if (type == NarrativeConditionType.CustomString)
            {
                string s1 = SaveEventData.Get().GetCustomString(target_id);
                string s2 = target_type == NarrativeConditionTargetType.Target ? SaveEventData.Get().GetCustomString(other_target_id) : value_string;
                condition_met = CompareString(oper, s1, s2);
            }

            if (type == NarrativeConditionType.IsVisible)
            {
                condition_met = (value_object != null && value_object.activeSelf);
                if (oper2 == NarrativeConditionOperator2.IsFalse)
                    condition_met = !condition_met;
            }
            
            if (type == NarrativeConditionType.InsideRegion)
            {
                Region region = Region.Get(target_id);
                if (value_object && region)
                    condition_met = region.IsInsideXZ(value_object.transform.position);
                if (oper2 == NarrativeConditionOperator2.IsFalse)
                    condition_met = !condition_met;
            }

            if (type == NarrativeConditionType.CountSceneObjects)
            {
                GameObject[] objs = GameObject.FindGameObjectsWithTag(target_id);
                int i1 = objs.Length;
                int i2 = value_int;
                condition_met = CompareInt(oper, i1, i2);
            }


            if (type == NarrativeConditionType.Day)
            {
                int i1 = SaveData.Get().day;
                condition_met = CompareFloat(oper, i1, value_int);
            }

            if (type == NarrativeConditionType.WeekDay)
            {
                int i1 = SaveData.Get().day;
                i1 = (i1 + 6) % 7 + 1; //Return value between 1 and 7
                condition_met = CompareFloat(oper, i1, value_int);
            }

            if (type == NarrativeConditionType.DayTime)
            {
                float f1 = SaveData.Get().day_time;
                condition_met = CompareFloat(oper, f1, value_float);
            }

            if (type == NarrativeConditionType.HasItem)
            {
                Inventory global = Inventory.GetGlobal();
                ItemData item = (ItemData)value_data;
                int i1 = item != null ? global.CountItem(item) : 0;
                condition_met = CompareInt(oper, i1, value_int);
            }

            if (type == NarrativeConditionType.HasItemGroup)
            {
                Inventory global = Inventory.GetGlobal();
                int i1 = value_group != null ? global.CountItemGroup(value_group) : 0;
                condition_met = CompareInt(oper, i1, value_int);
            }

            if (type == NarrativeConditionType.HasConstruction)
            {
                ConstructionData construct = (ConstructionData)value_data;
                int i1 = Construction.CountConstructions(construct);
                condition_met = CompareInt(oper, i1, value_int);
            }

            if (type == NarrativeConditionType.HasTech)
            {

                TechData tech = (TechData)value_data;
                condition_met = TechManager.Get().IsTechCompleted(tech);
            }

            if (type == NarrativeConditionType.ColonistAttribute)
            {
                Colonist colonist = Colonist.Get((ColonistData)value_data);
                float i1 = colonist?.Attributes != null ? colonist.Attributes.GetAttributeValue(value_attr) : 0f;
                float i2 = value_float;
                condition_met = CompareFloat(oper, i1, i2);
            }

            if (type == NarrativeConditionType.AverageAttribute)
            {
                float i1 = ColonistManager.Get().GetAverageAttribute(value_attr);
                float i2 = value_float;
                condition_met = CompareFloat(oper, i1, i2);
            }

            if (type == NarrativeConditionType.EventTriggered)
            {
                GameObject targ = value_object;
                if (targ && targ.GetComponent<EventTrigger>())
                {
                    EventTrigger evt = targ.GetComponent<EventTrigger>();
                    condition_met = evt.GetTriggerCount() >= 1;
                    if (oper2 == NarrativeConditionOperator2.IsFalse)
                        condition_met = !condition_met;
                }
            }

            if (type == NarrativeConditionType.QuestStarted)
            {
                QuestData quest = (QuestData)value_data;
                if (quest != null)
                {
                    condition_met = SaveQuestData.Get().IsQuestStarted(quest.quest_id);
                    if (oper2 == NarrativeConditionOperator2.IsFalse)
                        condition_met = !condition_met;
                }
            }

            if (type == NarrativeConditionType.QuestActive)
            {
                QuestData quest = (QuestData)value_data;
                if (quest != null)
                {
                    condition_met = SaveQuestData.Get().IsQuestActive(quest.quest_id);
                    if (oper2 == NarrativeConditionOperator2.IsFalse)
                        condition_met = !condition_met;
                }
            }

            if (type == NarrativeConditionType.QuestCompleted)
            {
                QuestData quest = (QuestData)value_data;
                if (quest != null)
                {
                    condition_met = SaveQuestData.Get().IsQuestCompleted(quest.quest_id);
                    if (oper2 == NarrativeConditionOperator2.IsFalse)
                        condition_met = !condition_met;
                }
            }

            if (type == NarrativeConditionType.QuestFailed)
            {
                QuestData quest = (QuestData)value_data;
                if (quest != null)
                {
                    condition_met = SaveQuestData.Get().IsQuestFailed(quest.quest_id);
                    if (oper2 == NarrativeConditionOperator2.IsFalse)
                        condition_met = !condition_met;
                }
            }

            if (type == NarrativeConditionType.QuestProgress)
            {
                QuestData quest = (QuestData)value_data;
                if (quest != null)
                {
                    int avalue = SaveQuestData.Get().GetQuestProgress(quest.quest_id, target_id);
                    condition_met = CompareInt(oper, avalue, value_int);
                }
            }

            if (type == NarrativeConditionType.Probability)
            {
                float i1 = Random.value;
                float i2 = value_float;
                condition_met = CompareFloat(oper, i1, i2);
            }

            if (type == NarrativeConditionType.RandomValue)
            {
                float i1 = EventManager.Get().GetRandomValue(target_id);
                float i2 = value_float;
                condition_met = CompareFloat(oper, i1, i2);
            }

            if (type == NarrativeConditionType.CustomCondition)
            {
                if (value_custom != null)
                {
                    condition_met = value_custom.IsMet();
                    if (oper2 == NarrativeConditionOperator2.IsFalse)
                        condition_met = !condition_met;
                }
            }

            return condition_met;
        }

        public static bool CompareInt(NarrativeConditionOperator oper, int ival1, int ival2)
        {
            bool condition_met = true;
            if (oper == NarrativeConditionOperator.Equal && ival1 != ival2)
            {
                condition_met = false;
            }
            if (oper == NarrativeConditionOperator.NotEqual && ival1 == ival2)
            {
                condition_met = false;
            }
            if (oper == NarrativeConditionOperator.GreaterEqual && ival1 < ival2)
            {
                condition_met = false;
            }
            if (oper == NarrativeConditionOperator.LessEqual && ival1 > ival2)
            {
                condition_met = false;
            }
            if (oper == NarrativeConditionOperator.Greater && ival1 <= ival2)
            {
                condition_met = false;
            }
            if (oper == NarrativeConditionOperator.Less && ival1 >= ival2)
            {
                condition_met = false;
            }
            return condition_met;
        }

        public static bool CompareFloat(NarrativeConditionOperator oper, float fval1, float fval2)
        {
            bool condition_met = true;
            if (oper == NarrativeConditionOperator.Equal && fval1 != fval2)
            {
                condition_met = false;
            }
            if (oper == NarrativeConditionOperator.NotEqual && fval1 == fval2)
            {
                condition_met = false;
            }
            if (oper == NarrativeConditionOperator.GreaterEqual && fval1 < fval2)
            {
                condition_met = false;
            }
            if (oper == NarrativeConditionOperator.LessEqual && fval1 > fval2)
            {
                condition_met = false;
            }
            if (oper == NarrativeConditionOperator.Greater && fval1 <= fval2)
            {
                condition_met = false;
            }
            if (oper == NarrativeConditionOperator.Less && fval1 >= fval2)
            {
                condition_met = false;
            }
            return condition_met;
        }

        public static bool CompareString(NarrativeConditionOperator oper, string sval1, string sval2)
        {
            bool condition_met = true;
            if (oper == NarrativeConditionOperator.Equal && sval1 != sval2)
            {
                condition_met = false;
            }
            if (oper == NarrativeConditionOperator.NotEqual && sval1 == sval2)
            {
                condition_met = false;
            }
            if (oper == NarrativeConditionOperator.GreaterEqual && sval1 != sval2)
            {
                condition_met = false;
            }
            if (oper == NarrativeConditionOperator.LessEqual && sval1 != sval2)
            {
                condition_met = false;
            }
            if (oper == NarrativeConditionOperator.Greater && sval1 == sval2)
            {
                condition_met = false;
            }
            if (oper == NarrativeConditionOperator.Less && sval1 == sval2)
            {
                condition_met = false;
            }
            return condition_met;
        }
    }

}
