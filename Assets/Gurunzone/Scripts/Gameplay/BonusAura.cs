using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Add this script to a construction or object, it will provide a bonus aura to characters around it.
    /// The bonus can either be an attribute increase (AttributeType), or a buff (BonusType)
    /// </summary>

    public class BonusAura : MonoBehaviour
    {
        public AttributeType attribute; //Which attribute in increased (set to None if using a bonus)
        public BonusType bonus;         //Bonus provided (set to None if using Attribute)
        public float value;             //Per game hour for attribute increase. Percentage must be represented in decimals (0.10 for 10%)
        public float range = 10f;       //Radius affected around the object

        void Start()
        {
        
        }

        void Update()
        {
            if (TheGame.Get().IsPaused())
                return;

            foreach (Colonist colonist in Colonist.GetAll())
            {
                float dist = (transform.position - colonist.transform.position).magnitude;
                if (dist < range && !colonist.IsDead())
                {
                    if (colonist.Attributes != null && attribute != AttributeType.None)
                    {
                        float speed = TheGame.Get().GetGameTimeSpeed();
                        colonist.Attributes.AddAttribute(attribute, value * speed * Time.deltaTime);
                    }

                    if (bonus != BonusType.None)
                    {
                        colonist.Character.SetTempBonusEffect(bonus, value, 0.01f);
                    }
                }
            }
        }
    }
}
