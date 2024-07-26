using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Action to havest resources
    /// </summary>
    
    [CreateAssetMenu(fileName = "Action", menuName = "Gurunzone/Actions/Attack", order = 50)]
    public class ActionAttack : ActionBasic
    {
        public GroupData weapon;
        public GroupData loot_gather;

        public override void StartAction(Character character, Interactable target)
        {
            character.HideTools();
            character.ShowTool(weapon);
            character.FaceToward(target.transform.position);
        }

        public override void StopAction(Character character, Interactable target)
        {
            character.StopAnimate();
            character.HideTools();
        }

        public override void UpdateAction(Character character, Interactable target)
        {
            Destructible destruct = target.Destructible;
            if (destruct.IsDead())
            {
                FindNextTarget(character);
                return;
            }

            if (character.Attack.IsCooldownReady())
            {
                if (!character.Attack.IsAttackTargetInRange(destruct.Interactable))
                    AttackTarget(character, destruct);
                else
                    character.Attack.AttackStrike(destruct);
            }
        }

        public override bool CanDoAction(Character character, Interactable target)
        {
            Destructible destruct = target != null ? target.Destructible : null;
            return destruct != null && character.Attack != null && character.Attack.CanAttack(destruct);
        }

        private void AttackTarget(Character character, Destructible target)
        {
            character.StopAction();
            ActionAttack attack = ActionBasic.Get<ActionAttack>();
            if (attack != null && target != null)
            {
                character.OrderInterupt(attack, target.Interactable);
            }
        }

        private void FindNextTarget(Character character)
        {
            character.Stop();

            Vector3 pos = character.GetLastTargetPos();
            Gatherable nTarget = Gatherable.GetNearest(loot_gather, pos, 3f);
            ActionGather gather = ActionBasic.Get<ActionGather>();
            if (gather != null && nTarget != null)
            {
                character.OrderInterupt(gather, nTarget.Interactable);
            }
        }
    }
}
