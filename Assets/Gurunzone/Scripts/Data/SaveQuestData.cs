using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{

    [System.Serializable]
    public class SaveQuestData
    {
        public Dictionary<string, int> quests_status = new Dictionary<string, int>(); //0=NotStarted, 1=Ongoing, 2=Completed, 3=Failed
        public Dictionary<string, int> quests_progress = new Dictionary<string, int>(); //ID is quest_id+title

        public void FixData()
        {
            if (quests_status == null)
                quests_status = new Dictionary<string, int>();
            if (quests_progress == null)
                quests_progress = new Dictionary<string, int>();
        }

        public void StartQuest(string quest_id)
        {
            if (!IsQuestStarted(quest_id))
                quests_status[quest_id] = 1;
        }

        public void CancelQuest(string quest_id)
        {
            if (IsQuestStarted(quest_id))
                quests_status[quest_id] = 0;
        }

        public void CompleteQuest(string quest_id)
        {
            if (IsQuestStarted(quest_id))
                quests_status[quest_id] = 2;
        }

        public void FailQuest(string quest_id)
        {
            if (IsQuestStarted(quest_id))
                quests_status[quest_id] = 3;
        }

        public int GetQuestStatus(string quest_id)
        {
            if (quests_status.ContainsKey(quest_id))
                return quests_status[quest_id];
            return 0;
        }

        public bool IsQuestStarted(string quest_id)
        {
            int status = GetQuestStatus(quest_id);
            return status >= 1;
        }

        public bool IsQuestActive(string quest_id)
        {
            int status = GetQuestStatus(quest_id);
            return status == 1;
        }

        public bool IsQuestCompleted(string quest_id)
        {
            int status = GetQuestStatus(quest_id);
            return status == 2;
        }

        public bool IsQuestFailed(string quest_id)
        {
            int status = GetQuestStatus(quest_id);
            return status == 3;
        }

        public void AddQuestProgress(string quest_id, string progress, int value)
        {
            string id = quest_id + "-" + progress;
            if (quests_progress.ContainsKey(id))
                quests_progress[id] += value;
            else
                quests_progress[id] = value;
        }

        public void SetQuestProgress(string quest_id, string progress, int value)
        {
            string id = quest_id + "-" + progress;
            quests_progress[id] = value;
        }

        public int GetQuestProgress(string quest_id, string progress)
        {
            string id = quest_id + "-" + progress;
            if (quests_progress.ContainsKey(id))
                return quests_progress[id];
            return 0;
        }

        public bool IsQuestProgressCompleted(string quest_id, string progress, int max)
        {
            string id = quest_id + "-" + progress;
            if (quests_progress.ContainsKey(quest_id))
                return quests_progress[id] >= max;
            return false;
        }

        public static SaveQuestData Get()
        {
            return SaveData.Get().quest_data;
        }
    }
}
