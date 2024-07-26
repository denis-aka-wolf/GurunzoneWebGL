using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Action to havest resources
    /// </summary>
    
    [CreateAssetMenu(fileName = "Action", menuName = "Gurunzone/Actions/Sleep", order = 50)]
    public class ActionSleep : ActionBasic
    {
        public float health_increase; //Per game hour
        public float energy_increase;
        public string anim;

        public override void StartAction(Character character, Interactable target)
        {
            character.Animate(anim, true);
        }

        public override void StopAction(Character character, Interactable target)
        {
            character.Animate(anim, false);
        }

        public override void UpdateAction(Character character, Interactable target)
        {
            if (character.Colonist != null && character.Colonist.Attributes != null)
            {
                float speed = TheGame.Get().GetGameTimeSpeed();
                character.Colonist.Attributes.AddAttribute(AttributeType.Health, health_increase * speed * Time.deltaTime);
                character.Colonist.Attributes.AddAttribute(AttributeType.Energy, energy_increase * speed * Time.deltaTime);
            }
        }

        public override bool CanDoAction(Character character, Interactable target)
        {
            if (character.Colonist != null && character.Colonist.Attributes != null)
                return !character.Colonist.Attributes.IsDepleted(AttributeType.Hunger);
            return true;
        }
    }
}
