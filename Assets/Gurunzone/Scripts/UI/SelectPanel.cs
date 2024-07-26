using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Gurunzone
{
    /// <summary>
    /// UI panel that appears when selecting a selectable
    /// </summary>

    public class SelectPanel : UIPanel
    {
        protected Selectable select;

        private static List<SelectPanel> panels = new List<SelectPanel>();

        protected override void Awake()
        {
            base.Awake();
            panels.Add(this);
        }

        private void OnDestroy()
        {
            panels.Remove(this);
        }


        protected override void Update()
        {
            base.Update();

            if (IsVisible())
            {
                if (select == null)
                    Hide();

                if (select != null && !select.IsSelected())
                    Hide();
            }
        }

        public void ShowSelectable(Selectable select)
        {
            this.select = select;
            Show();
        }

        public void OnClickDestroy()
        {
            DestroyPanel.Get().ShowDestroy(select);
        }

        public void OnClickQuickDestroy()
        {
            if(select != null)
                select.Kill();
        }

        public virtual int GetPriority()
        {
            return 0;  //If a selectable has multiple valid SelectPanel, highest priority will be shown only (need to be overriden)
        }

        public virtual bool IsShowable(Selectable select)
        {
            return true; //Determine the conditions for this panel to be shown (need to be overriden)
        }

        public static List<SelectPanel> GetAll()
        {
            return panels;
        }

        public static void Show(Selectable select)
        {
            int priority = -999;
            SelectPanel best = null;
            foreach (SelectPanel panel in panels)
            {
                if (panel.IsShowable(select) && panel.GetPriority() > priority)
                {
                    priority = panel.GetPriority();
                    best = panel;
                }
            }

            if (best != null)
                best.ShowSelectable(select);
        }

        public static void HideAll()
        {
            foreach (SelectPanel panel in panels)
                panel.Hide();
        }
    }

}
