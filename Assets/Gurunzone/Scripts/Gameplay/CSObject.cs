using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// A generic ColonySimulator class that includes items, colonist, buildings, NPCs and spawnables
    /// Basically, any object that is linked to a data file (CSData)
    /// </summary>

    [RequireComponent(typeof(Selectable))]
    [RequireComponent(typeof(UniqueID))]
    public abstract class CSObject : MonoBehaviour
    {
        private Selectable cselect;

        private static List<CSObject> obj_list = new List<CSObject>();

        protected virtual void Awake()
        {
            obj_list.Add(this);
            cselect = GetComponent<Selectable>();

            TheGame game = TheGame.Get();
            if (game != null)
            {
                game.onStart += AfterStart;
                game.onStartLoad += AfterLoad;
                game.onStartNewGame += AfterNew;
            }
        }

        protected virtual void OnDestroy()
        {
            obj_list.Remove(this);

            TheGame game = TheGame.Get();
            if (game != null)
            {
                game.onStart -= AfterStart;
                game.onStartLoad -= AfterLoad;
                game.onStartNewGame -= AfterNew;
            }
        }

        protected virtual void Start()
        {

        }

        protected virtual void AfterStart()
        {

        }

        protected virtual void AfterLoad()
        {

        }

        protected virtual void AfterNew()
        {

        }

        protected virtual void Update()
        {

        }

        public virtual void Kill()
        {
            if (cselect != null)
                cselect.Kill();
            else
                Destroy(gameObject);
        }

        public virtual CSData GetData()
        {
            if (this is Colonist)
            {
                Colonist colonist = (Colonist)this;
                return colonist.data;
            }
            if (this is Item)
            {
                Item item = (Item)this;
                return item.data;
            }
            if (this is Construction)
            {
                Construction constr = (Construction)this;
                return constr.data;
            }
            if (this is NPC)
            {
                NPC npc = (NPC)this;
                return npc.data;
            }
            if (this is Spawnable)
            {
                Spawnable spawn = (Spawnable)this;
                return spawn.data;
            }
            return null;
        }

        //--- Static functions for easy access

        public static int CountObjectsInRadius(CSData data, Vector3 pos, float radius)
        {
            int count = 0;
            foreach (CSObject obj in GetAll())
            {
                if (data == obj.GetData())
                {
                    float dist = (pos - obj.transform.position).magnitude;
                    if (dist < radius)
                        count++;
                }
            }
            return count;
        }

        public static CSObject GetNearest(Vector3 pos, float range = 999f)
        {
            CSObject nearest = null;
            float min_dist = range;
            foreach (CSObject item in obj_list)
            {
                float dist = (item.transform.position - pos).magnitude;
                if (dist < min_dist)
                {
                    min_dist = dist;
                    nearest = item;
                }
            }
            return nearest;
        }

        public static List<CSObject> GetAll()
        {
            return obj_list;
        }

        public static GameObject Create(CSData data, Vector3 pos)
        {
            if (data == null)
                return null;

            if (data is SpawnData)
            {
                SpawnData item = (SpawnData)data;
                GameObject obj = Spawnable.Create(item, pos);
                return obj;
            }

            if (data is ItemData)
            {
                ItemData item = (ItemData)data;
                Item obj = Item.Create(item, pos, 1);
                return obj.gameObject;
            }

            if (data is ConstructionData)
            {
                ConstructionData item = (ConstructionData)data;
                Construction obj = Construction.Create(item, pos);
                return obj.gameObject;
            }

            if (data is ColonistData)
            {
                ColonistData item = (ColonistData)data;
                Colonist obj = Colonist.Create(item, pos);
                return obj.gameObject;
            }

            if (data is NPCData)
            {
                NPCData item = (NPCData)data;
                NPC obj = NPC.Create(item, pos);
                return obj.gameObject;
            }

            if (data is CraftGroupData)
            {
                CraftGroupData group = (CraftGroupData)data;
                CraftData craft = group.GetRandomData();
                if (craft != null)
                    return Create(craft, pos);
            }

            return null;
        }

        public static GameObject Create(CSData data, Vector3 pos, Quaternion rot)
        {
            GameObject obj = Create(data, pos);
            if (obj != null)
                obj.transform.rotation = rot;
            return obj;
        }
    }
}
