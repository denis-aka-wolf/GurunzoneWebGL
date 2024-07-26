using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gurunzone
{

    public class InfoPanel : SelectPanel
    {
        public Text title;
        public Text subtitle;

        private static InfoPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
            title.text = "";
            subtitle.text = "";
        }

        private void RefreshPanel()
        {

            CSObject csobject = select.GetComponent<CSObject>();
            if (csobject != null)
            {
                CSData csdata = csobject.GetData();
                if (csdata != null)
                {
                    if (csdata is CraftData)
                    {
                        CraftData cdata = (CraftData)csdata;
                        title.text = cdata.title;

                    }
                    if (csdata is SpawnData)
                    {
                        SpawnData sdata = (SpawnData)csdata;
                        title.text = sdata.title;
                    }

                }
            }
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            RefreshPanel();
        }

        public override bool IsShowable(Selectable select)
        {
            CSObject obj = select.GetComponent<CSObject>();
            return obj != null;
        }

        public override int GetPriority()
        {
            return 0;
        }

        public static InfoPanel Get()
        {
            return instance;
        }
    }
}
