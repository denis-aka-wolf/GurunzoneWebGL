using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gurunzone
{
    public class ItemIcon : MonoBehaviour
    {
        public Image icon;
        public Text title;
        public Text quantity;

        private ItemData item;

        void Start()
        {

        }

        public void SetItem(ItemData item, int quantity = 0)
        {
            this.item = item;
            icon.sprite = item.icon;

            if(title != null)
                title.text = item.title;

            if (this.quantity != null)
            {
                this.quantity.text = quantity.ToString();
                this.quantity.enabled = quantity > 0;
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public ItemData GetItem()
        {
            return item;
        }

    }
}
