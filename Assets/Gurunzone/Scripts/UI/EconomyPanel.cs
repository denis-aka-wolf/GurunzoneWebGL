using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{

    public class EconomyPanel : UIPanel
    {
        public GameObject lines_content;

        private EconomyLine[] lines;

        private float timer = 0f;

        private static EconomyPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
            lines = lines_content.GetComponentsInChildren<EconomyLine>();
        }

        protected override void Update()
        {
            base.Update();

            timer += Time.deltaTime;
            if (timer > 1f)
            {
                timer = 0f;
                RefreshPanel();
            }
        }

        private void RefreshPanel()
        {
            foreach (EconomyLine line in lines)
                line.Hide();

            Inventory global = Inventory.GetGlobal();
            int index = 0;
            foreach (ItemData item in ItemData.GetAll())
            {
                ItemSet set = global.GetItem(item);
                if (set != null && set.quantity > 0)
                {
                    if (index < lines.Length)
                    {
                        EconomyLine line = lines[index];
                        line.SetLine(item);
                        index++;
                    }
                }
            }
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            RefreshPanel();
        }

        public static EconomyPanel Get()
        {
            return instance;
        }
    }
}