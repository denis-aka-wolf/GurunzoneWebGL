using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Action to havest resources
    /// </summary>
    
    [CreateAssetMenu(fileName = "Action", menuName = "Gurunzone/Actions/Store", order = 50)]
    public class ActionStore : ActionBasic
    {
        public override void StartAction(Character character, Interactable target)
        {
            Storage storage = target.GetComponent<Storage>();
            if (storage != null)
            {
                character.FaceToward(target.transform.position);
                character.WaitFor(0.2f, () =>
                {
                    character.Inventory.Transfer(storage.Inventory);
                    character.StopAnimate();
                    character.HideTools();
                    character.Stop();
                });
            }
        }

        public override void StopAction(Character character, Interactable target)
        {

        }

        public override void UpdateAction(Character character, Interactable target)
        {
            
        }

        public override bool CanDoAction(Character character, Interactable target)
        {
            Storage storage = target != null ? target.GetComponent<Storage>() : null;
            return storage != null && character.Inventory != null && storage.IsActive() && character.Inventory.CountItems() > 0;
        }
    }
}
