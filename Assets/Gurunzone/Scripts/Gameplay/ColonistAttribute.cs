using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    [RequireComponent(typeof(Colonist))]
    public class ColonistAttribute : MonoBehaviour
    {
        [Header("Attributes")]
        public AttributeData[] attributes;  //List of available attributes

        [Header("Levels")]
        public LevelData[] levels;  //List of available levels

        [Header("Auto Eat")]
        public bool auto_eat = true;        //Will the colonist try to eat automatically when hungry?
        public GroupData food_group;        //Food item group for the auto eat

        private Character character;
        private Colonist colonist;
        private Destructible destruct;

        private float move_speed_mult = 1f;
        private float gather_mult = 1f;
        private bool depleting = false;

        private void Awake()
        {
            character = GetComponent<Character>();
            colonist = GetComponent<Colonist>();
            destruct = GetComponent<Destructible>();
        }

        void Start()
        {
            //Init attributes
            foreach (AttributeData attr in attributes)
            {
                if (!CharacterData.HasAttribute(attr.type))
                    CharacterData.SetAttributeValue(attr.type, attr.start_value, attr.max_value);
            }

            //Init XP
            foreach (LevelData lvl in levels)
            {
                int start_level = colonist.data.GetStartingLevel(lvl.id);
                int start_xp = lvl.GetRequiredXP(start_level);
                if (!CharacterData.HasXP(lvl.id))
                    CharacterData.SetXP(lvl.id, start_xp);
            }

            //Events
            destruct.onDeath += OnDeath;
        }

        void Update()
        {
            if (TheGame.Get().IsPaused())
                return;

            if (character.IsDead())
                return;

            //Update attributes
            float game_speed = TheGame.Get().GetGameTimeSpeed();

            //Update Attributes
            foreach (AttributeData attr in attributes)
            {
                float update_value = attr.value_per_hour;
                update_value = update_value * game_speed * Time.deltaTime;
                CharacterData.AddAttributeValue(attr.type, update_value, attr.max_value);
            }

            //Penalty for depleted attributes
            move_speed_mult = 1f;
            gather_mult = 1f;
            depleting = false;

            foreach (AttributeData attr in attributes)
            {
                if (GetAttributeValue(attr.type) < 0.01f)
                {
                    move_speed_mult = move_speed_mult * attr.deplete_move_mult;
                    gather_mult = gather_mult * attr.deplete_gather_mult;
                    float update_value = attr.deplete_hp_loss * game_speed * Time.deltaTime;
                    AddAttribute(AttributeType.Health, update_value);
                    if (attr.deplete_hp_loss < 0f)
                        depleting = true;
                }
            }

            UpdateAutoActions();

            //Update hp
            if (destruct != null)
                destruct.hp = Mathf.RoundToInt(GetAttributeValue(AttributeType.Health));

            //Dying
            float health = GetAttributeValue(AttributeType.Health);
            if (health < 0.01f)
                character.Kill();

        }

        private void UpdateAutoActions()
        {
            if (colonist.Character.IsWaiting())
                return;

            //Auto Eat
            if (auto_eat && HasAttribute(AttributeType.Hunger) && !Character.IsSleeping())
            {
                if (IsLow(AttributeType.Hunger))
                {
                    Inventory global = Inventory.GetGlobal();
                    colonist.UseItem(global, food_group, AttributeType.Hunger);
                }
            }
        }

        private void OnDeath()
        {
            CharacterData.SetAttributeValue(AttributeType.Health, 0f, 0f);
        }

        public void AddAttribute(AttributeType type, float value)
        {
            if (HasAttribute(type) && !character.IsDead())
            {
                CharacterData.AddAttributeValue(type, value, GetAttributeMax(type));
            }
        }

        public void SetAttribute(AttributeType type, float value)
        {
            if (HasAttribute(type) && !character.IsDead())
            {
                CharacterData.SetAttributeValue(type, value, GetAttributeMax(type));
            }
        }

        public bool HasAttribute(AttributeType type)
        {
            return CharacterData.HasAttribute(type) && GetAttribute(type) != null;
        }

        public float GetAttributeValue(AttributeType type)
        {
            return CharacterData.GetAttributeValue(type);
        }

        public float GetAttributeMax(AttributeType type)
        {
            AttributeData adata = GetAttribute(type);
            if (adata != null)
                return adata.max_value;
            return 100f;
        }

        public AttributeData GetAttribute(AttributeType type)
        {
            foreach (AttributeData attr in attributes)
            {
                if (attr.type == type)
                    return attr;
            }
            return null;
        }

        public bool IsLow(AttributeType type)
        {
            AttributeData attr = GetAttribute(type);
            float val = GetAttributeValue(type);
            return (val <= attr.low_threshold);
        }

        public bool IsDepleted(AttributeType type)
        {
            float val = GetAttributeValue(type);
            return (val <= 0f);
        }

        public bool IsAnyDepleted()
        {
            foreach (AttributeData attr in attributes)
            {
                float val = GetAttributeValue(attr.type);
                if (val <= 0f)
                    return true;
            }
            return false;
        }

        public bool HasXP(string id)
        {
            return CharacterData.HasXP(id);
        }

        public void SetXP(string id, float xp)
        {
            if(HasXP(id))
                CharacterData.SetXP(id, xp);
        }

        public void AddXP(string id, float xp)
        {
            if (HasXP(id))
                CharacterData.AddXP(id, xp);
        }

        public int GetXP(string id)
        {
            return CharacterData.GetXPI(id);
        }

        public int GetLevel(string id)
        {
            return LevelData.GetLevel(id, GetXP(id));
        }

        //Add XP based on the level bonus
        public void AddXP(BonusType type, float xp, Interactable target = null, CraftData itarget = null)
        {
            foreach (LevelData level in levels)
            {
                if (level.bonus == type)
                {
                    bool is_any = target == null && itarget == null;
                    bool is_valid_select = target != null && target.Selectable.HasGroup(level.bonus_target);
                    bool is_valid_item = itarget != null && itarget.HasGroup(level.bonus_target);

                    if (is_any || is_valid_select || is_valid_item)
                    {
                        AddXP(level.id, xp);
                    }
                }
            }
        }

        public float GetLevelBonus(BonusType type, Interactable target = null, CraftData itarget = null)
        {
            foreach (LevelData level in levels)
            {
                if (level.bonus == type)
                {
                    bool is_any = target == null && itarget == null;
                    bool is_valid_select = target != null && target.Selectable.HasGroup(level.bonus_target);
                    bool is_valid_item = itarget != null && itarget.HasGroup(level.bonus_target);

                    if (is_any || is_valid_select || is_valid_item)
                    {
                        int xp = CharacterData.GetXPI(level.id);
                        int lvl = level.GetLevel(xp);
                        float value = level.GetBonusValue(lvl);
                        return value;
                    }
                }
            }
            return 0f;
        }

        public float GetSpeedMult()
        {
            return Mathf.Max(move_speed_mult, 0.01f);
        }

        public float GetGatherMult()
        {
            return Mathf.Max(gather_mult, 0.01f);
        }

        public bool IsDepletingHP()
        {
            return depleting;
        }

        public string GetStatusText()
        {
            List<string> tlist = new List<string>();
            foreach (AttributeData attr in attributes)
            {
                if (IsLow(attr.type))
                    tlist.Add(attr.low_status);
            }
            return string.Join(", ", tlist.ToArray());
        }

        public SaveColonistData CharacterData
        {
            get { return colonist.SData; }
        }

        public Character Character { get { return character; } }
        public Colonist Colonist { get { return colonist; } }
    }
}
