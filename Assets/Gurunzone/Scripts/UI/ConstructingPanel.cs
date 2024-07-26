using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gurunzone
{

    public class ConstructingPanel : SelectPanel
    {
        public Text title;
        public ProgressBar progress;

        private Construction construct;
        
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Update()
        {
            base.Update();

            if (construct != null && progress != null)
            {
                progress.value = construct.GetProgress();
                progress.value_max = construct.GetProgressDuration();
            }

            if (construct != null && construct.IsCompleted())
                Hide();
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            construct = select.GetComponent<Construction>();
            title.text = construct.data.title;
        }

        public override int GetPriority()
        {
            return 50;
        }

        public override bool IsShowable(Selectable select)
        {
            Construction construct = select.GetComponent<Construction>();
            return construct != null && construct.IsConstructing();
        }

        public new void OnClickDestroy()
        {
            DestroyPanel.Get().ShowDestroy(construct);
        }

        public new void OnClickQuickDestroy()
        {
            if (construct != null)
                construct.CancelConstruct();
        }
    }
}
