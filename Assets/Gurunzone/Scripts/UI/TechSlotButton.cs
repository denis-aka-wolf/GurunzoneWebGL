using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gurunzone
{
    [RequireComponent(typeof(TechSlot))]
    public class TechSlotButton : MonoBehaviour
    {
        public TechData tech;

        public UnityAction<TechSlot> onClick;

        private TechSlot slot;

        void Awake()
        {
            slot = GetComponent<TechSlot>();
            slot.onClick += OnClick;
        }

        private void Start()
        {
            if (tech != null)
            {
                slot.SetSlot(tech);
            }
        }

        void Update()
        {
            if (tech != null)
            {
                slot.HideProgress();
                slot.SetCompleted(TechManager.Get().IsTechCompleted(tech));
                slot.SetLocked(!TechManager.Get().HasTechRequirements(tech) && !TechManager.Get().IsTechStarted(tech));
                if (TechManager.Get().IsTechResearch(tech))
                {
                    slot.SetProgress(TechManager.Get().GetTechProgress(tech) / tech.craft_duration);
                }
            }
        }

        private void OnClick(UISlot slot)
        {
            if (tech != null)
            {
                onClick?.Invoke(this.slot);
            }
        }

        public TechSlot GetSlot()
        {
            return slot;
        }
    }
}
