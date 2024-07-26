using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gurunzone.Events
{
    public class DialogueMessage : MonoBehaviour
    {
        public string title;
        public Sprite icon;

        [TextArea(5, 10)]
        public string text;

        public AudioClip audio_clip = null;

        [Tooltip("For in-game dialogues: time of the pause between this dialogue and the next one")]
        public float pause = 0f;

        public UnityAction onStart;
        public UnityAction onEnd;

        public string GetTitle()
        {
            string txt = title;

            //If you integrate a translation system, convert your string here!
            txt = EventTool.Translate(txt);

            //Replace codes like [s:variable_id]
            txt = EventTool.ReplaceCodes(txt);

            return txt;
        }

        //Get the text for display
        public string GetText()
        {
            string txt = text;

            //If you integrate a translation system, convert your string here!
            txt = EventTool.Translate(txt);

            //Replace codes like [s:variable_id]
            txt = EventTool.ReplaceCodes(txt);

            return txt;
        }
    }

}