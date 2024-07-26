using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Manages tech bonus and tech research
    /// </summary>

    public class TechManager : MonoBehaviour
    {

        private static TechManager instance;

        void Awake()
        {
            instance = this;
        }

        void Update()
        {
            if (TheGame.Get().IsPaused())
                return;

            List<TechData> active_techs = new List<TechData>();
            foreach (KeyValuePair<string, float> pair in SaveData.Get().tech_progress)
            {
                TechData itech = TechData.Get(pair.Key);
                active_techs.Add(itech);
            }

            foreach (TechData tech in active_techs)
            {
                AddTechProgress(tech);
            }
        }

        public void StartTechResearch(TechData tech)
        {
            if (!IsTechCompleted(tech) && !SaveData.Get().HasTechProgress(tech.id))
            {
                SaveData.Get().SetTechProgress(tech.id, 0f);
            }
        }

        public void CancelTechResearch(TechData tech)
        {
            if (!IsTechCompleted(tech))
            {
                SaveData.Get().RemoveTechProgress(tech.id);
            }
        }

        public void AddTechProgress(TechData tech, float speed = 1f)
        {
            float gspeed = TheGame.Get().GetGameTimeSpeed();
            float progress = gspeed * speed * Time.deltaTime;
            SaveData.Get().AddTechProgress(tech.id, progress);

            float nprogress = SaveData.Get().GetTechProgress(tech.id);
            if (nprogress >= tech.craft_duration)
            {
                CompleteTech(tech);
            }
        }

        public float GetTechProgress(TechData tech)
        {
            return SaveData.Get().GetTechProgress(tech.id);
        }

        public float GetTechProgressPercent(TechData tech)
        {
            if(tech.craft_duration > 0.0001f)
                return SaveData.Get().GetTechProgress(tech.id) / tech.craft_duration;
            return 0f;
        }

        public bool IsTechResearch(TechData tech)
        {
            return SaveData.Get().HasTechProgress(tech.id);
        }

        public void CompleteTech(TechData tech)
        {
            if (!IsTechCompleted(tech))
            {
                SaveData.Get().RemoveTechProgress(tech.id);
                SaveData.Get().CompleteTech(tech.id);
            }
        }

        public bool IsTechCompleted(TechData tech)
        {
            return SaveData.Get().IsTechCompleted(tech.id);
        }

        public bool IsTechStarted(TechData tech)
        {
            return IsTechCompleted(tech) || IsTechResearch(tech);
        }

        public int GetTechBonusInt(BonusType type, Selectable receiver = null, Interactable target = null, CraftData itarget = null)
        {
            return Mathf.RoundToInt(GetTechBonus(type, receiver, target, itarget));
        }

        public float GetTechBonus(BonusType type, Selectable receiver = null, Interactable target = null, CraftData itarget = null)
        {
            float bonus = 0f;
            foreach (KeyValuePair<string, int> pair in SaveData.Get().tech_completed)
            {
                TechData itech = TechData.Get(pair.Key);
                if (itech && itech.effect == type)
                {
                    bool is_any = target == null && itarget == null;
                    bool can_target = is_any || itech.CanTarget(target.Selectable) || itech.CanTarget(itarget);
                    bool can_receive = receiver == null || itech.CanReceive(receiver);
                    if (can_receive && can_target)
                        bonus += itech.value;
                }
            }
            return bonus;
        }

        public void PayTechCost(TechData tech)
        {
            Inventory inventory = Inventory.GetGlobal();
            inventory.PayCraftCost(tech);
        }

        public bool CanBeResearched(TechData tech)
        {
            return HasTechRequirements(tech) && HasTechCost(tech);
        }

        public bool HasTechRequirements(TechData tech)
        {
            return tech.HasRequirements();
        }

        public bool HasTechCost(TechData tech)
        {
            Inventory inventory = Inventory.GetGlobal();
            return inventory.HasCraftCost(tech);
        }

        public static TechManager Get()
        {
            return instance;
        }
    }
}
