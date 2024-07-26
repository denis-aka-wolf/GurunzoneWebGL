using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Action to havest resources
    /// </summary>
    
    [CreateAssetMenu(fileName = "Action", menuName = "Gurunzone/Actions/Gather", order = 50)]
    public class ActionGather : ActionBasic
    {
        public float storage_dist = 200f;   //Max auto-detect storage distance
        public float next_dist = 20f;       //Max auto-detect distance for next resource when this one is completed

        public override void StartAction(Character character, Interactable target)
        {
            Gatherable gather = target.GetComponent<Gatherable>();
            if (gather != null)
            {
                character.HideTools();
                character.Animate(gather.harvest_anim, true);
                character.ShowTool(gather.harvest_tool);
                character.FaceToward(target.transform.position);
            }
        }

        public override void StopAction(Character character, Interactable target)
        {
            character.StopAnimate();
            character.HideTools();

            Gatherable gather = target != null ? target.GetComponent<Gatherable>() : null;
            if (gather != null && gather.GetValue() <= 0 && character.CountQueuedOrders() == 0)
                FindReturnTarget(character, gather);
        }

        public override void UpdateAction(Character character, Interactable target)
        {
            Gatherable gather = target.GetComponent<Gatherable>();
            float speed = TheGame.Get().GetGameTimeSpeed();
            float bonus = character.GetBonusMult(BonusType.GatherSpeed, gather.Interactable, gather.GetItem());
            character.AddActionProgress(speed * gather.harvest_speed * bonus * Time.deltaTime);
            if (character.GetActionProgress() > 1f)
            {
                character.SetActionProgress(0f);
                character.Inventory.AddItem(gather.GetHarvestItem(), 1);
                character.Colonist?.Attributes.AddXP(BonusType.GatherSpeed, 1, target);
                gather.AddValue(-1);
            }

            if (gather.GetValue() <= 0 || character.Inventory.IsMax())
            {
                bool found = FindReturnTarget(character, gather);
                if (!found)
                    character.Stop();
            }
        }

        public override void TriggerAction(Character character, Interactable target, string trigger)
        {
            if (trigger == "hit")
            {
                Gatherable gather = target.GetComponent<Gatherable>();
                TheAudio.Get().PlaySFX3D("harvest", gather.harvest_audio, target.transform.position);
            }
        }

        public override bool CanDoAction(Character character, Interactable target)
        {
            Gatherable gather = target != null ? target.GetComponent<Gatherable>() : null;
            return gather != null && character.Inventory != null && gather.CanHarvest(character);
        }

        private bool FindReturnTarget(Character character, Gatherable target)
        {
            character.StopAnimate();
            character.HideTools();

            Storage storage = Storage.GetNearestActive(character.transform.position, storage_dist);
            if (storage != null && !character.Inventory.IsEmpty())
            {
                //Return to storage
                ActionStore store = ActionBasic.Get<ActionStore>();
                character.OrderInterupt(store, storage.Interactable);

                //Find next resource to harvest
                Gatherable next = Gatherable.GetNearest(target.item, character.transform.position, next_dist, target);
                if (next != null && character.CountQueuedOrders() <= 2)
                    character.OrderNext(this, next.Interactable, true);

                return true;
            }
            return false;
        }
    }
}
