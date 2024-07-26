using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Gurunzone
{
    /// <summary>
    /// Controls manager
    /// </summary>

    public enum SelectMode
    {
        Select = 0,
        Build=10,
    }

    public class TheControls : MonoBehaviour
    {
        [Header("Actions")]
        public KeyCode action_key = KeyCode.Space;
        public KeyCode build_rotate_left_key = KeyCode.Z;
        public KeyCode build_rotate_right_key = KeyCode.C;

        [Header("Camera")]
        public KeyCode cam_rotate_left = KeyCode.Q;
        public KeyCode cam_rotate_right = KeyCode.E;
        public float edge_dist = 10f;

        [Header("Menu/UI")]
        public KeyCode menu_accept = KeyCode.Return;
        public KeyCode menu_cancel = KeyCode.Backspace;
        public KeyCode menu_pause = KeyCode.Escape;

        [Header("Floor")]
        public LayerMask floor_layer = (1 << 9);

        public UnityAction<Vector3> onClick;
        public UnityAction<Vector3> onRClick;
        public UnityAction<Vector3> onClickFloor;
        public UnityAction<Vector3> onRClickFloor;
        public UnityAction<Selectable, Vector3> onClickSelect;
        public UnityAction<Selectable, Vector3> onRClickSelect;
        public UnityAction onSelect;
        public UnityAction<Vector3> onRelease;

        public UnityAction onMenuAccept;
        public UnityAction onMenuCancel;

        private Vector2 move;
        private Vector2 edge_move;
        private Vector2 ui_move;
        private bool ui_moved;
        private float rotate_cam;
        private float rotate_build;
        private float mouse_scroll;
        private Vector2 scroll_start;
        private Vector2 drag_move; //middle button
        private bool mouse_hold_left = false;
        private bool mouse_hold_right = false;

        private bool press_action;
        private bool press_space;
        private bool hold_shift;
        private int press_number;

        private bool press_accept;
        private bool press_cancel;
        private bool press_pause;
        private bool press_ui_select;
        private bool press_ui_use;
        private bool press_ui_cancel;

        private SelectMode select_mode;
        private Buildable selected_buildable = null;
        private bool selecting = false;
        private Vector3 selection_start;
        private Vector3 selection_end;

        private static TheControls instance = null;

        void Awake()
        {
            instance = this;
        }

        void Update()
        {
            move = Vector3.zero;
            ui_move = Vector2.zero;
            rotate_cam = 0f;
            rotate_build = 0f;
            press_action = false;
            press_space = false;
            hold_shift = false;
            press_number = -1;

            press_accept = false;
            press_cancel = false;
            press_pause = false;

            Vector2 wasd = Vector2.zero;
            if (Input.GetKey(KeyCode.A))
                wasd += Vector2.left;
            if (Input.GetKey(KeyCode.D))
                wasd += Vector2.right;
            if (Input.GetKey(KeyCode.W))
                wasd += Vector2.up;
            if (Input.GetKey(KeyCode.S))
                wasd += Vector2.down;

            Vector2 arrows = Vector2.zero;
            if (Input.GetKey(KeyCode.LeftArrow))
                arrows += Vector2.left;
            if (Input.GetKey(KeyCode.RightArrow))
                arrows += Vector2.right;
            if (Input.GetKey(KeyCode.UpArrow))
                arrows += Vector2.up;
            if (Input.GetKey(KeyCode.DownArrow))
                arrows += Vector2.down;

            if (Input.GetKey(cam_rotate_left))
                rotate_cam += -1f;
            if (Input.GetKey(cam_rotate_right))
                rotate_cam += 1f;
            if (Input.GetKey(build_rotate_left_key))
                rotate_build += -1f;
            if (Input.GetKey(build_rotate_right_key))
                rotate_build += 1f;

            if (Input.GetKeyDown(action_key))
                press_action = true;
            if (Input.GetKeyDown(KeyCode.Space))
                press_space = true;

            if (Input.GetKeyDown(menu_accept))
                press_accept = true;
            if (Input.GetKeyDown(menu_cancel))
                press_cancel = true;
            if (Input.GetKeyDown(menu_pause))
                press_pause = true;

            move = arrows + wasd;
            move = move.normalized * Mathf.Min(move.magnitude, 1f);
            hold_shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            //Numbers
            for (int i = 1; i < 10; i++)
            {
                if (Input.GetKeyDown(i.ToString()))
                    press_number = i;
            }

            //Menu / UI
            if (!ui_moved && arrows.magnitude > 0.5f)
            {
                ui_move = arrows;
                ui_moved = true;
            }

            if (arrows.magnitude < 0.5f)
                ui_moved = false;

            if (press_accept)
                onMenuAccept?.Invoke();
            if (press_cancel)
                onMenuCancel?.Invoke();

            //Mouse hold
            bool in_gameplay = IsInGameplay();
            mouse_hold_left = Input.GetMouseButton(0) && in_gameplay;
            mouse_hold_right = Input.GetMouseButton(1) && in_gameplay;

            //Mouse scroll
            mouse_scroll = Input.mouseScrollDelta.y;

            //Scroll move
            drag_move = Vector2.zero;

            if (Input.GetMouseButtonDown(2) && in_gameplay)
                scroll_start = Input.mousePosition;
            if (Input.GetMouseButton(2) && in_gameplay)
            {
                drag_move = (Vector2)Input.mousePosition - scroll_start;
                scroll_start = Input.mousePosition;
            }

            //Mouse movement
            Vector3 mouse_pos = Input.mousePosition;
            Vector3 diff1 = mouse_pos - Vector3.zero;
            Vector3 diff2 = new Vector3(Screen.width, Screen.height) - mouse_pos;
            edge_move = Vector2.zero;
            edge_move.x += Mathf.Clamp01(1f - Mathf.Abs(diff2.x) / edge_dist) - Mathf.Clamp01(1f - Mathf.Abs(diff1.x) / edge_dist);
            edge_move.y += Mathf.Clamp01(1f - Mathf.Abs(diff2.y) / edge_dist) - Mathf.Clamp01(1f - Mathf.Abs(diff1.y) / edge_dist);

            //Selection
            bool colonist_selected = Selectable.IsColonistSelected();
            bool order_click = TheGame.IsMobile() && colonist_selected;

            //Click object
            if (Input.GetMouseButtonDown(0) && in_gameplay)
            {
                selecting = (select_mode == SelectMode.Select);
                selection_start = GetMouseWorldPos();

                if (!order_click && !IsShiftHold())
                    Selectable.UnselectAll();

                Vector3 pos = GetMouseWorldPos();
                onClick?.Invoke(pos);

                Selectable nearest = GetClickedSelect();
                if (nearest != null)
                    onClickSelect?.Invoke(nearest, pos);
                else if (IsMouseHitFloor())
                    onClickFloor?.Invoke(pos);
            }

            //Give order (right click or left click mobile with colonist selected)
            bool oclick = Input.GetMouseButtonDown(1) || (Input.GetMouseButtonDown(0) && order_click);
            if (oclick && in_gameplay)
            {
                Vector3 pos = GetMouseWorldPos();
                onRClick?.Invoke(pos);

                Selectable nearest = GetClickedSelect();
                if (nearest != null)
                    onRClickSelect?.Invoke(nearest, pos);
                else if (IsMouseHitFloor())
                    onRClickFloor?.Invoke(pos);

                selecting = false;
            }
            
            //Hold click for drag selection
            if (Input.GetMouseButton(0))
            {
                selection_end = GetMouseWorldPos();
            }

            //Release for drag selection
            if (Input.GetMouseButtonUp(0))
            {
                if (selecting)
                    SelectObjects();
                onRelease?.Invoke(GetMouseWorldPos());
            }
        }

        public void SetBuildMode(Buildable buildable)
        {
            if (select_mode != SelectMode.Build)
            {
                select_mode = SelectMode.Build;
                if (selected_buildable != null)
                    selected_buildable.CancelBuild();
                selected_buildable = buildable;
                Selectable.UnselectAll();
            }
        }

        public void SetSelectMode()
        {
            if (select_mode != SelectMode.Select)
            {
                select_mode = SelectMode.Select;
                if (selected_buildable != null)
                    selected_buildable.CancelBuild();
                selected_buildable = null;
                Selectable.UnselectAll();
            }
        }

        private void SelectObjects()
        {
            Vector3 cam_front = TheCamera.Get().GetFacingFront();
            Vector3 cam_right = TheCamera.Get().GetFacingRight();
            Vector3 dir = selection_end - selection_start;
            Vector3 front_vect = Vector3.Project(dir, cam_front);
            Vector3 side_vect = Vector3.Project(dir, cam_right);
            selecting = false;

            if (!IsShiftHold())
                Selectable.UnselectAll();

            if (IsSelectionBox())
            {
                Selectable.SelectAll(selection_start, front_vect, side_vect);
                onSelect?.Invoke();
            }
            else
            {
                Selectable nearest = GetClickedSelect();
                nearest?.Select();
                onSelect?.Invoke();
            }
        }

        public Selectable GetClickedSelect()
        {
            Selectable nearest = Selectable.GetNearestBySize(GetMouseWorldPos());
            Selectable raycast = Selectable.GetRaycast(GetMouseRay());
            if (nearest != null)
                return nearest;
            else if (raycast != null)
                return raycast;
            return null;
        }

        public bool IsShiftHold()
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        public Ray GetMouseRay()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return ray;
        }

        public Vector3 GetMouseWorldPos()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return PhysicsTool.RaycastFloorPos(ray.origin, ray.direction * 1000f, floor_layer);
        }

        public bool IsMouseHitFloor()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return PhysicsTool.RaycastFloorPos(ray.origin, ray.direction * 1000f, floor_layer, out Vector3 fpos);
        }

        public SelectMode GetSelectMode()
        {
            return select_mode;
        }

        public Vector2 GetMousePosition()
        {
            return Input.mousePosition;
        }

        public Vector2 GetMove() { return move; }
        public Vector2 GetUIMove() { return ui_move; }
        public Vector2 GetEdgeMove() { return edge_move; }
        public bool IsMoving() { return move.magnitude > 0.1f; }
        public float GetRotateCam() { return rotate_cam; }
        public float GetRotateBuild() { return rotate_build; }
        public float GetMouseScroll(){ return mouse_scroll; }
        public Vector2 GetMouseDrag(){ return drag_move; }
        public bool IsMouseHold() { return mouse_hold_left; }
        public bool IsMouseHoldRight() { return mouse_hold_right; }

        public bool IsPressAction() { return press_action; }
        public bool IsPressSpace() { return press_space; }
        public bool IsHoldShift() { return hold_shift; }
        public bool IsPressNumber(int nb) { return nb == press_number; }

        public bool IsPressMenuAccept() { return press_accept; }
        public bool IsPressMenuCancel() { return press_cancel; }
        public bool IsPressPause() { return press_pause; }
        public bool IsPressUISelect() { return press_ui_select; }
        public bool IsPressUIUse() { return press_ui_use; }
        public bool IsPressUICancel() { return press_ui_cancel; }

        public bool IsUIPressAny() { return ui_move.magnitude > 0.5f; }
        public bool IsUIPressLeft() { return ui_move.x < -0.5f; }
        public bool IsUIPressRight() { return ui_move.x > 0.5f; }
        public bool IsUIPressUp() { return ui_move.y > 0.5f; }
        public bool IsUIPressDown() { return ui_move.y < -0.5f; }

        public bool IsSelecting() { return selecting; }
        public bool IsSelectionBox() { return Mathf.Abs(selection_start.x - selection_end.x) > 1f && Mathf.Abs(selection_start.z - selection_end.z) > 1f; }
        public Vector3 GetSelectionStart() { return selection_start; }
        public Vector3 GetSelectionEnd() { return selection_end; }

        public bool IsMouseOverUI()
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
            return results.Count > 0;
        }

        public bool IsMouseInsideCamera()
        {
            return TheCamera.Get().IsInside(GetMousePosition());
        }

        public bool IsInGameplay()
        {
            return !IsMouseOverUI() && IsMouseInsideCamera();
        }

        public static TheControls Get()
        {
            return instance;
        }
    }

}