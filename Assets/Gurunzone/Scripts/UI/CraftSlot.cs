using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gurunzone
{

    public class CraftSlot : UISlot
    {
        public Text title;
        public Image icon;
        public Image highlight;
        public Text quantity;
        public float hover_delay = 0.5f;
        public bool tooltip = true;

        private CraftData item;
        private GroupData group;
        private CanvasGroup canvas_group;
        private bool valid = true;
        private bool hover = false;
        private float timer = 0f;

        protected override void Awake()
        {
            base.Awake();
            canvas_group = GetComponent<CanvasGroup>();
            onMouseEnter += OnEnter;
            onMouseExit += OnExit;
        }

        protected override void Update()
        {
            base.Update();

            if (hover && tooltip && !TheGame.IsMobile())
            {
                timer += Time.deltaTime;
                if (timer > hover_delay)
                {
                    TooltipBox.Get().Show(this);
                    TooltipBox.Get().ResetSize();
                }
            }
        }

        public void SetSlot(CraftData item, int quantity=0)
        {
            this.item = item;
            this.group = null;
            icon.sprite = item.icon;
            title.text = item.title;
            valid = true;
            Show();

            if(highlight != null)
                highlight.enabled = false;
            if (canvas_group != null)
                canvas_group.alpha = 1f;

            if (this.quantity != null)
            {
                this.quantity.enabled = quantity > 0;
                this.quantity.text = quantity.ToString();
            }
        }

        public void SetSlot(GroupData group, int quantity = 0)
        {
            this.item = null;
            this.group = group;
            icon.sprite = group.icon;
            title.text = group.title;
            valid = true;
            active = true;
            Show();

            if (highlight != null)
                highlight.enabled = false;
            if (canvas_group != null)
                canvas_group.alpha = 1f;

            if (this.quantity != null)
            {
                this.quantity.enabled = quantity > 0;
                this.quantity.text = quantity.ToString();
            }
        }

        //If false, will be grayedout
        public void SetValid(bool active)
        {
            if (canvas_group != null)
                canvas_group.alpha = active ? 1f : 0.5f;
            valid = active;
        }

        //If true will show highlight
        public void SetHighlight(bool active)
        {
            if (highlight != null)
                highlight.enabled = active;
        }

        public void SetQuantityValid(bool valid)
        {
            if (this.quantity != null)
            {
                this.quantity.color = valid ? Color.white : Color.red;
            }
        }

        public CraftData GetItem()
        {
            return item;
        }

        public GroupData GetGroup()
        {
            return group;
        }

        public bool IsValid()
        {
            return valid;
        }

        public bool IsHover()
        {
            return hover;
        }

        private void OnEnter(UISlot slot)
        {
            hover = true;
        }

        private void OnExit(UISlot slot)
        {
            hover = false;
        }

        void OnDisable()
        {
            hover = false;
        }
    }

}
