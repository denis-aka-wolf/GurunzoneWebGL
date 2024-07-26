using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    [CreateAssetMenu(fileName = "Work", menuName = "Gurunzone/Work/WorkFactory", order = 40)]
    public class WorkFactory : WorkBasic
    {
        public override void StartWork(Colonist colonist)
        {
            Interactable target = colonist.GetWorkTarget();
            ActionBasic action = colonist.Character.GetPriorityAction(target);
            colonist.AutoOrder(action, target);
        }

        public override void StopWork(Colonist colonist)
        {

        }

        public override bool CanDoWork(Colonist colonist, Interactable target)
        {
            if (target != null)
            {
                Factory factory = target.GetComponent<Factory>();
                if (factory != null)
                {
                    CraftData data = factory.GetSelected();
                    return data != null && data.HasRequirements();
                }
            }
            return false;
        }

        public override Interactable FindBestTarget(Vector3 pos)
        {
            Factory factory = Factory.GetNearestUnassigned(pos, range);
            return factory?.Interactable;
        }
    }
}
