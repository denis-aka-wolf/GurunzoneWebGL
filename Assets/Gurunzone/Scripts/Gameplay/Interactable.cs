using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gurunzone
{
    /// <summary>
    /// Object that characters can interact with
    /// </summary>

    [RequireComponent(typeof(Selectable))]
    public class Interactable : MonoBehaviour
    {
        [Header("Interaction")]
        public float use_range = 5f;        //Distance from which characters can interact with this object
        public int use_max = 8;           //Maximum number of characters who can interact with this object

        [Header("Actions")]
        public ActionBasic[] actions;       //Default actions when interacting with this Interactable (right clicking on it)

        [Header("Interaction Point")]
        public Transform[] interact_points;  //Optional, if you want the interaction point to not be the center


        public UnityAction<Character> onTarget;
        public UnityAction<Character> onInteract;

        private Selectable select;
        private Destructible destruct; //May be null
        private Workable workable; //May be null
        private Gatherable gather; //May be null
        private Factory factory; //May be null
        private UniqueID uid; //May be null
        private Transform transf;

        private static List<Interactable> interact_list = new List<Interactable>();

        void Awake()
        {
            interact_list.Add(this);
            select = GetComponent<Selectable>();
            destruct = GetComponent<Destructible>();
            workable = GetComponent<Workable>();
            gather = GetComponent<Gatherable>();
            factory = GetComponent<Factory>();
            uid = GetComponent<UniqueID>();
            transf = transform;
        }

        private void OnDestroy()
        {
            interact_list.Remove(this);
        }

        void Update()
        {

        }

        public void Interact(Character character)
        {
            onInteract?.Invoke(character);
        }
        
        public void Target(Character character)
        {
            select.DoSelectFX();
            onTarget?.Invoke(character);
        }

        public bool IsInRange(Vector3 pos)
        {
            return IsInRange(pos, use_range);
        }

        public bool IsInRange(Vector3 pos, float range)
        {
            float dist = (pos - transf.position).magnitude;
            return dist <= range;
        }

        public bool IsInteractFull()
        {
            int nb = Character.CountTargetingTarget(this);
            return nb >= use_max;
        }

        public bool IsInUseRange(Vector3 pos)
        {
            Vector3 select_pos = transf.position;
            float dist = (select_pos - pos).magnitude;
            return dist <= use_range;
        }

        public Vector3 GetInteractCenter()
        {
            if (interact_points.Length > 0)
                return interact_points[0].position;
            return transf.position;
        }

        public Vector3 GetInteractCenter(int index)
        {
            if (interact_points.Length > 0 && index >= 0)
            {
                int interact_point = index % interact_points.Length;
                return interact_points[interact_point].position;
            }
            return transf.position;
        }

        public Vector3 GetInteractPosition(LayerMask layers, int index)
        {
            Vector3 center = GetInteractCenter(index);
            float angle = (index * 360f / use_max) * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            Vector3 pos = center + dir * use_range;
            PhysicsTool.FindGroundPosition(pos, layers, out Vector3 gpos);
            return gpos;
        }

        public List<Vector3> GetInteractPositions(LayerMask layers)
        {
            List<Vector3> positions = new List<Vector3>();
            for (int i = 0; i < use_max; i++)
            {
                Vector3 pos = GetInteractPosition(layers, i);
                positions.Add(pos);
            }
            return positions;
        }

        public int GetInteractPositionIndex(Character character, LayerMask layers)
        {
            List<Character> icharacters = Character.GetAllTargeting(this);
            List<Vector3> positions = GetInteractPositions(layers);
            HashSet<int> interact_index = new HashSet<int>();
            int nearest = 0;
            float min_dist = 99999f;
            foreach (Character acharacter in icharacters)
            {
                if (acharacter != character)
                    interact_index.Add(acharacter.GetTargetPosIndex());
            }
            for (int i = 0; i < positions.Count; i++)
            {
                if (!interact_index.Contains(i))
                {
                    float dist = (character.transform.position - positions[i]).magnitude;
                    if (dist < min_dist)
                    {
                        min_dist = dist;
                        nearest = i;
                    }
                }
            }
            return nearest;
        }

        public Selectable Selectable { get { return select; } }
        public Destructible Destructible { get { return destruct; } }
        public Gatherable Gatherable { get { return gather; } } //May be null
        public Workable Workable { get { return workable; } } //May be null
        public Factory Factory { get { return factory; } } //May be null
        public Transform Transform { get { return transf; } } //May be null
        public string UID { get { return uid != null ? uid.uid : ""; } }

        public static Interactable Get(string uid)
        {
            foreach (Interactable select in interact_list)
            {
                if (select.UID == uid)
                    return select;
            }
            return null;
        }

        public static List<Interactable> GetAll()
        {
            return interact_list;
        }
        }
}
