using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gurunzone
{

    public class HousePanel : SelectPanel
    {
        public Text title;
        public Text population;

        private House house;

        private static HousePanel instance;

        protected override void Awake()
        {
            base.Awake();
            instance = this;
        }

        protected override void Start()
        {
            base.Start();

        }

        protected override void Update()
        {
            base.Update();

            if (!IsVisible() || house == null)
                return;

            int pop = Colonist.CountPopulation();
            int max = House.CountMaxPopulation();

            title.text = house.Construction?.data != null ? house.Construction.data.title : "Population";
            population.text = pop + " / " + max;
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            house = select.GetComponent<House>();
        }

        public override bool IsShowable(Selectable select)
        {
            House house = select.GetComponent<House>();
            return house != null;
        }

        public override int GetPriority()
        {
            return 2;
        }

        public static HousePanel Get()
        {
            return instance;
        }
    }

}
