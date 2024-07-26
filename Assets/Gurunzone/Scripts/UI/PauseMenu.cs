using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{

    public class PauseMenu : UIPanel
    {

        private static PauseMenu instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }

        public void OnClickSave()
        {
            TheGame.Get().Save();
        }

        public void OnClickLoad()
        {
            TheGame.Get().Pause();
            TheGame.Load();
        }

        public void OnClickNew()
        {
            TheGame.Get().Pause();
            TheGame.NewGame();
        }

        public void OnClickBack()
        {
            Hide();
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            TheGame.Get().Pause();
        }

        public override void Hide(bool instant = false)
        {
            base.Hide(instant);
            TheGame.Get().Unpause();
        }

        public static PauseMenu Get()
        {
            return instance;
        }
    }

}