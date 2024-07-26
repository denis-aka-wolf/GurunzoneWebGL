using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gurunzone
{

    public class AttributeUI : MonoBehaviour
    {
        public Text title;
        public Text value_text;
        public ProgressBar progress;

        public Sprite high_sprite;
        public Sprite low_sprite;

        private AttributeType type;
        private LevelData level;
        private bool active = false;

        private void Update()
        {
            if (!active)
                gameObject.SetActive(false);
        }

        public void SetAttribute(AttributeType type, float value, float value_max)
        {
            AttributeData attribute = AttributeData.Get(type);
            if (attribute != null)
            {
                this.type = type;
                level = null;
                title.text = attribute.title;
                value_text.text = Mathf.RoundToInt(value) + " / " + value_max;
                progress.value = value;
                progress.value_max = value_max;
                active = true;
                gameObject.SetActive(true);
            }
        }

        public void SetLevel(LevelData ldata, int value, int value_max)
        {
            if (ldata != null)
            {
                type = AttributeType.None;
                level = ldata;
                title.text = ldata.title;
                value_text.text = value.ToString();
                progress.value = value;
                progress.value_max = value_max;
                active = true;
                gameObject.SetActive(true);
            }
        }

        public void SetLow(bool low)
        {
            progress.fill.sprite = low ? low_sprite : high_sprite;
        }

        public void Hide()
        {
            type = AttributeType.None;
            level = null;
            active = false;
        }
    }
}
