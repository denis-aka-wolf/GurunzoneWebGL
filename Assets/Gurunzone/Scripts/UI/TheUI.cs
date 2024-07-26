using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Gurunzone
{
    /// <summary>
    /// Main UI manager
    /// </summary>

    public class TheUI : MonoBehaviour
    {
        public Animator warning;
        public Text warning_text;
        public Text build_text;

        private Canvas canvas;
        private RectTransform rect;

        private static TheUI instance;

        void Awake()
        {
            instance = this;
            canvas = GetComponent<Canvas>();
            rect = GetComponent<RectTransform>();
            canvas.worldCamera = Camera.main;
            if(warning_text != null)
                warning_text.text = "";
            if (build_text != null)
                build_text.enabled = false;
            Selectable.onSelectAny += OnSelect;
        }

        private void OnDestroy()
        {
            Selectable.onSelectAny -= OnSelect;
        }

        private void Start()
        {
            MainBar.Get().Show(true);

            //Black panel transition
            if (!BlackPanel.Get().IsVisible())
            {
                BlackPanel.Get().Show(true);
                BlackPanel.Get().Hide();
            }
        }

        private void Update()
        {
            if (TheControls.Get().IsPressPause())
                PauseMenu.Get().Toggle();

            bool show_build = TheGame.IsMobile() && TheControls.Get().GetSelectMode() == SelectMode.Build;
            if(build_text != null && show_build != build_text.enabled)
                build_text.enabled = show_build;
        }

        private void OnSelect()
        {
            SelectPanel.HideAll();

            List<Selectable> selection = Selectable.GetAllSelected();
            if (selection.Count == 1)
            {
                Selectable aselect = selection[0];
                SelectPanel.Show(aselect);
            }
        }

        public void ShowWarning(string text)
        {
            if(warning_text != null)
                warning_text.text = text;
            if(warning != null)
                warning.Rebind();
        }

        public void OnClickPause()
        {
            PauseMenu.Get().Show();
        }

        public Vector2 GetCanvasSize()
        {
            return rect.sizeDelta;
        }

        public Vector2 ScreenPointToCanvasPos(Vector2 pos)
        {
            Vector2 localpoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, pos, canvas.worldCamera, out localpoint);
            return localpoint;
        }

        public Vector2 ScreenPointToCanvasPos(Vector2 pos, RectTransform localRect)
        {
            Vector2 localpoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(localRect, pos, canvas.worldCamera, out localpoint);
            return localpoint;
        }

        public Vector2 WorldToCanvasPos(Vector3 world)
        {
            Vector2 screen_pos = Camera.main.WorldToScreenPoint(world);
            return ScreenPointToCanvasPos(screen_pos);
        }

        public bool IsPanelOpened()
        {
            return TechPanel.Get().IsVisible() || PauseMenu.Get().IsVisible();
        }

        public static TheUI Get()
        {
            return instance;
        }
    }
}
