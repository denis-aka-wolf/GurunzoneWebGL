using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Script that managages all Colonists and assign them to tasks
    /// </summary>

    public class ColonistManager : MonoBehaviour
    {
        private HashSet<Colonist> registered_colonists = new HashSet<Colonist>();
        private HashSet<Colonist> working_colonists = new HashSet<Colonist>();
        private Dictionary<Interactable, HashSet<Colonist>> assigned_colonists = new Dictionary<Interactable, HashSet<Colonist>>();

        private int idle_index = 0;
        private float update_timer = 0f;

        private static ColonistManager instance;

        void Awake()
        {
            instance = this;
        }

        void Update()
        {
            if (TheGame.Get().IsPaused())
                return;

            update_timer += Time.deltaTime;
            if (update_timer > 0.5f)
            {
                update_timer = 0f;
                SlowUpdate();
            }
        }

        private void SlowUpdate()
        {
            //Start new work
            foreach (Vector3 base_pos in GetBasePositions())
            {
                foreach (WorkBasic work in WorkBasic.GetAll())
                {
                    //Check if work has target
                    Interactable target = work.FindBestTarget(base_pos);
                    if (target != null)
                    {
                        if (target.Workable == null || target.Workable.CanBeAssigned())
                        {
                            //Find the best colonist for the work
                            Colonist colonist = FindBestColonist(work, target);
                            if (colonist != null)
                            {
                                StartWork(colonist, work, target);
                            }
                        }
                    }
                }
            }

            //Stop work
            foreach (Colonist colonist in Colonist.GetAll())
            {
                WorkBasic current_work = colonist.GetWork();
                Interactable work_target = colonist.GetWorkTarget();
                if (current_work != null)
                {
                    //Stop work
                    if (colonist.IsIdle() || !colonist.CanDoWork(current_work, work_target) || colonist.Attributes.IsAnyDepleted())
                        colonist.StopWork();
                    if(work_target != null && work_target.Workable != null && work_target.Workable.IsOverAssigned())
                        colonist.StopWork();
                }
            }
        }

        public void RegisterColonist(Colonist colonist)
        {
            //Add colonist to list and register events
            if (!registered_colonists.Contains(colonist))
            {
                registered_colonists.Add(colonist);
                colonist.onStartWork += OnStartWork;
                colonist.onStopWork += OnStopWork;
            }
        }

        public void UnregisterColonist(Colonist colonist)
        {
            //Remove colonist events
            if (registered_colonists.Contains(colonist))
            {
                registered_colonists.Remove(colonist);
                colonist.onStartWork -= OnStartWork;
                colonist.onStopWork -= OnStopWork;
            }
        }

        public void StartWork(Colonist colonist, WorkBasic work, Interactable target)
        {
            //Start working on a task
            if (colonist != null && work != null && target != null)
            {
                colonist.StartWork(work, target);
            }
        }

        private void OnStartWork(Colonist colonist, WorkBasic work)
        {
            AssignColonist(colonist, colonist.GetWorkTarget());
        }

        private void OnStopWork(Colonist colonist)
        {
            UnassignColonist(colonist);
        }

        private void AssignColonist(Colonist colonist, Interactable select)
        {
            //Assign colonist to selectable
            UnassignColonist(colonist);
            working_colonists.Add(colonist);
            if (select != null)
            {
                if (!assigned_colonists.ContainsKey(select))
                    assigned_colonists[select] = new HashSet<Colonist>();
                assigned_colonists[select].Add(colonist);
            }
        }

        private void UnassignColonist(Colonist colonist)
        {
            //Unlink colonist from all selectables
            if (working_colonists.Contains(colonist))
            {
                working_colonists.Remove(colonist);
                foreach (KeyValuePair<Interactable, HashSet<Colonist>> pair in assigned_colonists)
                {
                    if (pair.Value.Contains(colonist))
                        pair.Value.Remove(colonist);
                }
            }
        }

        public Colonist FindBestColonist(WorkBasic work, Interactable target)
        {
            Colonist best = null;
            float min_dist = work.range;
            foreach (Colonist colonist in Colonist.GetAll())
            {
                if (colonist.IsAuto() || colonist.IsIdle())
                {
                    if (!colonist.IsWorking() || work.priority > colonist.GetWork().priority)
                    {
                        float dist = (colonist.transform.position - target.Transform.position).magnitude;
                        if (dist < work.range && colonist.CanDoWork(work, target))
                        {
                            bool best_idle = best != null && best.IsIdle();
                            bool is_idle_better = colonist.IsIdle() || !best_idle; //Idle colonists priority over working ones
                            bool is_better = is_idle_better && dist < min_dist;

                            if (is_better)
                            {
                                min_dist = dist;
                                best = colonist;
                            }
                        }
                    }
                }
            }
            return best;
        }

        public Colonist GetNextIdle()
        {
            idle_index++;
            if (idle_index >= CountIdles())
                idle_index = 0;
            return GetIdle(idle_index);
        }

        public Colonist GetIdle(int index)
        {
            int aindex = 0;
            foreach (Colonist colonist in Colonist.GetAll())
            {
                if (colonist.IsActive() && colonist.IsIdle())
                {
                    if (index == aindex)
                        return colonist;
                    aindex++;
                }
            }
            return null;
        }

        public int CountIdles()
        {
            int count = 0;
            foreach (Colonist colonist in Colonist.GetAll())
            {
                if (colonist.IsActive() && colonist.IsIdle())
                    count++;
            }
            return count;
        }

        public List<Colonist> GetWorkingOn(Interactable target)
        {
            if (assigned_colonists.ContainsKey(target))
            {
                List<Colonist> colonists = new List<Colonist>(assigned_colonists[target]);
                return colonists;
            }
            return new List<Colonist>();
        }

        //Only count those who are assigned automatically with a WorkBasic action
        public int CountWorkingOn(Interactable target)
        {
            if (assigned_colonists.ContainsKey(target))
            {
                HashSet<Colonist> colonists = assigned_colonists[target];
                return colonists.Count;
            }
            return 0;
        }
        
        //Count how many are currently gathering a resource
        public int CountGathering(ItemData item)
        {
            int count = 0;
            foreach (Colonist colonist in Colonist.GetAll())
            {
                Interactable target = colonist.GetCurrentActionTarget();
                Gatherable gather = target?.Gatherable;
                if (gather != null && gather.GetItem() == item)
                {
                    count++;
                }
            }
            return count;
        }

        public int CountProducing(ItemData item)
        {
            int count = 0;
            foreach (Colonist colonist in Colonist.GetAll())
            {
                Interactable target = colonist.GetCurrentActionTarget();
                Factory factory = target?.Factory;
                if (factory != null && factory.GetSelected() == item)
                {
                    count++;
                }
            }
            return count;
        }

        public float GetAverageAttribute(AttributeType type)
        {
            int count = 0;
            float total = 0f;
            foreach (Colonist colonist in Colonist.GetAll())
            {
                if (!colonist.IsDead() && colonist.Attributes != null)
                {
                    total += colonist.Attributes.GetAttributeValue(type);
                    count += 1;
                }
            }
            if(count > 0)
                return total / (float)count;
            return 0f;
        }

        public bool IsAssigned(Colonist colonist)
        {
            return working_colonists.Contains(colonist);
        }

        public List<Vector3> GetBasePositions()
        {
            List<Vector3> list = new List<Vector3>();
            foreach (Headquarter hq in Headquarter.GetAll())
                list.Add(hq.transform.position);
            if (list.Count == 0)
                list.Add(Vector3.zero);
            return list;
        }

        public static ColonistManager Get()
        {
            if (instance == null)
                return FindObjectOfType<ColonistManager>();
            return instance;
        }
    }
}
