using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Just a script to access the Headquarter easily, doesn't do much on its own.
    /// </summary>

    public class Headquarter : MonoBehaviour
    {

        private static List<Headquarter> headquarters = new List<Headquarter>();

        void Awake()
        {
            headquarters.Add(this);
        }

        private void OnDestroy()
        {
            headquarters.Remove(this);
        }

        public static Headquarter GetMain()
        {
            return GetNearest(Vector3.zero);
        }

        public static Headquarter GetNearest(Vector3 pos, float range = 9999f)
        {
            float min_dist = range;
            Headquarter nearest = null;
            foreach (Headquarter hq in headquarters)
            {
                float dist = (hq.transform.position - pos).magnitude;
                if (dist < min_dist)
                {
                    min_dist = dist;
                    nearest = hq;
                }
            }
            return nearest;
        }

        public static List<Headquarter> GetAll()
        {
            return headquarters;
        }
    }
}