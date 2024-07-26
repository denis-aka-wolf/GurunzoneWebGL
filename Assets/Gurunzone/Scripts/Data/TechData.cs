using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Data for Techs that can be researched
    /// </summary>

    [CreateAssetMenu(fileName = "TechData", menuName = "Gurunzone/AppData/TechData", order = 10)]
    public class TechData : CraftData
    {
        [Header("Tech")]
        public BonusType effect;    //Effect provided by the Tech
        public float value;         //Value of the effect

        public GroupData[] tech_receiver; //The object receiving the bonus (ex: colonist, tower) (if empty, will apply to all)
        public GroupData[] tech_target;  //The tech is only active when targeting selectable/item/craftable (ex: wood for wood cutting techs) (if empty, will apply to all)

        private static List<TechData> tlist = new List<TechData>();

        public bool CanReceive(Selectable receiver)
        {
            if (receiver == null)
                return true; //Everything
            if (tech_receiver.Length == 0)
                return true; //Everything

            foreach (GroupData group in tech_receiver)
            {
                if (receiver.HasGroup(group))
                    return true;
            }
            return false;
        }

        public bool CanTarget(Selectable target)
        {
            if (target == null)
                return false; //Invalid target, use CraftData
            if (tech_target.Length == 0)
                return true; //Targets everything

            foreach (GroupData group in tech_target)
            {
                if (target.HasGroup(group))
                    return true;
            }
            return false;
        }

        public bool CanTarget(CraftData item)
        {
            if (item == null)
                return false; //Invalid target, use Selectable
            if (tech_target.Length == 0)
                return true; //Targets everything

            foreach (GroupData group in tech_target)
            {
                if (item.HasGroup(group))
                    return true;
            }
            return false;
        }

        public static new void Load(string folder = "")
        {
            tlist.Clear();
            tlist.AddRange(Resources.LoadAll<TechData>(folder));
        }

        public static new TechData Get(string id)
        {
            foreach (TechData tech in tlist)
            {
                if (tech.id == id)
                    return tech;
            }
            return null;
        }

        public static new List<TechData> GetAll()
        {
            return tlist;
        }
    }
}
