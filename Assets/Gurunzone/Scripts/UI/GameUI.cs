using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gurunzone
{

    public class GameUI : MonoBehaviour
    {
        public Text day;
        public Text time;
        public Text multiplier;
        public Text paused;

        private float prev_mult = 1f;

        void Start()
        {

        }

        void Update()
        {
            int iday = SaveData.Get().day;
            float itime = SaveData.Get().day_time;
            day.text = iday.ToString();

            int hours = Mathf.FloorToInt(itime);
            int minutes = Mathf.FloorToInt((itime * 60f) % 60f);
            time.text = hours + ":" + minutes.ToString("00");

            multiplier.text = "X" + Mathf.RoundToInt(TheGame.Get().GetSpeedMultiplier());
            paused.enabled = TheGame.Get().GetSpeedMultiplier() < 0.001f;

            if (TheControls.Get().IsPressSpace())
                TogglePause();

            if (TheControls.Get().IsPressNumber(1))
                OnClickSpeed1();
            if (TheControls.Get().IsPressNumber(2))
                OnClickSpeed2();
            if (TheControls.Get().IsPressNumber(3))
                OnClickSpeed3();
        }

        public void TogglePause()
        {
            float mult = TheGame.Get().GetSpeedMultiplier();
            float nmult = mult > 0.001f ? 0f : prev_mult;
            TheGame.Get().SetGameSpeedMultiplier(nmult);
        }

        public void OnClickSpeed0()
        {
            TheGame.Get().SetGameSpeedMultiplier(0f);
        }

        public void OnClickSpeed1()
        {
            prev_mult = 1f;
            TheGame.Get().SetGameSpeedMultiplier(1f);
        }

        public void OnClickSpeed2()
        {
            prev_mult = 2f;
            TheGame.Get().SetGameSpeedMultiplier(2f);
        }

        public void OnClickSpeed3()
        {
            prev_mult = 4f;
            TheGame.Get().SetGameSpeedMultiplier(4f);
        }
    }
}
