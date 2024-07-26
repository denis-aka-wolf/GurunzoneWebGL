using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Increases population limit
    /// </summary>

    [RequireComponent(typeof(Selectable))]
    public class House : MonoBehaviour
    {
        public int population = 1;      //Population increase

        private Selectable select;
        private Construction construct; //May be null

        private static List<House> house_list = new List<House>();

        void Awake()
        {
            house_list.Add(this);
            select = GetComponent<Selectable>();
            construct = GetComponent<Construction>();
        }

        private void OnDestroy()
        {
            house_list.Remove(this);
        }

        void Update()
        {
            
        }

        public bool IsCompleted()
        {
            if (construct != null)
                return construct.IsCompleted();
            return true;
        }

        public Selectable Selectable { get { return select; } }
        public Construction Construction { get { return construct; } }

        public static int CountMaxPopulation()
        {
            int count = 0;
            foreach (House house in house_list)
            {
                if(house.IsCompleted())
                    count += house.population;
            }
            return count + TechManager.Get().GetTechBonusInt(BonusType.ColonySize);
        }

        public static List<House> GetAll()
        {
            return house_list;
        }
    }
}
