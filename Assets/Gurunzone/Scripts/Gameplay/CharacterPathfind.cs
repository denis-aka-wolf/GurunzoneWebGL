using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Pathfinding functions for character movement
    /// </summary>
    
    public class CharacterPathfind : MonoBehaviour
    {
        public float refresh_rate = 0.5f;   //Calculate path every X sec
        public float path_threshold = 0.5f; //How far from a path point before changing to the next
        public float path_dist_max = 10f;  //Max distance to find a path from, if you click outside the the navmesh, it will only find a path if there is a navmesh within this range
        public bool force_navmesh = false;  //If true, character will never move outside of the navmesh
        public bool debug = false;          //Show the path in the editor scene view, for debuging

        private bool has_target = false;
        private bool has_path = false;
        private Vector3 move_target;
        private Vector3 move_target_next;

        private Vector3[] nav_paths = new Vector3[0];
        private Vector3 path_target;
        private Vector3 path_target_calculated;
        private int path_index = 0;
        private bool calculating_path = false;
        private float navmesh_timer = 0f;

        private void Awake()
        {
            move_target = transform.position;
            move_target_next = transform.position;
            path_target = transform.position;
            path_target_calculated = transform.position;
            navmesh_timer = refresh_rate + 1f;
        }

        private void Update()
        {
            if (TheGame.Get().IsPaused())
                return;

            navmesh_timer += Time.deltaTime;

            //Navmesh
            CheckIncreasePath();
            
            if (has_target && has_path)
            {
                Vector3 path_dir = path_target - move_target;
                float dist = path_dir.magnitude;
                if (dist > path_threshold)
                    CalculateNavmesh();
            }

            //Stop
            if (has_path && HasReachedTarget())
                Stop();

            if (debug && has_path)
            {
                Vector3 prev = transform.position;
                foreach (Vector3 pos in nav_paths)
                {
                    Debug.DrawLine(prev, pos);
                    prev = pos;
                }
            }
        }

        public void MoveTo(Vector3 pos)
        {
            has_target = true;
            move_target = pos;
            CalculateNavmesh();
        }

        public void UpdateMoveTo(Vector3 pos)
        {
            has_target = true;
            move_target = pos;
        }

        public void Stop()
        {
            has_target = false;
            has_path = false;
            calculating_path = false;
            move_target = transform.position;
            move_target_next = transform.position;
        }

        private void CalculateNavmesh()
        {
            if (!calculating_path)
            {
                calculating_path = true;
                path_index = 0;
                path_target = move_target;
                path_target_calculated = move_target;
                navmesh_timer = 0f;
                int walkable = (1 << 0); //Walkable is layer 0, so bit #0 is set to 1, for other layers, replace the 0 by the layer index
                NavMeshTool.CalculatePath(transform.position, move_target, walkable, path_dist_max, FinishCalculateNavmesh);
            }
        }

        private void FinishCalculateNavmesh(NavMeshToolPath path)
        {
            calculating_path = false;
            has_path = path.success;
            nav_paths = path.path;
            path_target_calculated = path.GetDestination();
            path_index = 0;
            navmesh_timer = 0f;
            CheckIncreasePath();
        }

        private void CheckIncreasePath()
        {
            if (has_target && has_path && path_index < nav_paths.Length)
            {
                move_target_next = nav_paths[path_index];
                while ((path_index+1) < nav_paths.Length && HasReachedNextTarget())
                {
                    path_index++;
                    move_target_next = nav_paths[path_index];
                }
            }
        }

        public bool HasTarget()
        {
            return has_target;
        }

        public bool HasPath()
        {
            return has_target && has_path;
        }

        public bool IsCalculating()
        {
            return calculating_path;
        }

        public bool HasFailed()
        {
            return has_target && !calculating_path && !has_path;
        }

        public bool HasReachedNextTarget()
        {
            Vector3 dir_total = move_target_next - transform.position;
            return dir_total.magnitude < path_threshold;
        }

        public bool HasReachedTarget()
        {
            Vector3 dir_total = path_target_calculated - transform.position;
            return dir_total.magnitude < path_threshold;
        }

        public bool IsTargetUnreachable(float max_offset = 1f)
        {
            if (HasTarget() && !IsCalculating())
            {
                float path_offset = GetTargetOffset();
                return path_offset > max_offset;
            }
            return false;
        }

        public Vector3 GetNextTarget()
        {
            return move_target_next; //Next position in the path
        }

        public Vector3 GetFinalTarget()
        {
            return path_target; //Final target selected ("to" position when calling the pathfinding)
        }

        public Vector3 GetFinalDestination()
        {
            return path_target_calculated; //Final target calculated (last point in the calculated path)
        }

        public float GetTargetOffset()
        {
            Vector3 target = GetFinalTarget();
            Vector3 dest = GetFinalDestination();
            float dist = (target - dest).magnitude;
            return dist; //Distance between the selected destination and the calculated destination
        }

    }
}
