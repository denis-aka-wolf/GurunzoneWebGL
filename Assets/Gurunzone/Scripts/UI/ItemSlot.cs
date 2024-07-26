using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gurunzone
{

    public class ItemSlot : UISlot
    {
        public Text title;
        public Image icon;
        public Text quantity;

        private ItemData item;


        public void SetSlot(ItemData item, int quant)
        {
            this.item = item;
            icon.sprite = item.icon;
            title.text = item.title;
            quantity.text = quant.ToString();
            quantity.enabled = quant > 0;
            Show();
        }

        public ItemData GetItem()
        {
            return item;
        }
    }

}
