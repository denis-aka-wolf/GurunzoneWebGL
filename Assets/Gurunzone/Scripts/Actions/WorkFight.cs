using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    [CreateAssetMenu(fileName = "Work", menuName = "Gurunzone/Work/WorkFight", order = 40)]
    public class WorkFight : WorkBasic
    {
        public ActionAttack action_attack;

        public override void StartWork(Colonist colonist)
        {
            Interactable target = colonist.GetWorkTarget();
            colonist.Character.AttackTarget(target);
        }

        public override void StopWork(Colonist colonist)
        {

        }

        public override bool CanDoWork(Colonist colonist, Interactable target)
        {
            if (target != null)
            {
                Destructible destruct = target.Destructible;
                bool azone = destruct != null && destruct.CanBeAttacked();
                return azone;
            }
            return false;
        }

        public override Interactable FindBestTarget(Vector3 pos)
        {
            Destructible destruct = Destructible.GetNearest(AttackTeam.Enemy, pos);
            if (destruct != null)
                return destruct.Interactable;
            return null;
        }
    }
}
