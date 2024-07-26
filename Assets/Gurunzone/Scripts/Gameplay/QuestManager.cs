using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gurunzone
{

    public class QuestManager : MonoBehaviour
    {
        public UnityAction<QuestData> onQuestStart;
        public UnityAction<QuestData> onQuestComplete;
        public UnityAction<QuestData> onQuestFail;
        public UnityAction<QuestData> onQuestCancel;

        private static QuestManager instance;

        private void Awake()
        {
            instance = this;
        }

        public void StartQuest(QuestData quest)
        {
            if (quest != null && !SaveQuestData.Get().IsQuestStarted(quest.quest_id))
            {
                SaveQuestData.Get().StartQuest(quest.quest_id);

                if (onQuestStart != null)
                    onQuestStart.Invoke(quest);
            }
        }

        public void CompleteQuest(QuestData quest)
        {
            if (quest != null && SaveQuestData.Get().IsQuestActive(quest.quest_id))
            {
                SaveQuestData.Get().CompleteQuest(quest.quest_id);

                if (onQuestComplete != null)
                    onQuestComplete.Invoke(quest);
            }
        }

        public void FailQuest(QuestData quest)
        {
            if (quest != null && SaveQuestData.Get().IsQuestActive(quest.quest_id))
            {
                SaveQuestData.Get().FailQuest(quest.quest_id);

                if (onQuestFail != null)
                    onQuestFail.Invoke(quest);
            }
        }

        public void CancelQuest(QuestData quest)
        {
            if (quest != null && SaveQuestData.Get().IsQuestActive(quest.quest_id))
            {
                SaveQuestData.Get().CancelQuest(quest.quest_id);

                if (onQuestCancel != null)
                    onQuestCancel.Invoke(quest);
            }
        }

        public int GetQuestStatus(QuestData quest)
        {
            if (quest != null)
                return SaveQuestData.Get().GetQuestStatus(quest.quest_id);
            return 0;
        }

        public void SetQuestProgress(QuestData quest, string progress, int value)
        {
            if (quest != null)
                SaveQuestData.Get().SetQuestProgress(quest.quest_id, progress, value);
        }

        public void AddQuestProgress(QuestData quest, string progress, int value)
        {
            if (quest != null)
                SaveQuestData.Get().AddQuestProgress(quest.quest_id, progress, value);
        }

        public int GetQuestProgress(QuestData quest, string progress)
        {
            if (quest != null)
                return SaveQuestData.Get().GetQuestProgress(quest.quest_id, progress);
            return 0;
        }

        public static QuestManager Get()
        {
            return instance;
        }
    }
}
