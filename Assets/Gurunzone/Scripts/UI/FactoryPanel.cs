using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gurunzone
{

    public class FactoryPanel : SelectPanel
    {
        public Text title;
        public Text population;
        public Text quantity;
        public Text warning;
        public CraftSlot selected_item;
        public IntSelector workers_select;
        public ProgressBar progress;

        public CraftSlot[] slots;

        private Factory factory;
        private House house;
        private float timer = 0f;

        private static FactoryPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;

            title.text = "";
            population.text = "";
            quantity.text = "";
            warning.text = "";
            progress.gameObject.SetActive(false);
            foreach (CraftSlot slot in slots)
                slot.Hide();
        }

        protected override void Start()
        {
            base.Start();
            workers_select.onChange += OnChangeWorkers;

            foreach (CraftSlot slot in slots)
                slot.onClick += OnSelectItem;
        }

        protected override void Update()
        {
            base.Update();

            if (factory == null || !factory.IsAlive())
            {
                Hide();
                return;
            }

            workers_select.value_max = factory.Workable.workers_max;
            workers_select.SetValue(factory.Workable.GetWorkerAmount());
            progress.value = factory.GetProgress();
            progress.value_max = factory.GetProgressMax();

            if (house != null)
            {
                int pop = Colonist.CountPopulation();
                int max = House.CountMaxPopulation();
                population.text = pop + " / " + max;
            }

            CraftData selected = factory.GetSelected();
            progress.gameObject.SetActive(selected != null);
            if (selected != null)
                selected_item.SetSlot(factory.GetSelected());
            else
                selected_item.Hide();

            //quantity
            quantity.text = "";
            if (quantity != null && selected != null && selected is ItemData
                && factory.Inventory != null && !factory.Inventory.global)
            {
                ItemData idata = (ItemData) selected;
                ItemSet set = factory.Inventory.GetItem(idata);
                if(set != null)
                    quantity.text = set.quantity.ToString();
            }

            int index = 0;
            foreach (CraftData item in factory.items)
            {
                if (index < slots.Length)
                {
                    CraftSlot slot = slots[index];
                    slot.SetHighlight(item == factory.GetSelected());
                    index++;
                }
            }

            timer += Time.deltaTime;
            if (timer > 0.5f)
            {
                timer = 0f;
                UpdateWarning();
            }
        }

        private void UpdateWarning()
        {
            warning.text = "";

            CraftData selected = factory.GetSelected();
            Inventory global = Inventory.GetGlobal();

            if (selected != null)
            {
                if(Character.CountTargetingTarget(factory.Interactable) == 0 && factory.auto_work_speed < 0.0001f)
                    warning.text = "No Worker Assigned";

                if (!global.HasCraftCost(selected))
                    warning.text = "Missing Resources";

                if (!selected.HasRequirements())
                    warning.text = "Missing Requirements";

                if (factory.IsPopCap())
                    warning.text = "Population limit reached";
            }
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);

            Factory fact = select.GetComponent<Factory>();
            factory = fact;
            title.text = factory.Construction != null ? factory.Construction.data.title : "";
            warning.text = "";

            bool worker_selector = factory.Workable.workers_max > 0;
            workers_select.gameObject.SetActive(worker_selector);

            House house = select.GetComponent<House>();
            this.house = house;

            foreach (CraftSlot slot in slots)
                slot.Hide();

            int index = 0;
            foreach (CraftData item in fact.items)
            {
                if (index < slots.Length)
                {
                    CraftSlot slot = slots[index];
                    slot.SetSlot(item);
                    index++;
                }
            }
        }

        private void OnChangeWorkers(int value)
        {
            if (factory)
                factory.Workable.SetWorkerAmount(value);
        }

        private void OnSelectItem(UISlot uslot)
        {
            CraftSlot slot = (CraftSlot)uslot;
            if (slot.GetItem() != factory.GetSelected())
            {
                factory.SelectItem(slot.GetItem());
            }
            else
            {
                factory.UnselectItem();
            }
        }

        public override bool IsShowable(Selectable select)
        {
            Factory fact = select.GetComponent<Factory>();
            return fact != null;
        }

        public override int GetPriority()
        {
            return 20;
        }

        public static FactoryPanel Get()
        {
            return instance;
        }
    }

}
