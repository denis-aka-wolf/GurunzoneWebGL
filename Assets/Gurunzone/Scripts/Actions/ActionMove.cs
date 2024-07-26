using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Action to havest resources
    /// </summary>
    
    [CreateAssetMenu(fileName = "Action", menuName = "Gurunzone/Actions/Move", order = 50)]
    public class ActionMove : ActionBasic
    {
        public override void StartAction(Character character, Interactable target)
        {
            character.MoveToTarget(target);
        }

        public override void StopAction(Character character, Interactable target)
        {
            
        }

        public override void UpdateAction(Character character, Interactable target)
        {
            if (target == null)
            {
                character.StopAction();
                return;
            }

            if (character.HasReachedTarget())
                character.StopAction();
        }

        public override bool CanDoAction(Character character, Interactable target)
        {
            return target != null ;
        }

    }
}
