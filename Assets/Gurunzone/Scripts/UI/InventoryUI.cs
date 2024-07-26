using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gurunzone
{
    public class InventoryUI : MonoBehaviour
    {
        public bool global;
        public string inventory_uid;
        public GridLayoutGroup grid;
        public GameObject icon_template;

        private List<CraftSlot> icon_list = new List<CraftSlot>();

        void Start()
        {
            icon_template.SetActive(false);
        }

        void Update()
        {
            Inventory inventory;
            if (global)
                inventory = Inventory.GetGlobal();
            else
                inventory = Inventory.Get(inventory_uid);

            if (inventory != null)
            {
                //Add icons
                while (inventory.GetItems().Count > icon_list.Count)
                {
                    GameObject nicon = Instantiate(icon_template, grid.transform);
                    icon_list.Add(nicon.GetComponent<CraftSlot>());
                }

                foreach (CraftSlot icon in icon_list)
                    icon.Hide();

                int index = 0;
                foreach (ItemSet item in inventory.GetItems())
                {
                    ItemData idata = item.item;
                    if (idata && index < icon_list.Count)
                    {
                        CraftSlot icon = icon_list[index];
                        icon.SetSlot(idata, item.quantity);
                        index++;
                    }
                }
            }
        }
    }
}
