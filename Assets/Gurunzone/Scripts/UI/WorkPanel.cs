using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    public class WorkPanel : UIPanel
    {
        public GameObject lines_content;

        private WorkLine[] lines;

        private static WorkPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
            lines = lines_content.GetComponentsInChildren<WorkLine>();
        }

        private void RefreshPanel()
        {
            foreach (WorkLine line in lines)
                line.Hide();

            int index = 0;
            foreach (Colonist colonist in Colonist.GetAll())
            {
                if (index < lines.Length)
                {
                    WorkLine line = lines[index];
                    line.SetLine(colonist);
                    index++;
                }
            }
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            RefreshPanel();
        }

        public static WorkPanel Get()
        {
            return instance;
        }
    }
}
