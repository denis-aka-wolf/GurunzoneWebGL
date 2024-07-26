using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gurunzone
{

    public class LevelsPanel : UIPanel
    {
        public AttributeUI[] levels;

        private Colonist colonist;

        private static LevelsPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }

        private void RefreshPanel()
        {
            foreach (AttributeUI attr in levels)
                attr.Hide();

            int index = 0;
            foreach (LevelData lvl in colonist.Attributes.levels)
            {
                if (index < levels.Length)
                {
                    AttributeUI attr_ui = levels[index];
                    int level = colonist.Attributes.GetLevel(lvl.id);
                    int max = lvl.level_max;
                    attr_ui.SetLevel(lvl, level, max);
                    index++;
                }
            }
        }

        public void ShowColonist(Colonist colonist)
        {
            this.colonist = colonist;
            if (colonist != null && colonist.Attributes != null && colonist.Character.Equip != null)
            {
                Show();
                RefreshPanel();
            }
        }

        public static LevelsPanel Get()
        {
            return instance;
        }
    }

}