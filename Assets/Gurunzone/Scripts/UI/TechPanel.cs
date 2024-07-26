using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gurunzone
{

    public class TechPanel : UIPanel
    {
        public GameObject desc_group;
        public Text title;
        public Text desc;
        public CraftSlot[] cost_slot;
        public Text progress_txt;
        public Text completed_txt;
        public Button research_btn;

        private TechSlotButton[] tech_slots;

        private TechData tech_selected = null;

        private static TechPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
            tech_slots = GetComponentsInChildren<TechSlotButton>();
            foreach (TechSlotButton slot in tech_slots)
                slot.onClick += OnClickSlot;
        }

        protected override void Update()
        {
            base.Update();

            TechManager tmanager = TechManager.Get();
            progress_txt.enabled = tech_selected != null && tmanager.IsTechResearch(tech_selected);
            completed_txt.enabled = tech_selected != null && tmanager.IsTechCompleted(tech_selected);

            if (tech_selected != null)
            {
                progress_txt.text = Mathf.RoundToInt(tmanager.GetTechProgressPercent(tech_selected) * 100f) + "%";
            }
        }

        private void RefreshDesc()
        {
            if (desc_group == null)
                return;

            desc_group.SetActive(tech_selected != null);

            if (tech_selected != null)
            {
                title.text = tech_selected.title;
                desc.text = tech_selected.desc;
                research_btn.interactable = TechManager.Get().CanBeResearched(tech_selected);
                research_btn.gameObject.SetActive(!TechManager.Get().IsTechStarted(tech_selected));

                foreach (CraftSlot slot in cost_slot)
                    slot.Hide();

                foreach (TechSlotButton tslot in tech_slots)
                {
                    tslot.GetSlot().SetHighlight(tslot.tech == tech_selected);
                }

                int index = 0;
                CraftCostData cost = tech_selected.GetCraftCost();
                foreach (KeyValuePair<ItemData, int> item in cost.items)
                {
                    if (index < cost_slot.Length)
                    {
                        CraftSlot slot = cost_slot[index];
                        slot.SetSlot(item.Key, item.Value);
                        index++;
                    }
                }
                foreach (KeyValuePair<GroupData, int> item in cost.fillers)
                {
                    if (index < cost_slot.Length)
                    {
                        CraftSlot slot = cost_slot[index];
                        slot.SetSlot(item.Key, item.Value);
                        index++;
                    }
                }
                foreach (KeyValuePair<CraftData, int> pair in cost.requirements)
                {
                    if (index < cost_slot.Length)
                    {
                        CraftSlot cslot = cost_slot[index];
                        cslot.SetSlot(pair.Key);
                        index++;
                    }
                }
            }
        }

        private void HideDesc()
        {
            foreach (TechSlotButton tslot in tech_slots)
                tslot.GetSlot().SetHighlight(false);

            if (desc_group != null)
                desc_group.SetActive(false);
        }

        public void OnClickResearch()
        {
            if (TechManager.Get().CanBeResearched(tech_selected))
            {
                TechManager.Get().PayTechCost(tech_selected);
                TechManager.Get().StartTechResearch(tech_selected);
                RefreshDesc();
            }
        }

        private void OnClickSlot(TechSlot slot)
        {
            TechData tech = slot.GetTech();
            tech_selected = tech;
            RefreshDesc();
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            HideDesc();
        }

        public static TechPanel Get()
        {
            return instance;
        }
    }
}
