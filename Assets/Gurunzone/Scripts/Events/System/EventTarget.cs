using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone.Events
{
    public enum TargetType
    {
        None=0,
        GameObject=5,
        Region = 10,
        RandomInGroup=15,
        LastCreated=20, //Last object create with Spawn or Create
        EventTriggerer=25, //Object that triggered the event (example: that entered the region)
        CustomTarget = 30,
    }

    [System.Serializable]
    public class EventTarget
    {
        public TargetType type;
        public string target_id;
        public GameObject target_object;
        public GroupData target_group;

        public GameObject GetTarget()
        {
            if (type == TargetType.GameObject)
            {
                return target_object;
            }
            if (type == TargetType.Region)
            {
                Region region = Region.Get(target_id);
                return region?.gameObject;
            }
            if (type == TargetType.RandomInGroup)
            {
                List<Selectable> list = Selectable.GetAllGroup(target_group);
                if (list.Count > 0)
                {
                    Selectable rand = list[Random.Range(0, list.Count)];
                    return rand?.gameObject;
                }
            }
            if (type == TargetType.LastCreated)
            {
                return EventManager.Get().GetLastCreated();
            }
            if (type == TargetType.EventTriggerer)
            {
                return EventManager.Get().GetEventTriggerer();
            }
            if (type == TargetType.CustomTarget)
            {
                return EventManager.Get().GetCustomTarget(target_id);
            }
            return null;
        }

        public Vector3 GetTargetPos()
        {
            if (type == TargetType.Region)
            {
                Region region = Region.Get(target_id);
                if (region != null)
                    return region.PickRandomPosition();
            }

            GameObject obj = GetTarget();
            if (obj != null)
                return obj.transform.position;

            return Vector3.zero;
        }

        public bool HasTarget()
        {
            return GetTarget() != null;
        }

    }
}
