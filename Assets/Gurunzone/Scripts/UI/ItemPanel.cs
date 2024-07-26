using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gurunzone
{

    public class ItemPanel : SelectPanel
    {
        [Header("Resource")]
        public Text title;
        public Text quantity;
        public Image img;

        private Item item;

        private static ItemPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
        {
            base.Update();

            if (item == null)
            {
                Hide();
                return;
            }

            quantity.text = item.quantity.ToString();
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            Item item = select.GetComponent<Item>();
            this.item = item;
            title.text = item.data.title;
            img.sprite = item.data.icon;
        }

        public override bool IsShowable(Selectable select)
        {
            Item item = select.GetComponent<Item>();
            return item != null;
        }

        public static ItemPanel Get()
        {
            return instance;
        }
    }

}
