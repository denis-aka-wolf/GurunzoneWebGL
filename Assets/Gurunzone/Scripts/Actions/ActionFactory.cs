using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Action to work in a factory (kitchen or other...)
    /// </summary>
    
    [CreateAssetMenu(fileName = "Action", menuName = "Gurunzone/Actions/Factory", order = 50)]
    public class ActionFactory : ActionBasic
    {
        public string work_anim = "work"; //boolean param

        public override void StartAction(Character character, Interactable target)
        {
            Factory factory = target.GetComponent<Factory>();
            if (factory != null)
            {
                character.FaceToward(factory.transform.position);
                character.Animate(work_anim, true);
            }
        }

        public override void StopAction(Character character, Interactable target)
        {
            character.StopAnimate();
        }

        public override void UpdateAction(Character character, Interactable target)
        {
            if (target == null)
            {
                character.Stop();
                return;
            }

            Factory factory = target?.GetComponent<Factory>();
            CraftData data = factory.GetSelected();
            if (data != null && factory.CanProduce())
            {
                float gspeed = TheGame.Get().GetGameTimeSpeed();
                float speed = gspeed * character.GetBonusMult(BonusType.FactorySpeed, target, data);
                factory.AddWorkerProgress(speed);

                float prod_speed = factory.GetProductionSpeed(speed) * Time.deltaTime;
                character.Colonist?.Attributes.AddXP(BonusType.FactorySpeed, prod_speed, target);
            }

            if (factory.Inventory != null && !factory.Inventory.global && character.Inventory != null
                && factory.Inventory.GetCargo() >= character.Inventory.GetCargoMax())
            {
                factory.Inventory.Transfer(character.Inventory);
                FindReturnTarget(character, target);
                return;
            }
        }

        public override bool CanDoAction(Character character, Interactable target)
        {
            Factory factory = target != null ? target.GetComponent<Factory>() : null;
            return factory != null && factory.CanProduce() && character.Inventory != null;
        }

        private void FindReturnTarget(Character character, Interactable target)
        {
            character.StopAnimate();
            character.HideTools();

            Storage storage = Storage.GetNearestActive(character.transform.position, 200f);
            if (storage != null && !character.Inventory.IsEmpty())
            {
                ActionStore store = ActionBasic.Get<ActionStore>();
                character.OrderInterupt(store, storage.Interactable);
            }
            else
            {
                character.Stop();
            }
        }

    }
}
