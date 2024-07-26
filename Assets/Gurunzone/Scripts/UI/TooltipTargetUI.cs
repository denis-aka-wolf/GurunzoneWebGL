using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Gurunzone
{

    public class TooltipTargetUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public string title;

        [TextArea(5, 7)]
        public string desc;
        public Sprite icon;

        public float delay = 0.5f;
        public int text_size = 22;
        public int width = 350;
        public int height = 140;

        private Canvas canvas;
        private RectTransform rect;
        private float timer = 0f;
        private bool hover = false;

        void Awake()
        {
            canvas = GetComponentInParent<Canvas>();
            rect = canvas.GetComponent<RectTransform>();
        }

        void Start()
        {

        }

        void Update()
        {
            if (hover && !TheGame.IsMobile())
            {
                timer += Time.deltaTime;
                if (timer > delay)
                {
                    TooltipBox.Get().Show(this);
                    TooltipBox.Get().SetSize(width, height, text_size);
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            timer = 0f;
            hover = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            timer = 0f;
            hover = false;
        }

        void OnDisable()
        {
            hover = false;
        }

        public Canvas GetCanvas()
        {
            return canvas;
        }

        public RectTransform GetRect()
        {
            return rect;
        }

        public bool IsHover()
        {
            return hover;
        }
    }
}
