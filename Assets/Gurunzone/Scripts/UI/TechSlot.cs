using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gurunzone
{

    public class TechSlot : UISlot
    {
        public Text title;
        public Image icon;
        public Image highlight;
        public Image progress;
        public Image completed;
        public float hover_delay = 0.5f;

        private TechData tech;
        private CanvasGroup canvas_group;
        private bool hover = false;
        //private float timer = 0f;

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

            /*if (hover && !TheGame.IsMobile())
            {
                timer += Time.deltaTime;
                if (timer > hover_delay)
                {
                    TooltipBox.Get().Show(this);
                    TooltipBox.Get().ResetSize();
                }
            }*/
        }

        public void SetSlot(TechData tech)
        {
            this.tech = tech;
            icon.sprite = tech.icon;
            title.text = tech.title;
            progress.enabled = false;
            canvas_group.alpha = 1f;
            completed.enabled = false;
            if(highlight != null)
                highlight.enabled = false;
            Show();
        }

        public void SetHighlight(bool active)
        {
            if (highlight != null)
                highlight.enabled = active;
        }

        public void HideProgress()
        {
            progress.enabled = false;
        }

        public void SetProgress(float value)
        {
            progress.enabled = true;
            progress.fillAmount = 1f - Mathf.Clamp01(value);
        }

        public void SetLocked(bool locked)
        {
            canvas_group.alpha = locked ? 0.5f : 1f;
        }

        public void SetCompleted(bool completed)
        {
            this.completed.enabled = completed;
        }

        public TechData GetTech()
        {
            return tech;
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
