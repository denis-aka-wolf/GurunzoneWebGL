using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    public enum TooltipTargetType
    {
        Custom = 0,
        Automatic = 10,
    }

    [RequireComponent(typeof(Selectable))]
    public class TooltipTarget : MonoBehaviour
    {
        public TooltipTargetType type;

        [Header("Custom")]
        public string title;
        public Sprite icon;
        [TextArea(3,5)]
        public string desc;

        [Header("Options")]
        public float delay = 0.5f;
        public int text_size = 22;
        public int width = 350;
        public int height = 140;

        private Construction construct;
        private Item item;
        private Colonist colonist;
        private bool hover = false;
        private float timer = 0f;

        void Awake()
        {
            construct = GetComponent<Construction>();
            item = GetComponent<Item>();
            colonist = GetComponent<Colonist>();
        }

        void Update()
        {
            if (hover && !TheGame.IsMobile() && TooltipBox.Get() != null) {

                timer += Time.deltaTime;
                if (timer > delay)
                {
                    TooltipBox.Get().Show(this);
                    TooltipBox.Get().SetSize(width, height, text_size);
                }
            }
        }

        public string GetTitle()
        {
            if (type == TooltipTargetType.Automatic)
            {
                if (construct != null) return construct.data.title;
                if (colonist != null) return colonist.data.title;
                if (item != null) return item.data.title;
            }
            return title;
        }

        public Sprite GetIcon()
        {

            if (type == TooltipTargetType.Automatic)
            {

            }
            return icon;
        }

        public string GetDesc()
        {

            if (type == TooltipTargetType.Automatic)
            {
                if (construct != null) return construct.data.title;
                if (colonist != null) return colonist.data.title;
                if (item != null) return item.data.title;
            }
            return desc;
        }

        public bool IsHover()
        {
            return hover;
        }

        private void OnMouseEnter()
        {
            timer = 0f;
            hover = true;
        }

        private void OnMouseExit()
        {
            hover = false;
            timer = 0f;
        }
    }

}