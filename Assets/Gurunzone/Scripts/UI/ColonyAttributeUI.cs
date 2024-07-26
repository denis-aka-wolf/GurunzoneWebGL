using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{

    public enum ColonyAttributeType
    {
        GlobalAttribute = 0,    //Will show independant attribute update by script in ColonyManager.cs
        ColonistAverage = 10,   //Will show the average of the attribute of all colonists
    }

    [RequireComponent(typeof(AttributeUI))]
    public class ColonyAttributeUI : MonoBehaviour
    {
        public ColonyAttributeType type;
        public AttributeType attribute;

        private AttributeUI attribute_ui;
        private float timer = 99f;
        private float refresh_rate = 0.5f;

        void Awake()
        {
            attribute_ui = GetComponent<AttributeUI>();
        }

        private void Start()
        {
            UpdateUI();
        }

        void Update()
        {
            timer += Time.deltaTime;
            if (timer > refresh_rate)
            {
                timer = 0f;
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            if (type == ColonyAttributeType.ColonistAverage)
                UpdateAverage();
            else if (type == ColonyAttributeType.GlobalAttribute)
                UpdateGlobal();
        }

        private void UpdateGlobal()
        {
            ColonyManager colony = ColonyManager.Get();
            float val = colony.GetAttributeValue(attribute);
            float mval = colony.GetAttributeMax(attribute);
            attribute_ui.SetAttribute(attribute, val, mval);
            attribute_ui.SetLow(colony.IsLow(attribute));
        }

        private void UpdateAverage()
        {
            AttributeData idata = AttributeData.Get(attribute);
            if (idata != null)
            {
                float average = ColonistManager.Get().GetAverageAttribute(attribute);
                attribute_ui.SetAttribute(attribute, average, idata.max_value);
                attribute_ui.SetLow(average <= idata.low_threshold);
            }
        }
    }
}