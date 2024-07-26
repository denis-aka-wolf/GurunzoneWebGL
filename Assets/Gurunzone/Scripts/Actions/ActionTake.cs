using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Action to take items
    /// </summary>
    
    [CreateAssetMenu(fileName = "Action", menuName = "Gurunzone/Actions/Take", order = 50)]
    public class ActionTake : ActionBasic
    {
        public ActionBasic action_return;

        public override void StartAction(Character character, Interactable target)
        {
            if (character.Inventory.IsMax())
            {
                FindReturnTarget(character);
                return;
            }

            Item item = target.GetComponent<Item>();
            if (item != null)
            {
                character.PlayAnim("take");
                character.FaceToward(target.transform.position);
                character.WaitFor(1f, () =>
                {
                    if (item != null)
                    {
                        int quantity = item.quantity;
                        int max = character.Inventory.CountAvailableCargo();
                        quantity = Mathf.Min(quantity, max);
                        character.Inventory.AddItem(item.data, quantity);
                        item.quantity -= quantity;

                        if (item.quantity <= 0)
                            item.Kill();

                        FindReturnTarget(character);
                    }
                });
            }
        }

        public override void StopAction(Character character, Interactable target)
        {
            character.StopAnimate();
            character.HideTools();
        }

        public override void UpdateAction(Character character, Interactable target)
        {
            
        }

        public override bool CanDoAction(Character character, Interactable target)
        {
            Item item = target != null ? target.GetComponent<Item>() : null;
            return item != null && character.Inventory != null && !character.Inventory.IsMax();
        }

        private void FindReturnTarget(Character character)
        {
            character.StopAnimate();
            character.HideTools();
            character.Stop();

            Storage storage = Storage.GetNearestActive(character.transform.position, 200f);
            if (storage != null && !character.Inventory.IsEmpty())
            {
                character.OrderInterupt(action_return, storage.Interactable);
                FindNextTarget(character); //Automatically take next item
            }
            else
            {
                character.Stop();
            }
        }

        private void FindNextTarget(Character character)
        {
            Item item = Item.GetNearest(character.transform.position, 20f);
            if (item != null && character.CountQueuedOrders() == 0) //Interupt is not added to queue, so queue should be 0 unless player gave manual order with shift
            {
                character.OrderNext(this, item.Interactable);
            }
        }
    }
}
