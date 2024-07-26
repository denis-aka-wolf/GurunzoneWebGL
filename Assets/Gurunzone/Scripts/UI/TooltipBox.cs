using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gurunzone
{

    public class TooltipBox : UIPanel
    {
        public RectTransform box;
        public Text title;
        public Text desc;
        public Image icon;

        public GameObject cost_group;
        public CraftSlot[] cost_slots;

        private TooltipTarget current;
        private TooltipTargetUI current_ui;
        private CraftSlot current_craft;
        private TechSlot current_tech;
        private RectTransform rect;

        private int start_width;
        private int start_height;
        private int start_text_size;

        private static TooltipBox instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
            rect = GetComponent<RectTransform>();
            start_width = Mathf.RoundToInt(rect.sizeDelta.x);
            start_height = Mathf.RoundToInt(rect.sizeDelta.y);
            start_text_size = desc.fontSize;
        }

        protected override void Update()
        {
            base.Update();

            Vector2 pos = TheUI.Get().ScreenPointToCanvasPos(TheControls.Get().GetMousePosition());
            rect.anchoredPosition = pos;

            if (current != null && !current.IsHover())
                Hide(current);

            if (current_ui != null && !current_ui.IsHover())
                Hide(current_ui);

            if (current_craft != null && !current_craft.IsHover())
                Hide(current_craft);

            if (current_tech != null && !current_tech.IsHover())
                Hide(current_tech);
        }

        private void UpdateAnchoring()
        {
            Vector2 pos = TheUI.Get().ScreenPointToCanvasPos(TheControls.Get().GetMousePosition());
            Vector2 csize = TheUI.Get().GetCanvasSize() * 0.5f;
            float pivotX = Mathf.Sign(pos.x - csize.x * 0.5f) * 0.5f + 0.5f;
            float pivotY = Mathf.Sign(pos.y + csize.y * 0.5f) * 0.5f + 0.5f;
            box.pivot = new Vector2(pivotX, pivotY);
            box.anchoredPosition = Vector2.zero;
            rect.anchoredPosition = pos;
        }

        public void Show(TooltipTarget tooltip)
        {
            current = tooltip;
            current_ui = null;
            current_craft = null;
            current_tech = null;

            title.text = tooltip.GetTitle();
            desc.text = tooltip.GetDesc();
            icon.sprite = tooltip.GetIcon();
            icon.enabled = icon.sprite != null;

            if (cost_group != null)
                cost_group.SetActive(false);

            Show();
            UpdateAnchoring();
        }

        public void Show(TooltipTargetUI tooltip)
        {
            current = null;
            current_ui = tooltip;
            current_craft = null;
            current_tech = null;

            title.text = tooltip.title;
            desc.text = tooltip.desc;
            icon.sprite = tooltip.icon;
            icon.enabled = tooltip.icon != null;

            if(cost_group != null)
                cost_group.SetActive(false);

            Show();
            UpdateAnchoring();
        }

        public void Show(CraftSlot slot)
        {
            current = null;
            current_ui = null;
            current_craft = slot;
            current_tech = null;

            CraftData item = slot.GetItem();
            if (item != null)
            {
                title.text = item.title;
                desc.text = item.desc;
                icon.sprite = item.icon;
                icon.enabled = item.icon != null;
                ShowCost(item);
                Show();
                UpdateAnchoring();
            }

            GroupData group = slot.GetGroup();
            if (group != null)
            {
                title.text = group.title;
                desc.text = "";
                icon.sprite = group.icon;
                icon.enabled = group.icon != null;
                cost_group.SetActive(false);
                Show();
                UpdateAnchoring();
            }
        }

        public void Show(TechSlot slot)
        {
            current = null;
            current_ui = null;
            current_craft = null;
            current_tech = slot;

            TechData item = slot.GetTech();
            title.text = item.title;
            desc.text = item.desc;
            icon.sprite = item.icon;
            icon.enabled = item.icon != null;
            ShowCost(item);
            Show();
            UpdateAnchoring();
        }

        private void ShowCost(CraftData item)
        {
            cost_group.SetActive(item != null);
            if (cost_group != null && item != null)
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

        public void Hide(TooltipTarget hover)
        {
            if (current == hover)
            {
                current = null;
                Hide();
            }
        }

        public void Hide(TooltipTargetUI hover)
        {
            if (current_ui == hover)
            {
                current_ui = null;
                Hide();
            }
        }

        public void Hide(CraftSlot slot)
        {
            if (current_craft == slot)
            {
                current_ui = null;
                Hide();
            }
        }

        public void Hide(TechSlot slot)
        {
            if (current_tech == slot)
            {
                current_ui = null;
                Hide();
            }
        }

        public void SetSize(int width, int height, int text)
        {
            rect.sizeDelta = new Vector2(width, height);
            desc.fontSize = text;
        }

        public void ResetSize()
        {
            rect.sizeDelta = new Vector2(start_width, start_height);
            desc.fontSize = start_text_size;
        }

        public static TooltipBox Get()
        {
            return instance;
        }
    }
}
