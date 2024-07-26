using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Gurunzone
{
    public class Region : MonoBehaviour
    {
        public string region_id;        //Optional, for getting this region with Region.Get()
        public GroupData trigger_group; //Which groups can trigger this region?

        [HideInInspector]
        public event UnityAction<Character> onEnterRegion;
        [HideInInspector]
        public event UnityAction<Character> onExitRegion;
        
        private int nb_in_region;
        private Collider collide;
        private Collider2D collide2d;
        private Bounds bounds;

        private static List<Region> region_list = new List<Region>();

        private void Awake()
        {
            region_list.Add(this);
            if (GetComponent<Renderer>())
                GetComponent<Renderer>().enabled = false;
            collide = GetComponent<Collider>();
            collide2d = GetComponent<Collider2D>();
            if(collide2d != null)
                bounds = collide2d.bounds;
            if (collide != null)
                bounds = collide.bounds;
            nb_in_region = 0;
        }
        
        private void OnDestroy()
        {
            region_list.Remove(this);
        }

        private void TriggerRegionEffects(Character triggerer)
        {
            //Custom event
            if (onEnterRegion != null)
                onEnterRegion.Invoke(triggerer);
        }

        private void TriggerRegionEffectsExit(Character triggerer)
        {
            //Custom events
            if (onExitRegion != null)
                onExitRegion.Invoke(triggerer);
        }

        private void OnEnter(Character actor)
        {
            if (actor)
            {
                if (trigger_group == null || actor.Selectable.HasGroup(trigger_group))
                {
                    nb_in_region++;
                    if (nb_in_region >= 1)
                    {
                        TriggerRegionEffects(actor);
                    }
                }
            }
        }

        private void OnExit(Character actor)
        {
            if (actor)
            {
                if (trigger_group == null || actor.Selectable.HasGroup(trigger_group))
                {
                    nb_in_region--;
                    if (nb_in_region == 0)
                    {
                        TriggerRegionEffectsExit(actor);
                    }
                }
            }
        }

        void OnTriggerEnter(Collider coll)
        {
            Character actor = coll.GetComponent<Character>();
            OnEnter(actor);
        }

        void OnTriggerExit(Collider coll)
        {
            Character actor = coll.GetComponent<Character>();
            OnExit(actor);
        }

        void OnTriggerEnter2D(Collider2D coll)
        {
            Character actor = coll.GetComponent<Character>();
            OnEnter(actor);
        }

        void OnTriggerExit2D(Collider2D coll)
        {
            Character actor = coll.GetComponent<Character>();
            OnExit(actor);
        }

        public bool IsActivated()
        {
            return nb_in_region >= 1;
        }

        public Vector3 PickRandomPosition()
        {
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float y = Random.Range(bounds.min.y, bounds.max.y);
            float z = Random.Range(bounds.min.z, bounds.max.z);
            return new Vector3(x, y, z);
        }

        public bool IsInside(Vector3 position)
        {
            return (position.x > bounds.min.x && position.x < bounds.max.x 
                && position.y > bounds.min.y && position.y < bounds.max.y
                && position.z > bounds.min.z && position.z < bounds.max.z);
        }

        public bool IsInsideXY(Vector3 position)
        {
            return (position.x > bounds.min.x && position.y > bounds.min.y && position.x < bounds.max.x && position.y < bounds.max.y);
        }

        public bool IsInsideXZ(Vector3 position)
        {
            return (position.x > bounds.min.x && position.z > bounds.min.z && position.x < bounds.max.x && position.z < bounds.max.z);
        }

        public static Region GetNearest(Vector3 pos, float range =999f)
        {
            float min_dist = range;
            Region nearest = null;
            foreach (Region region in region_list)
            {
                float dist = (pos - region.transform.position).magnitude;
                if (dist < min_dist)
                {
                    min_dist = dist;
                    nearest = region;
                }
            }
            return nearest;
        }

        public static Region Get(string id)
        {
            List<Region> valid_region = GetAll(id);
            if (valid_region.Count == 1)
                return valid_region[0];
            if (valid_region.Count > 1)
                return valid_region[Random.Range(0, valid_region.Count)];
            return null;
        }

        public static List<Region> GetAll(string id)
        {
            List<Region> valid_region = new List<Region>();
            foreach (Region region in region_list)
            {
                if (region.region_id == id)
                {
                    valid_region.Add(region);
                }
            }
            return valid_region;
        }

        public static Region[] GetAll()
        {
            return region_list.ToArray();
        }
    }

}