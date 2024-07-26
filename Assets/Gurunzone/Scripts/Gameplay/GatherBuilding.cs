using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Building built on top of a gatherable, like a mine or oil pump
    /// </summary>

    public class GatherBuilding : Gatherable
    {
        [Header("Building")]
        public float target_dist = 2f;      //Distance that the resource must be at

        private Buildable buildable;
        private Gatherable target;

        protected override void Awake()
        {
            base.Awake();
            buildable = GetComponent<Buildable>();
            if (buildable != null)
                buildable.onBuild += OnBuild;
        }

        protected override void Start()
        {
            base.Start();
            OnBuild();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (target != null)
                target.Selectable.selection_active = true;
        }

        private void OnBuild()
        {
            target = FindTarget();
            if (target != null && !IsBuilding())
                target.Selectable.selection_active = false;
        }

        public override void AddValue(int value)
        {
            if (target != null)
                target.AddValue(value);
        }

        public override int GetValue()
        {
            if (target != null)
                return target ? target.GetValue() : 0;
            return 0;
        }

        public override int GetMaxValue()
        {
            if (target != null)
                return target ? target.GetMaxValue() : 0;
            return 0;
        }

        private Gatherable FindTarget()
        {
            return Gatherable.GetNearest(transform.position, target_dist, this);
        }

        public Gatherable GetTarget()
        {
            return target;
        }

        public override ItemData GetHarvestItem()
        {
            return target?.GetHarvestItem();
        }

        public override ItemData GetItem()
        {
            return target?.GetItem();
        }

        public bool IsBuilding()
        {
            return buildable != null && buildable.IsBuilding();
        }
    }
}
