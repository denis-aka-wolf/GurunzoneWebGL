using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{

    public class MainBar : UIPanel
    {
        public GameObject unselect_btn;

        private static MainBar instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }

        protected override void Update()
        {
            base.Update();

            bool visible = Selectable.GetSelectionCount() > 0 || TheControls.Get().GetSelectMode() == SelectMode.Build;
            if (unselect_btn != null && visible != unselect_btn.activeSelf)
                unselect_btn.SetActive(visible);
        }

        public void OnClickZone()
        {
            HidePanels();
            Zone.CreateBuildMode(ZoneType.Harvest);
        }

        public void OnClickBuild()
        {
            HidePanels();
            BuildPanel.Get().Show();
        }

        public void OnClickWork()
        {
            HidePanels();
            WorkPanel.Get().Show();
        }

        public void OnClickResource()
        {
            HidePanels();
            EconomyPanel.Get().Show();
        }

        public void OnClickTech()
        {
            HidePanels();
            TechPanel.Get().Toggle();
        }

        public void OnClickUnselect()
        {
            Selectable.UnselectAll();
            TheControls.Get().SetSelectMode();
            SelectPanel.HideAll();
        }

        private void HidePanels()
        {
            TheControls.Get().SetSelectMode();
            BuildPanel.Get().Hide();
            BuildInfoPanel.Get().Hide();
            UpgradePanel.Get().Hide();
            UpgradeInfoPanel.Get().Hide();
            TechPanel.Get().Hide();
            EconomyPanel.Get().Hide();
            WorkPanel.Get().Hide();
        }

        public static MainBar Get()
        {
            return instance;
        }
    }
}
