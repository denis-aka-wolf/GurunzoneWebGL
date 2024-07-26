using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Gurunzone
{
    public class TabFilter : MonoBehaviour
    {
        public string tab_group;
        public GroupData filter_group;
        public GameObject highlight;
        public bool select_at_start = false;

        private Button button;
        private bool selected = false;

        public UnityAction<TabFilter> onClick;

        private static List<TabFilter> button_list = new List<TabFilter>();

        void Awake()
        {
            button_list.Add(this);
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            button_list.Remove(this);
        }

        void Start()
        {
            if (select_at_start)
                Select();
        }

        void Update()
        {
            if (highlight != null && highlight.activeSelf != selected)
                highlight.SetActive(selected);
        }

        public void Select()
        {
            selected = true;
        }

        public void Unselect()
        {
            selected = false;
        }

        void OnClick()
        {
            UnselectAll(tab_group);
            Select();
            onClick?.Invoke(this);
        }

        public static GroupData GetGroupSeleted(string tgroup)
        {
            foreach (TabFilter tab in button_list)
            {
                if (tab.tab_group == tgroup)
                {
                    if (tab.selected)
                        return tab.filter_group;
                }
            }
            return null;
        }

        public static void Select(string tgroup, GroupData group)
        {
            UnselectAll(tgroup);
            foreach (TabFilter tab in button_list)
            {
                if (tab.tab_group == tgroup && tab.filter_group == group)
                    tab.Select();
            }
        }

        public static void UnselectAll(string tgroup)
        {
            foreach (TabFilter tab in button_list)
            {
                if(tab.tab_group == tgroup)
                    tab.Unselect();
            }
        }

        public static List<TabFilter> GetAll(string tgroup)
        {
            List<TabFilter> group_list = new List<TabFilter>();
            foreach (TabFilter tab in button_list)
            {
                if (tab.tab_group == tgroup)
                    group_list.Add(tab);
            }
            return group_list;
        }
    }
}
