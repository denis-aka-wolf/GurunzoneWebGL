using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gurunzone
{

    public class QuestUI : MonoBehaviour
    {
        public Text title;
        public Image panel;
        public QuestLine[] quest_lines;

        private float refresh_timer = 0f;

        void Start()
        {
            if(title != null)
                title.enabled = false;
            if(panel != null)
                panel.enabled = false;
            foreach (QuestLine line in quest_lines)
                line.Hide();
        }

        void Update()
        {
            refresh_timer += Time.deltaTime;
            if (refresh_timer > 2f)
            {
                refresh_timer = 0f;
                RefreshQuest();
            }
        }

        private void RefreshQuest()
        {
            foreach (QuestLine line in quest_lines)
                line.Hide();

            List<QuestData> quests = QuestData.GetAllActive();

            if (title != null)
                title.enabled = quests.Count > 0;
            if (panel != null)
                panel.enabled = quests.Count > 0;

            int index = 0;
            foreach (QuestData quest in quests)
            {
                if (index < quest_lines.Length)
                {
                    QuestLine line = quest_lines[index];
                    line.SetQuest(quest);
                    index++;
                }
            }
        }
    }
}
