using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gurunzone
{

    public class WorkLine : MonoBehaviour
    {
        public Text title;
        public Text status;
        public Toggle fight_toggle;
        public Toggle harvest_toggle;
        public Toggle build_toggle;
        public Toggle produce_toggle;

        private Colonist colonist;

        void Start()
        {
            fight_toggle.onValueChanged.AddListener(OnChangeToggle);
            harvest_toggle.onValueChanged.AddListener(OnChangeToggle);
            build_toggle.onValueChanged.AddListener(OnChangeToggle);
            produce_toggle.onValueChanged.AddListener(OnChangeToggle);

            foreach (ActionButton button in GetComponentsInChildren<ActionButton>())
                button.onClick += OnClickAction;
        }

        public void SetLine(Colonist colonist)
        {
            this.colonist = colonist;

            if (colonist != null)
            {
                title.text = colonist.GetName();
                status.text = colonist.Attributes.GetStatusText();

                WorkFight fight = WorkBasic.Get<WorkFight>();
                WorkHarvest harvest = WorkBasic.Get<WorkHarvest>();
                WorkBuild build = WorkBasic.Get<WorkBuild>();
                WorkFactory produce = WorkBasic.Get<WorkFactory>();

                fight_toggle.isOn = colonist.SData.IsActionEnabled(fight.id);
                harvest_toggle.isOn = colonist.SData.IsActionEnabled(harvest.id);
                build_toggle.isOn = colonist.SData.IsActionEnabled(build.id);
                produce_toggle.isOn = colonist.SData.IsActionEnabled(produce.id);

                gameObject.SetActive(true);
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnChangeToggle(bool val)
        {
            if (colonist != null)
            {
                bool bfight = fight_toggle.isOn;
                bool bharvest = harvest_toggle.isOn;
                bool bbuild = build_toggle.isOn;
                bool bproduce = produce_toggle.isOn;

                WorkFight fight = WorkBasic.Get<WorkFight>();
                WorkHarvest harvest = WorkBasic.Get<WorkHarvest>();
                WorkBuild build = WorkBasic.Get<WorkBuild>();
                WorkFactory produce = WorkBasic.Get<WorkFactory>();

                colonist.SData.SetActionEnabled(fight.id, bfight);
                colonist.SData.SetActionEnabled(harvest.id, bharvest);
                colonist.SData.SetActionEnabled(build.id, bbuild);
                colonist.SData.SetActionEnabled(produce.id, bproduce);
            }
        }

        private void OnClickAction(ActionBasic action)
        {
            if (colonist != null && !colonist.IsDead() && action.CanDoAction(colonist.Character, null))
                colonist.OrderInterupt(action, null);
        }

        public void OnClickColonist()
        {
            Selectable.UnselectAll();
            colonist.Selectable.Select();
            TheCamera.Get().MoveToTarget(colonist.transform.position);
            WorkPanel.Get().Hide();
        }
    }
}