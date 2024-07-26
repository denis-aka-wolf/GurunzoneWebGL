using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gurunzone
{

    public class GatherPanel : SelectPanel
    {
        [Header("Resource")]
        public Text title;
        public Text quantity;
        public IntSelector workers_select;

        private Gatherable resource;

        private static GatherPanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }

        protected override void Start()
        {
            base.Start();
            workers_select.onChange += OnChangeWorkers;
        }

        protected override void Update()
        {
            base.Update();

            if (resource == null || !resource.IsAlive())
            {
                Hide();
                return;
            }

            quantity.text = resource.GetValue() + " / " + resource.GetMaxValue();
            workers_select.SetValue(resource.Workable.GetWorkerAmount());
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            Gatherable gather = select.GetComponent<Gatherable>();
            resource = gather;
            workers_select.value_max = resource.Workable.workers_max;
            ItemData item = gather.GetItem();
            title.text = item.title;
        }

        public override bool IsShowable(Selectable select)
        {
            Gatherable gather = select.GetComponent<Gatherable>();
            return gather != null && gather.GetItem() != null;
        }

        private void OnChangeWorkers(int value)
        {
            if (resource)
                resource.Workable.SetWorkerAmount(value);
        }

        public override int GetPriority()
        {
            return 2;
        }

        public static GatherPanel Get()
        {
            return instance;
        }
    }

}
