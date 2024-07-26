using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gurunzone
{
    /// <summary>
    /// Basic class for any type of slot (item, other)
    /// </summary>

    public class UISlot : MonoBehaviour, IPointerClickHandler, IPointerDownHandler,
                IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [HideInInspector]
        public int index = -1;

        public UnityAction<UISlot> onClick;
        public UnityAction<UISlot> onClickRight;
        public UnityAction<UISlot> onClickLong;
        public UnityAction<UISlot> onClickDouble;

        public UnityAction<UISlot> onMouseEnter;
        public UnityAction<UISlot> onMouseExit;

        public UnityAction<UISlot> onDragStart; //When you started dragging and exit the first slot
        public UnityAction<UISlot> onDragEnd; //When dragging and releasing
        public UnityAction<UISlot, UISlot> onDragTo; //When dragging slot and releasing on another slot

        protected EventTrigger evt_trigger;
        protected RectTransform rect;
        protected UISlotPanel parent;

        protected bool active = true;
        protected bool selected = false;
        protected bool key_hover = false;

        private bool is_holding = false;
        private bool can_drag = false;
        private bool is_dragging = false;
        private bool can_click = false;
        private float holding_timer = 0f;
        private float double_timer = 0f;

        private static List<UISlot> slot_list = new List<UISlot>();

        protected virtual void Awake()
        {
            slot_list.Add(this);
            parent = GetComponentInParent<UISlotPanel>();
            rect = GetComponent<RectTransform>();
            evt_trigger = GetComponent<EventTrigger>();
        }

        protected virtual void OnDestroy()
        {
            slot_list.Remove(this);
        }

        protected virtual void Start()
        {

        }

        protected virtual void Update()
        {
            if (double_timer < 1f)
                double_timer += Time.deltaTime;

            //Hold
            if (is_holding)
            {
                holding_timer += Time.deltaTime;
                if (holding_timer > 0.5f)
                {
                    can_click = false;
                    can_drag = false;
                    is_holding = false;

                    if (onClickLong != null)
                        onClickLong.Invoke(this);
                }
            }

            if(!active)
                gameObject.SetActive(false);
        }

        public void SelectSlot()
        {
            selected = true;
        }

        public void UnselectSlot()
        {
            selected = false;
        }

        public void SetSelected(bool sel)
        {
            selected = sel;
        }

        public bool IsSelected()
        {
            return selected;
        }

        public void Show()
        {
            active = true;
            if (active != gameObject.activeSelf)
                gameObject.SetActive(true);
        }

        public void Hide()
        {
            active = false;
        }

        void OnClick(BaseEventData eventData)
        {
            if (can_click)
            {

            }
        }

        void OnDown(BaseEventData eventData)
        {
            is_holding = true;
            can_drag = true;
            is_dragging = false;
            can_click = true;
            holding_timer = 0f;

            PointerEventData pEventData = eventData as PointerEventData;

            if (pEventData.button == PointerEventData.InputButton.Right)
            {
                if (onClickRight != null)
                    onClickRight.Invoke(this);
            }
            else if (pEventData.button == PointerEventData.InputButton.Left)
            {
                if (double_timer < 0f)
                {
                    double_timer = 0f;
                    if (onClickDouble != null)
                        onClickDouble.Invoke(this);
                }
                else
                {
                    double_timer = -0.3f;
                    if (onClick != null)
                        onClick.Invoke(this);
                }
            }
        }

        void OnUp(BaseEventData eventData)
        {
            is_holding = false;
            can_drag = false;

            //Drag n drop
            if (is_dragging)
            {
                is_dragging = false;
                onDragEnd?.Invoke(this);
                Vector3 anchor_pos = TheUI.Get().ScreenPointToCanvasPos(TheControls.Get().GetMousePosition());
                UISlot target = UISlot.GetNearestActive(anchor_pos, 50f);
                if (target != null && target != this)
                    onDragTo?.Invoke(this, target);
            }
        }

        void OnEnter(BaseEventData eventData)
        {
            onMouseEnter?.Invoke(this);
        }

        void OnExit(BaseEventData eventData)
        {
            is_holding = false;
            onMouseExit?.Invoke(this);

            if (can_drag)
            {
                can_drag = false;
                is_dragging = true;
                onDragStart?.Invoke(this);
            }
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick(eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDown(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnUp(eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnEnter(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnExit(eventData);
        }

        public bool IsVisible()
        {
            return gameObject.activeSelf && (parent == null || parent.IsVisible());
        }

        public bool IsDrag()
        {
            return is_dragging;
        }

        public RectTransform GetRect()
        {
            return rect;
        }

        public UISlotPanel GetParent()
        {
            return parent;
        }

        public static UISlot GetDrag()
        {
            foreach (UISlot slot in slot_list)
            {
                if (slot.IsDrag())
                    return slot;
            }
            return null;
        }

        public static UISlot GetNearestActive(Vector2 anchor_pos, float range = 999f)
        {
            UISlot nearest = null;
            float min_dist = range;
            foreach (UISlot slot in slot_list)
            {
                Vector2 canvas_pos = TheUI.Get().WorldToCanvasPos(slot.transform.position);
                float dist = (canvas_pos - anchor_pos).magnitude;
                if (dist < min_dist && slot.gameObject.activeInHierarchy)
                {
                    min_dist = dist;
                    nearest = slot;
                }
            }
            return nearest;
        }

    }

}