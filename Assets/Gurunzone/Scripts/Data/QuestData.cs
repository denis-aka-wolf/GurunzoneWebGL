using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    [CreateAssetMenu(fileName ="Quest", menuName = "Gurunzone/AppData/QuestData", order= 10)]
    public class QuestData : ScriptableObject {

        [Tooltip("Important: make sure all quests have a unique ID")]
        public string quest_id;
        public Sprite icon;
        [TextArea(1, 2)]
        public string title;
        [TextArea(3, 5)]
        public string desc;
        public int sort_order;

        private static List<QuestData> quest_list = new List<QuestData>();

        public void Start(){ QuestManager.Get().StartQuest(this);}
        public void Complete(){ QuestManager.Get().CompleteQuest(this);}
        public void Fail(){ QuestManager.Get().FailQuest(this); }
        public void Cancel(){ QuestManager.Get().CancelQuest(this); }

        public void AddQuestProgress(string progress, int value){ QuestManager.Get().AddQuestProgress(this, progress, value); }
        public void SetQuestProgress(string progress, int value){ QuestManager.Get().SetQuestProgress(this, progress, value); }

        public bool IsStarted() { return SaveQuestData.Get().IsQuestStarted(quest_id); }
        public bool IsActive() { return SaveQuestData.Get().IsQuestActive(quest_id); }
        public bool IsCompleted() { return SaveQuestData.Get().IsQuestCompleted(quest_id); }
        public bool IsFailed() { return SaveQuestData.Get().IsQuestFailed(quest_id); }
        public int GetQuestStatus(){ return SaveQuestData.Get().GetQuestStatus(quest_id);}
        public int GetQuestProgress(string progress) { return SaveQuestData.Get().GetQuestProgress(quest_id, progress); }

        public string GetTitle()
        {
            return title;
        }

        public string GetDesc()
        {
            return desc;
        }

        public static void Load(string folder = "")
        {
            quest_list.Clear();
            quest_list.AddRange(Resources.LoadAll<QuestData>(folder));
            quest_list.Sort((QuestData a, QuestData b) => {
                if (a.sort_order == b.sort_order)
                    return a.title.CompareTo(b.title);
                return a.sort_order.CompareTo(b.sort_order);
            });
        }

        public static QuestData Get(string actor_id)
        {
            if (QuestManager.Get())
            {
                foreach (QuestData quest in GetAll())
                {
                    if (quest.quest_id == actor_id)
                        return quest;
                }
            }
            return null;
        }

        public static List<QuestData> GetAllActive()
        {
            List<QuestData> valid_list = new List<QuestData>();
            foreach (QuestData aquest in GetAll())
            {
                if (SaveQuestData.Get().IsQuestActive(aquest.quest_id))
                    valid_list.Add(aquest);
            }
            return valid_list;
        }

        public static List<QuestData> GetAllStarted()
        {
            List<QuestData> valid_list = new List<QuestData>();
            foreach (QuestData aquest in GetAll())
            {
                if (SaveQuestData.Get().IsQuestStarted(aquest.quest_id))
                    valid_list.Add(aquest);
            }
            return valid_list;
        }

        public static List<QuestData> GetAllActiveOrCompleted()
        {
            List<QuestData> valid_list = new List<QuestData>();
            foreach (QuestData aquest in GetAll())
            {
                if (SaveQuestData.Get().IsQuestActive(aquest.quest_id) || SaveQuestData.Get().IsQuestCompleted(aquest.quest_id))
                    valid_list.Add(aquest);
            }
            return valid_list;
        }

        public static List<QuestData> GetAllActiveOrFailed()
        {
            List<QuestData> valid_list = new List<QuestData>();
            foreach (QuestData aquest in GetAll())
            {
                if (SaveQuestData.Get().IsQuestActive(aquest.quest_id) || SaveQuestData.Get().IsQuestFailed(aquest.quest_id))
                    valid_list.Add(aquest);
            }
            return valid_list;
        }

        public static List<QuestData> GetAll()
        {
            return quest_list;
        }
    }
}
