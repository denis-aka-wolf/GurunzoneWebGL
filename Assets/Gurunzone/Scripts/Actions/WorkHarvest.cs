using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    [CreateAssetMenu(fileName = "Work", menuName = "Gurunzone/Work/WorkHarvest", order = 40)]
    public class WorkHarvest : WorkBasic
    {
        public ActionGather action_gather;
        public ActionAttack action_attack;
        public ActionTake action_take;

        public override void StartWork(Colonist colonist)
        {
            Interactable target = colonist.GetWorkTarget();
            Zone zone = target.GetComponent<Zone>();
            Gatherable gather = target.GetComponent<Gatherable>();
            Item item = zone?.GetNearestItem(colonist.transform.position);
            Destructible attack_targ = zone?.GetNearestDestructible(colonist.transform.position);
            if (zone != null)
                gather = zone.GetNearestGatherable(colonist.Character, colonist.transform.position);

            if (item != null)
                colonist.AutoOrder(action_take, item.Interactable);
            else if (attack_targ != null)
                colonist.AutoOrder(action_attack, attack_targ.Interactable);
            else if (gather != null)
                colonist.AutoOrder(action_gather, gather.Interactable);
        }

        public override void StopWork(Colonist colonist)
        {

        }

        public override bool CanDoWork(Colonist colonist, Interactable target)
        {
            if (target != null)
            {
                Zone zone = target.GetComponent<Zone>();
                Gatherable gather = target.GetComponent<Gatherable>();
                bool azone = zone != null && zone.CanBeGathered(colonist.Character);
                bool agather = gather != null && gather.CanHarvest(colonist.Character);
                return azone || agather;
            }
            return false;
        }

        public override Interactable FindBestTarget(Vector3 pos)
        {
            Zone zone = Zone.GetNearestUnassigned(pos, range);
            Gatherable gather = Gatherable.GetNearestUnassigned(pos, range);
            if (zone != null)
                return zone.Interactable;
            else if (gather != null)
                return gather.Interactable;
            return null;
        }
    }
}
