using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gurunzone.Events
{
    public class DialoguePanel : UIPanel
    {
        [Header("Ref")]
        public Image portrait;
        public Text title;
        public Text text;

        [Header("Type FX")]
        public bool type_fx = true;
        public float type_fx_speed = 30f;

        [Header("Choices")]
        public Button ok_button;
        public DialogueChoiceButton[] choices;

        private EventLine current_line;
        private string current_text = "";
        private bool text_skipable;
        private bool event_skipable;
        
        private bool should_hide = false;
        private float hide_timer = 0f;

        private Coroutine text_anim;
        private bool text_anim_completed = true;

        private static DialoguePanel _instance;

        protected override void Awake()
        {
            base.Awake();
            _instance = this;

        }

        protected override void Start()
        {
            base.Start();
            Hide();

            if (EventManager.Get())
            {
                EventManager.Get().onDialogueMessageStart += OnStart;
                EventManager.Get().onDialogueMessageEnd += OnEnd;
            }
        }

        protected override void Update()
        {
            base.Update();

            hide_timer += Time.deltaTime;
            if (IsVisible() && should_hide && hide_timer > 0.2f)
                Hide();
        }

        public void SetDialogue(EventLine line, DialogueMessage msg)
        {
            current_line = line;
            current_text = msg.GetText();
            this.text.text = current_text;

            if (title != null)
                title.text = msg.GetTitle();

            if (portrait != null)
                portrait.color = Color.white; //Revert from animation
            if (portrait != null)
                portrait.sprite = msg.icon;
            if (portrait != null)
                portrait.enabled = msg.icon != null;

            text_skipable = true;
            event_skipable = false;
            text_anim_completed = true;
            should_hide = false;

            if (type_fx && type_fx_speed > 1f)
            {
                text.text = "";
                gameObject.SetActive(true); //Allow starting coroutine
                text_anim_completed = false;
                text_anim = StartCoroutine(AnimateText());
            }

            foreach (DialogueChoiceButton button in choices)
                button.HideButton();

            ok_button.gameObject.SetActive(!HasChoices());

            if (HasChoices() && choices.Length > 0)
            {
                text_skipable = false;
                for (int i = 0; i < line.choices.Count; i++)
                {
                    if (i < choices.Length)
                    {
                        DialogueChoiceButton button = choices[i];
                        DialogueChoice choice = line.choices[i];
                        button.ShowButton(i, choice);
                    }
                }
            }
        }

        public void SkipTextAnim()
        {
            this.text.text = current_text;
            text_anim_completed = true;
            if(text_anim != null)
                StopCoroutine(text_anim);
        }

        public bool IsTextAnimCompleted()
        {
            return text_anim_completed;
        }

        IEnumerator AnimateText()
        {
            for (int i = 0; i < (current_text.Length + 1); i++)
            {
                this.text.text = current_text.Substring(0, i);
                yield return new WaitForSeconds(1f/type_fx_speed);
            }
            text_anim_completed = true;
        }

        public void OnClickOk()
        {
            if (IsVisible() && !HasChoices())
            {
                if (IsTextAnimCompleted())
                    EventManager.Get().StopDialogue();
                else if(text_skipable)
                    SkipTextAnim();
            }
        }

        public void OnPressCancel()
        {
            if (IsVisible() && event_skipable)
            {
                EventManager.Get().StopEvent();
                Hide();
            }
        }

        public bool HasChoices() {
            return current_line != null && current_line.choices.Count > 0;
        }

        private void OnStart(EventLine line, DialogueMessage msg)
        {
            SetDialogue(line, msg);
            Show();

            TheAudio.Get().PlaySFX("dialogue", msg.audio_clip);
        }

        private void OnEnd(EventLine line, DialogueMessage msg)
        {
            if (IsVisible())
            {
                if (text_anim != null)
                    StopCoroutine(text_anim);
                text.text = current_text;
                should_hide = true;
                hide_timer = 0f;
            }
        }

        public static DialoguePanel Get()
        {
            return _instance;
        }
    }

}