using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Gurunzone
{

    public class IntSelector : MonoBehaviour
    {
        public int value = 0;
        public int value_min = 0;
        public int value_max = 99;

        public Text value_text;

        public UnityAction<int> onChange;

        void Start()
        {
            Refresh();
        }

        public void Refresh()
        {
            value_text.text = value.ToString();
        }

        public void SetValue(int value)
        {
            this.value = value;
            onChange?.Invoke(value);
            Refresh();
        }

        public void OnClickLeft()
        {
            value = value - 1;
            value = Mathf.Clamp(value, value_min, value_max);
            onChange?.Invoke(value);
            Refresh();
        }

        public void OnClickRight()
        {
            value = value + 1;
            value = Mathf.Clamp(value, value_min, value_max);
            onChange?.Invoke(value);
            Refresh();
        }

    }
}
