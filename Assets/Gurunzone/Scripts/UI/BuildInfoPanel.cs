using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gurunzone
{

    public class BuildInfoPanel : UIPanel
    {
        public CraftSlot icon;
        public Text title;
        public Text description;
        public CraftSlot[] cost_slots;
        public Button build_button;

        private ConstructionData building;

        private static BuildInfoPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }

        protected override void Start()
        {
            base.Start();
            TheControls.Get().onRClick += (Vector3) => { Hide(); };
        }

        protected override void Update()
        {
            base.Update();

            if (building != null && IsVisible())
            {
                build_button.interactable = building.HasRequirements() && building.HasCraftCost();
                ShowCost(building);
            }
        }

        public void OnClickBuild()
        {
            if (building != null && building.HasRequirements() && building.HasCraftCost())
            {
                Construction.CreateBuildMode(building);
                BuildPanel.Get().Hide();
                Hide();
            }
        }

        public void ShowBuilding(ConstructionData building)
        {
            this.building = building;
            icon.SetSlot(building);
            title.text = building.title;
            description.text = building.desc;

            ShowCost(building);
            Show();
        }


        private void ShowCost(CraftData item)
        {
            if (item != null)
            {
                Inventory inventory = Inventory.GetGlobal();
                CraftCostData cost = item.GetCraftCost();

                foreach (CraftSlot cslot in cost_slots)
                    cslot.Hide();

                int index = 0;
                foreach (KeyValuePair<ItemData, int> pair in cost.items)
                {
                    if (index < cost_slots.Length)
                    {
                        CraftSlot cslot = cost_slots[index];
                        cslot.SetSlot(pair.Key, pair.Value);
                        cslot.SetQuantityValid(inventory.HasItem(pair.Key, pair.Value));
                        index++;
                    }
                }
                foreach (KeyValuePair<GroupData, int> pair in cost.fillers)
                {
                    if (index < cost_slots.Length)
                    {
                        CraftSlot cslot = cost_slots[index];
                        cslot.SetSlot(pair.Key, pair.Value);
                        cslot.SetQuantityValid(inventory.HasItemGroup(pair.Key, pair.Value));
                        index++;
                    }
                }
                foreach (KeyValuePair<CraftData, int> pair in cost.requirements)
                {
                    if (index < cost_slots.Length)
                    {
                        CraftSlot cslot = cost_slots[index];
                        cslot.SetSlot(pair.Key);
                        index++;
                    }
                }
            }
        }

        public static BuildInfoPanel Get()
        {
            return instance;
        }
    }

}
