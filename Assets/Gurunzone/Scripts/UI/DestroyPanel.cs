using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{

    public class DestroyPanel : UIPanel
    {
        private Selectable target;
        private Construction ctarget;

        private static DestroyPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }

        protected override void Update()
        {
            base.Update();
        }

        public void ShowDestroy(Selectable select)
        {
            if (select != null)
            {
                target = select;
                ctarget = null;
                Show();
            }
        }

        public void ShowDestroy(Construction construct)
        {
            if (construct != null)
            {
                target = null;
                ctarget = construct;
                Show();
            }
        }

        public void ConfirmDestroy()
        {
            if (target != null)
                target.Kill();
            if (ctarget != null)
                ctarget.CancelConstruct();

            Hide();
        }

        public static DestroyPanel Get()
        {
            return instance;
        }
    }
}
