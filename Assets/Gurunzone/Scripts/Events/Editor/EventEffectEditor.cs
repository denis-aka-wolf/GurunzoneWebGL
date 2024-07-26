using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Gurunzone.Events.EditorTool
{

    [CustomEditor(typeof(EventEffect))]
    public class EventEffectEditor : Editor
    {
        SerializedProperty sprop;

        internal void OnEnable()
        {
            sprop = serializedObject.FindProperty("callfunc_evt");
        }

        public override void OnInspectorGUI()
        {
            EventEffect myScript = target as EventEffect;

            myScript.type = (NarrativeEffectType)AddEnumField("Type", myScript.type);

            if (myScript.type == NarrativeEffectType.SetCustomInt
                || myScript.type == NarrativeEffectType.SetCustomFloat
                || myScript.type == NarrativeEffectType.SetCustomString
                || myScript.type == NarrativeEffectType.SetCustomTarget)
            {
                myScript.target_id = AddTextField("Custom ID", myScript.target_id);

                if (myScript.type == NarrativeEffectType.SetCustomInt)
                {
                    myScript.oper = (NarrativeEffectOperator)AddEnumField("Operator", myScript.oper);
                    myScript.value_int = AddIntField("Value Int", myScript.value_int);
                }

                if (myScript.type == NarrativeEffectType.SetCustomFloat)
                {
                    myScript.oper = (NarrativeEffectOperator)AddEnumField("Operator", myScript.oper);
                    myScript.value_float = AddFloatField("Value Float", myScript.value_float);
                }

                if (myScript.type == NarrativeEffectType.SetCustomString)
                {
                    myScript.value_string = AddTextField("Value String", myScript.value_string);
                }

                if (myScript.type == NarrativeEffectType.SetCustomTarget)
                {
                    myScript.value_target = AddEventTarget("Target", myScript.value_target);
                }
            }

            if (myScript.type == NarrativeEffectType.Show 
                || myScript.type == NarrativeEffectType.Hide
                || myScript.type == NarrativeEffectType.Destroy)
            {
                myScript.value_target = AddEventTarget("Target", myScript.value_target);
            }

            if (myScript.type == NarrativeEffectType.StartEvent
                || myScript.type == NarrativeEffectType.StartEventIfMet || myScript.type == NarrativeEffectType.StartRandomEvent)
            {
                myScript.value_object = AddGameObjectField("Target", myScript.value_object);
            }

            if (myScript.type == NarrativeEffectType.Spawn)
            {
                myScript.value_object = AddGameObjectField("Prefab", myScript.value_object);
                myScript.value_target = AddEventTarget("Spawn Location", myScript.value_target);
            }

            if (myScript.type == NarrativeEffectType.Create)
            {
                myScript.value_data = AddScriptableObjectField<CraftData>("CraftData", myScript.value_data);
                myScript.value_target = AddEventTarget("Spawn Location", myScript.value_target);
            }

            if (myScript.type == NarrativeEffectType.GainItem)
            {
                myScript.value_data = AddScriptableObjectField<ItemData>("Item", myScript.value_data);
                myScript.value_int = AddIntField("Quantity", myScript.value_int);
            }

            if (myScript.type == NarrativeEffectType.PayItemGroup)
            {
                myScript.value_group = AddScriptableObjectField<GroupData>("Group", myScript.value_group);
                myScript.value_int = AddIntField("Quantity", myScript.value_int);
            }

            if (myScript.type == NarrativeEffectType.GainTech)
            {
                myScript.value_data = AddScriptableObjectField<TechData>("Tech", myScript.value_data);
                myScript.value_int = AddIntField("Quantity", myScript.value_int);
            }

            if (myScript.type == NarrativeEffectType.AddColonistAttribute || myScript.type == NarrativeEffectType.SetColonistAttribute)
            {
                myScript.value_data = AddScriptableObjectField<ColonistData>("Target", myScript.value_data);
                myScript.value_group = AddScriptableObjectField<GroupData>("Group Target", myScript.value_group);
                myScript.value_attr = (AttributeType)AddEnumField("Attribute", myScript.value_attr);
                myScript.value_float = AddFloatField("Value", myScript.value_float);
            }

            if (myScript.type == NarrativeEffectType.AddGlobalAttribute || myScript.type == NarrativeEffectType.SetGlobalAttribute)
            {
                myScript.value_attr = (AttributeType)AddEnumField("Attribute", myScript.value_attr);
                myScript.value_float = AddFloatField("Value", myScript.value_float);
            }

            if (myScript.type == NarrativeEffectType.CharacterOrder)
            {
                myScript.value_target = AddEventTarget("Character", myScript.value_target);
                myScript.value_data = AddScriptableObjectField<ActionBasic>("Action", myScript.value_data);
                myScript.value_target2 = AddEventTarget("Target", myScript.value_target2);
            }

            if (myScript.type == NarrativeEffectType.CharacterMoveTo)
            {
                myScript.value_target = AddEventTarget("Character", myScript.value_target);
                myScript.value_target2 = AddEventTarget("Target", myScript.value_target2);
            }

            if (myScript.type == NarrativeEffectType.StartQuest || myScript.type == NarrativeEffectType.CancelQuest
                || myScript.type == NarrativeEffectType.CompleteQuest || myScript.type == NarrativeEffectType.FailQuest)
            {
                myScript.value_data = AddScriptableObjectField<QuestData>("Quest", myScript.value_data);
            }

            if (myScript.type == NarrativeEffectType.QuestProgress)
            {
                myScript.value_data = AddScriptableObjectField<QuestData>("Quest", myScript.value_data);
                myScript.target_id = AddTextField("Progress ID", myScript.target_id);
                myScript.oper = (NarrativeEffectOperator)AddEnumField("Operator", myScript.oper);
                myScript.value_int = AddIntField("Value", myScript.value_int);
            }

            if (myScript.type == NarrativeEffectType.SelectRandomTarget)
            {
                myScript.target_id = AddTextField("Target ID", myScript.target_id);
                myScript.value_group = AddScriptableObjectField<GroupData>("Group", myScript.value_group);
                myScript.value_int = AddIntField("Quantity", myScript.value_int);
            }

            if (myScript.type == NarrativeEffectType.SelectRandomValue)
            {
                myScript.target_id = AddTextField("Target ID", myScript.target_id);
                myScript.value_float = AddFloatField("Max Value", myScript.value_float);
            }

            if (myScript.type == NarrativeEffectType.PlaySFX || myScript.type == NarrativeEffectType.PlayMusic)
            {
                myScript.value_string = AddTextField("Channel", myScript.value_string);
                myScript.value_audio = AddAudioField("Audio", myScript.value_audio);
                myScript.value_float = AddFloatField("Volume", myScript.value_float);
            }

            if (myScript.type == NarrativeEffectType.StopMusic)
            {
                myScript.value_string = AddTextField("Channel", myScript.value_string);
            }

            if (myScript.type == NarrativeEffectType.Wait)
            {
                myScript.value_float = AddFloatField("Time", myScript.value_float);
            }

            if (myScript.type == NarrativeEffectType.CustomEffect)
            {
                GUILayout.Label("Use a script that inherit from CustomEffect");
                myScript.value_custom = AddScriptableObjectField<CustomEffect>("Custom Effect", myScript.value_custom);
            }

            if (myScript.type == NarrativeEffectType.CallFunction)
            {

                //EditorGUIUtility.LookLikeControls();
                GUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                {
                    EditorGUILayout.PropertyField(sprop, new GUIContent("List Callbacks", ""));
                }

                if (GUI.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                }
                GUILayout.EndHorizontal();
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

        private EventTarget AddEventTarget(string label, EventTarget target)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GetLabelWidth());
            GUILayout.EndHorizontal();

            if (target == null)
                target = new EventTarget();

            target.type = (TargetType) AddEnumField("Type", target.type);
            if (target.type == TargetType.GameObject)
            {
                target.target_object = AddGameObjectField("Target", target.target_object);
            }
            if (target.type == TargetType.Region)
            {
                target.target_id = AddTextField("Region ID", target.target_id);
            }
            if (target.type == TargetType.RandomInGroup)
            {
                target.target_group = AddScriptableObjectField<GroupData>("Group", target.target_group);
            }
            if (target.type == TargetType.CustomTarget)
            {
                target.target_id = AddTextField("Custom Target ID", target.target_id);
            }
            return target;
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