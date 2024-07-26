using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gurunzone.Events
{

    public class DialogueChoiceButton : MonoBehaviour
    {
        public Text text;
        public Image highlight;

        private Button button;
        private RectTransform rect;
        private int index;
        private DialogueChoice choice;

        void Awake()
        {
            rect = GetComponent<RectTransform>();
            button = GetComponent<Button>();
        }

        public void ShowButton(int index, DialogueChoice choice)
        {
            this.index = index;
            this.choice = choice;
            text.text = choice.GetText();

            button.interactable = CanDoChoice();
            gameObject.SetActive(true);

            if (highlight != null)
                highlight.enabled = false;
        }

        public void SetHighlight(bool visible)
        {
            if (highlight != null)
                highlight.enabled = visible;
        }

        public void HideButton()
        {
            gameObject.SetActive(false);
            if (highlight != null)
                highlight.enabled = false;
        }

        public void OnClick()
        {
            EventManager.Get().SelectChoice(index);
        }

        public bool CanDoChoice()
        {
            if (choice != null && choice.go_to != null)
            {
                if (choice.go_to.AreConditionsMet())
                    return true;
            }
            return false;
        }

        public RectTransform GetRect()
        {
            return rect;
        }
    }

}
