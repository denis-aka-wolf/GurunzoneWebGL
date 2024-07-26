using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gurunzone
{

    public class ColonistPanel : SelectPanel
    {
        [Header("Colonist")]
        public Text title;
        public Text subtitle;
        public Text status_text;
        public AttributeUI[] attributes;

        private Colonist colonist;

        private static ColonistPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;

            foreach (ActionButton button in GetComponentsInChildren<ActionButton>())
                button.onClick += OnClickAction;
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
        {
            base.Update();

            if (colonist != null)
            {
                status_text.text = colonist.Attributes.GetStatusText();

                foreach (AttributeUI attr in attributes)
                    attr.Hide();

                int index = 0;
                foreach (AttributeData attr in colonist.Attributes.attributes)
                {
                    if (index < attributes.Length)
                    {
                        AttributeUI attr_ui = attributes[index];
                        float value = colonist.Attributes.GetAttributeValue(attr.type);
                        float max = colonist.Attributes.GetAttributeMax(attr.type);
                        attr_ui.SetAttribute(attr.type, value, max);
                        attr_ui.SetLow(value <= attr.low_threshold);
                        index++;
                    }
                }
            }
        }

        public void OnClickAction(ActionBasic action)
        {
            if (colonist != null && !colonist.IsDead() && action.CanDoAction(colonist.Character,null))
                colonist.OrderInterupt(action, null);
        }

        public void OnClickBag()
        {
            if (BagPanel.Get().IsVisible())
                BagPanel.Get().Hide();
            else
                BagPanel.Get().ShowColonist(colonist);
        }

        public void OnClickStats()
        {
            if (LevelsPanel.Get().IsVisible())
                LevelsPanel.Get().Hide();
            else
                LevelsPanel.Get().ShowColonist(colonist);
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            Colonist colonist = select.GetComponent<Colonist>();
            this.colonist = colonist;
            title.text = colonist.GetName();
            subtitle.text = colonist.data.title;
        }

        public override void Hide(bool instant = false)
        {
            base.Hide(instant);
            BagPanel.Get()?.Hide();
            LevelsPanel.Get()?.Hide();
        }

        public override bool IsShowable(Selectable select)
        {
            Colonist colonist = select.GetComponent<Colonist>();
            return colonist != null;
        }
        
        public override int GetPriority()
        {
            return 50;
        }

        public static ColonistPanel Get()
        {
            return instance;
        }
    }
}
