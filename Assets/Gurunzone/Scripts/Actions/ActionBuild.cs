using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Action to havest resources
    /// </summary>
    
    [CreateAssetMenu(fileName = "Action", menuName = "Gurunzone/Actions/Build", order = 50)]
    public class ActionBuild : ActionBasic
    {
        public string build_anim = "build"; 
        public GroupData build_tool;

        public override void StartAction(Character character, Interactable target)
        {
            Construction building = target.GetComponent<Construction>();
            if (building != null)
            {
                if (!building.HasPaidCost())
                    building.PayBuildCost();
                if (building.HasPaidCost())
                {
                    character.HideTools();
                    character.Animate(build_anim, true);
                    character.ShowTool(build_tool);
                    character.FaceToward(target.transform.position);
                }
            }
        }

        public override void StopAction(Character character, Interactable target)
        {
            character.StopAnimate();
            character.HideTools();
        }

        public override void UpdateAction(Character character, Interactable target)
        {
            float speed = TheGame.Get().GetGameTimeSpeed();
            Construction building = target.GetComponent<Construction>();
            float bonus = character.GetBonusMult(BonusType.BuildSpeed, target);
            float value = speed * bonus * Time.deltaTime;
            building.AddProgress(value);

            if (building.IsCompleted())
            {
                bool auto = character.Colonist != null && character.Colonist.IsAuto();
                character.Stop();

                //Start gathering built
                GatherBuilding gbuilding = building.GetComponent<GatherBuilding>();
                ActionGather gather = ActionBasic.Get<ActionGather>();
                if (!auto && gbuilding != null && gather != null && character.CountQueuedOrders() == 0)
                {
                    character.Order(gather, gbuilding.Interactable);
                }

                //Start working built
                Factory fbuilding = building.GetComponent<Factory>();
                ActionFactory factory = ActionBasic.Get<ActionFactory>();
                if (!auto && fbuilding != null && factory != null && character.CountQueuedOrders() == 0)
                {
                    character.Order(factory, fbuilding.Interactable);
                }
            }
        }

        public override bool CanDoAction(Character character, Interactable target)
        {
            Construction building = target != null ? target.GetComponent<Construction>() : null;
            return building != null && !building.IsBuildMode() && building.IsConstructing() 
                && (building.CanPayBuildCost() || building.HasPaidCost());
        }

    }
}
