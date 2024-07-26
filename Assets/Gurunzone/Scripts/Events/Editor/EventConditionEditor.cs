using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Gurunzone.Events.EditorTool
{

    [CustomEditor(typeof(EventCondition))]
    public class EventConditionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EventCondition myScript = target as EventCondition;

            bool value_compare = false;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Type", GetLabelWidth());
            GUILayout.FlexibleSpace();
            myScript.type = (NarrativeConditionType)EditorGUILayout.EnumPopup(myScript.type, GetWidth());
            GUILayout.EndHorizontal();

            if (myScript.type == NarrativeConditionType.CustomInt
                || myScript.type == NarrativeConditionType.CustomFloat
                || myScript.type == NarrativeConditionType.CustomString)
            {

                myScript.target_id = AddTextField("Custom ID", myScript.target_id);
                myScript.oper = (NarrativeConditionOperator)AddEnumField("Operator", myScript.oper);
                myScript.target_type = (NarrativeConditionTargetType)AddEnumField("Other Type", myScript.target_type);
                value_compare = true;

                if (myScript.target_type == NarrativeConditionTargetType.Target)
                {
                    myScript.other_target_id = AddTextField("Other Target ID", myScript.other_target_id);
                }

                if (myScript.target_type == NarrativeConditionTargetType.Value)
                {
                    if (myScript.type == NarrativeConditionType.CustomInt)
                    {
                        myScript.value_int = AddIntField("Int Value", myScript.value_int);
                    }

                    if (myScript.type == NarrativeConditionType.CustomFloat)
                    {
                        myScript.value_float = AddFloatField("Float Value", myScript.value_float);
                    }

                    if (myScript.type == NarrativeConditionType.CustomString)
                    {
                        myScript.value_string = AddTextField("String Value", myScript.value_string);
                    }
                }

            }

            //Special cases
            if (myScript.type == NarrativeConditionType.IsVisible)
            {
                myScript.value_object = AddGameObjectField("Target", myScript.value_object);
            }
            
            if (myScript.type == NarrativeConditionType.InsideRegion)
            {
                myScript.value_object = AddGameObjectField("Target", myScript.value_object);
                myScript.target_id = AddTextField("Region ID", myScript.target_id);
            }

            if (myScript.type == NarrativeConditionType.CountSceneObjects)
            {
                myScript.target_id = AddTextField("Objects Tag", myScript.target_id);
                myScript.oper = (NarrativeConditionOperator)AddEnumField("Operator", myScript.oper);
                myScript.value_int = AddIntField("Value", myScript.value_int);
                value_compare = true;
            }

            if (myScript.type == NarrativeConditionType.EventTriggered)
            {
                myScript.value_object = AddGameObjectField("Event", myScript.value_object);
            }

            if (myScript.type == NarrativeConditionType.QuestActive || myScript.type == NarrativeConditionType.QuestStarted
                || myScript.type == NarrativeConditionType.QuestCompleted || myScript.type == NarrativeConditionType.QuestFailed)
            {
                myScript.value_data = AddScriptableObjectField<QuestData>("Quest", myScript.value_data);
            }

            if (myScript.type == NarrativeConditionType.QuestProgress)
            {
                myScript.value_data = AddScriptableObjectField<QuestData>("Quest", myScript.value_data);
                myScript.target_id = AddTextField("Progress ID", myScript.target_id);
                myScript.oper = (NarrativeConditionOperator)AddEnumField("Operator", myScript.oper);
                myScript.value_int = AddIntField("Value", myScript.value_int);
                value_compare = true;
            }

            if (myScript.type == NarrativeConditionType.Day){
                myScript.oper = (NarrativeConditionOperator)AddEnumField("Operator", myScript.oper);
                myScript.value_int = AddIntField("Value", myScript.value_int);
                value_compare = true;
            }

            if(myScript.type == NarrativeConditionType.WeekDay){
                myScript.oper = (NarrativeConditionOperator)AddEnumField("Operator", myScript.oper);
                myScript.value_int = AddIntField("Value", myScript.value_int);
                value_compare = true;
            }

            if(myScript.type == NarrativeConditionType.DayTime){
                myScript.oper = (NarrativeConditionOperator)AddEnumField("Operator", myScript.oper);
                myScript.value_float = AddFloatField("Value", myScript.value_float);
                value_compare = true;
            }

            if(myScript.type == NarrativeConditionType.HasItem){
                myScript.value_data = AddScriptableObjectField<ItemData>("Item", myScript.value_data);
                myScript.oper = (NarrativeConditionOperator)AddEnumField("Operator", myScript.oper);
                myScript.value_int = AddIntField("Value", myScript.value_int);
                value_compare = true;
            }
            
            if (myScript.type == NarrativeConditionType.HasItemGroup)
            {
                myScript.value_group = AddScriptableObjectField<GroupData>("Group", myScript.value_group);
                myScript.oper = (NarrativeConditionOperator)AddEnumField("Operator", myScript.oper);
                myScript.value_int = AddIntField("Value", myScript.value_int);
                value_compare = true;
            }

            if (myScript.type == NarrativeConditionType.HasConstruction){
                myScript.value_data = AddScriptableObjectField<ConstructionData>("Construction", myScript.value_data);
                myScript.oper = (NarrativeConditionOperator)AddEnumField("Operator", myScript.oper);
                myScript.value_int = AddIntField("Value", myScript.value_int);
                value_compare = true;
            }
            if(myScript.type == NarrativeConditionType.HasTech){
                myScript.value_data = AddScriptableObjectField<TechData>("Tech", myScript.value_data);
                myScript.oper = (NarrativeConditionOperator)AddEnumField("Operator", myScript.oper);
                myScript.value_int = AddIntField("Value", myScript.value_int);
                value_compare = true;
            }

            if (myScript.type == NarrativeConditionType.ColonistAttribute)
            {
                myScript.value_data = AddScriptableObjectField<ColonistData>("Target", myScript.value_data);
                myScript.value_attr = (AttributeType)AddEnumField("Attribute", myScript.value_attr);
                myScript.oper = (NarrativeConditionOperator)AddEnumField("Operator", myScript.oper);
                myScript.value_float = AddFloatField("Value", myScript.value_float);
                value_compare = true;
            }

            if (myScript.type == NarrativeConditionType.AverageAttribute)
            {
                myScript.value_attr = (AttributeType)AddEnumField("Attribute", myScript.value_attr);
                myScript.oper = (NarrativeConditionOperator)AddEnumField("Operator", myScript.oper);
                myScript.value_float = AddFloatField("Value", myScript.value_float);
                value_compare = true;
            }

            if (myScript.type == NarrativeConditionType.Probability)
            {
                GUILayout.Label("Probability is from 0 to 1");
                myScript.oper = (NarrativeConditionOperator)AddEnumField("Operator", myScript.oper);
                myScript.value_float = AddFloatField("Value", myScript.value_float);
                value_compare = true;
            }

            if (myScript.type == NarrativeConditionType.RandomValue)
            {
                myScript.target_id = AddTextField("ID", myScript.target_id);
                myScript.oper = (NarrativeConditionOperator)AddEnumField("Operator", myScript.oper);
                myScript.value_float = AddFloatField("Value", myScript.value_float);
                value_compare = true;
            }

            if (myScript.type == NarrativeConditionType.CustomCondition)
            {
                GUILayout.Label("Use a script that inherit from CustomCondition");
                myScript.value_custom = AddScriptableObjectField<CustomCondition>("Custom Condition", myScript.value_custom);
            }

            if (!value_compare)
            {
                myScript.oper2 = (NarrativeConditionOperator2)AddEnumField("Operator", myScript.oper2);
            }
            
            if (GUI.changed && !Application.isPlaying)
            {
                EditorUtility.SetDirty(myScript);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                AssetDatabase.SaveAssets();
            }
        }

        private string AddTextField(string label, string value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GetLabelWidth());
            GUILayout.FlexibleSpace();
            string outval = EditorGUILayout.TextField(value, GetWidth());
            GUILayout.EndHorizontal();
            return outval;
        }

        private string AddTextAreaField(string label, string value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GetShortLabelWidth());
            GUILayout.FlexibleSpace();
            EditorStyles.textField.wordWrap = true;
            string outval = EditorGUILayout.TextArea(value, GetLongWidth(), GUILayout.Height(50));
            GUILayout.EndHorizontal();
            return outval;
        }

        private int AddIntField(string label, int value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GetLabelWidth());
            GUILayout.FlexibleSpace();
            int outval = EditorGUILayout.IntField(value, GetWidth());
            GUILayout.EndHorizontal();
            return outval;
        }

        private float AddFloatField(string label, float value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GetLabelWidth());
            GUILayout.FlexibleSpace();
            float outval = EditorGUILayout.FloatField(value, GetWidth());
            GUILayout.EndHorizontal();
            return outval;
        }

        private System.Enum AddEnumField(string label, System.Enum value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GetLabelWidth());
            GUILayout.FlexibleSpace();
            System.Enum outval = EditorGUILayout.EnumPopup(value, GetWidth());
            GUILayout.EndHorizontal();
            return outval;
        }

        private GameObject AddGameObjectField(string label, GameObject value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GetLabelWidth());
            GUILayout.FlexibleSpace();
            GameObject outval = (GameObject)EditorGUILayout.ObjectField(value, typeof(GameObject), true, GetWidth());
            GUILayout.EndHorizontal();
            return outval;
        }

        private T AddScriptableObjectField<T>(string label, ScriptableObject value) where T : ScriptableObject
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GetLabelWidth());
            GUILayout.FlexibleSpace();
            T outval = (T)EditorGUILayout.ObjectField(value, typeof(T), true, GetWidth());
            GUILayout.EndHorizontal();
            return outval;
        }
        
        private Sprite AddSpriteField(string label, Sprite value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GetLabelWidth());
            GUILayout.FlexibleSpace();
            Sprite outval = (Sprite)EditorGUILayout.ObjectField(value, typeof(Sprite), true, GetWidth());
            GUILayout.EndHorizontal();
            return outval;
        }

        private AudioClip AddAudioField(string label, AudioClip value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GetLabelWidth());
            GUILayout.FlexibleSpace();
            AudioClip outval = (AudioClip)EditorGUILayout.ObjectField(value, typeof(AudioClip), true, GetWidth());
            GUILayout.EndHorizontal();
            return outval;
        }

        private GUILayoutOption GetLabelWidth()
        {
            return GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.4f);
        }

        private GUILayoutOption GetWidth()
        {
            return GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.45f);
        }

        private GUILayoutOption GetShortLabelWidth()
        {
            return GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.25f);
        }

        private GUILayoutOption GetLongWidth()
        {
            return GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.65f);
        }
    }

}