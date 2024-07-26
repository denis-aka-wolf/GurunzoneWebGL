using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Gurunzone
{
    /// <summary>
    /// Updated version of Destructible, for Colonists.
    /// Allow to link the HP to the Health attribute if the character is a colonist
    /// </summary>

    [RequireComponent(typeof(Colonist))]
    public class ColonistDestructible : Destructible
    {
        private Character character;
        private Colonist colonist;

        protected override void Awake()
        {
            base.Awake();
            character = GetComponent<Character>();
            colonist = GetComponent<Colonist>();
        }

        protected override void Update()
        {
            base.Update();


        }

        public override int HP
        {
            get {
                if (colonist.Attributes != null)
                    return (int) colonist.Attributes.GetAttributeValue(AttributeType.Health);
                return hp;
            }
            set
            {
                if (colonist.Attributes != null)
                    colonist.Attributes.SetAttribute(AttributeType.Health, value);
                hp = value;
            }
        }

        public override int MaxHP
        {
            get
            {
                if (colonist.Attributes != null)
                    return (int)colonist.Attributes.GetAttributeMax(AttributeType.Health);
                return max_hp;
            }
        }

        public override int Armor
        {
            get
            {
                float bonus = 1f + character.GetBonusValue(BonusType.ArmorPercent);
                int armor = Mathf.RoundToInt(this.armor * bonus) + Mathf.RoundToInt(character.GetBonusValue(BonusType.ArmorValue));
                return armor;
            }
        }
    }
}