using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    [CreateAssetMenu(fileName = "Work", menuName = "Gurunzone/Work/WorkBuild", order = 40)]
    public class WorkBuild : WorkBasic
    {
        public ActionBuild action_build;

        public override void StartWork(Colonist colonist)
        {
            Interactable target = colonist.GetWorkTarget();
            Construction construct = target.GetComponent<Construction>();
            if (construct != null)
            {
                colonist.AutoOrder(action_build, target);
            }
        }

        public override void StopWork(Colonist colonist)
        {
            
        }

        public override bool CanDoWork(Colonist colonist, Interactable target)
        {
            if (target != null)
            {
                Construction construct = target.GetComponent<Construction>();
                return construct != null && construct.IsConstructing() && action_build.CanDoAction(colonist.Character, target);
            }
            return false;
        }

        public override Interactable FindBestTarget(Vector3 pos)
        {
            Construction construct = Construction.GetNearestUnassigned(pos, range);
            return construct?.Interactable;
        }
    }
}