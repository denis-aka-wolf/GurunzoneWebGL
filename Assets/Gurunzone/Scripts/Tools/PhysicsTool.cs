using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Generic physics functions
    /// </summary>

    public class PhysicsTool
    {
        //Detect if object is grounded, the ground normal, and ground distance from root
        public static bool DetectGround(Transform root, Vector3 center, float hdist, LayerMask ground_layer)
        {
            Vector3 p1 = center;

            RaycastHit h1;
            bool f1 = Physics.Raycast(p1, Vector3.down, out h1, hdist, ground_layer.value, QueryTriggerInteraction.Ignore);
            f1 = f1 && h1.collider.transform != root && !h1.collider.transform.IsChildOf(root);
            return f1;
        }

        public static int[] DetectGroundLayers(Vector3 center, float hdist)
        {
            Vector3 p1 = center;
            RaycastHit[] hits = Physics.RaycastAll(p1, Vector3.down, hdist, ~0, QueryTriggerInteraction.Ignore);
            int[] layers = new int[hits.Length];

            for (int i = 0; i < hits.Length; i++)
                layers[i] = hits[i].collider.gameObject.layer;

            return layers;
        }

        public static Vector3 RaycastFloorPos(Vector3 pos, Vector3 dir, LayerMask floor)
        {
            RaycastHit hit;
            Ray ray = new Ray(pos, dir);
            bool found = Physics.Raycast(pos, dir.normalized, out hit, dir.magnitude, floor.value, QueryTriggerInteraction.Ignore);
            if (found)
                return hit.point;
            Plane plane = new Plane(Vector3.up, 0f);
            bool pfound = plane.Raycast(ray, out float d);
            if (pfound)
                return ray.GetPoint(d);
            return Vector3.zero;
        }

        public static bool RaycastFloorPos(Vector3 pos, Vector3 dir, LayerMask floor, out Vector3 fpos)
        {
            RaycastHit hit;
            fpos = pos;
            bool found = Physics.Raycast(pos, dir.normalized, out hit, dir.magnitude, floor.value, QueryTriggerInteraction.Ignore);
            if (found)
                fpos = hit.point;
            return found;
        }

        public static bool FindGroundPosition(Vector3 pos, out Vector3 ground_pos, float max_y = 999f)
        {
            return FindGroundPosition(pos, ~0, out ground_pos, max_y); //All layers
        }

        //Find the elevation of the ground at position (within max_y dist)
        public static bool FindGroundPosition(Vector3 pos, LayerMask ground_layer, out Vector3 ground_pos, float max_y = 999f)
        {
            Vector3 start_pos = pos + Vector3.up * max_y;
            RaycastHit rhit;
            bool is_hit = Physics.Raycast(start_pos, Vector3.down, out rhit, max_y * 2f, ground_layer, QueryTriggerInteraction.Ignore);
            bool is_collider = is_hit && rhit.collider != null;
            ground_pos = rhit.point;
            return is_hit && is_collider; 
        }

        //Find ground position only if no obstacles on top of it
        public static bool FindGroundPositionObstacle(Vector3 pos, LayerMask ground_layer, out Vector3 ground_pos, bool ignore_trigger = true, float max_y = 999f)
        {
            Vector3 start_pos = pos + Vector3.up * max_y;
            RaycastHit rhit;
            bool is_hit = Physics.Raycast(start_pos, Vector3.down, out rhit, max_y * 2f, ~0, ignore_trigger ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide);
            bool is_in_right_layer = is_hit && rhit.collider != null && IsLayerInLayerMask(rhit.collider.gameObject.layer, ground_layer.value);
            ground_pos = rhit.point;
            return is_hit && is_in_right_layer;
        }

        public static Vector3 FlipNormalUp(Vector3 normal)
        {
            if (normal.y < 0f)
                return -normal; //Face up
            return normal;
        }

        public static bool RaycastCollisionLayer(Vector3 pos, Vector3 dir, LayerMask layer, out RaycastHit hit) {
            //Debug.DrawRay(pos, dir);
            return Physics.Raycast(pos, dir.normalized, out hit, dir.magnitude, layer.value, QueryTriggerInteraction.Ignore);
        }

        public static bool RaycastCollision(Vector3 pos, Vector3 dir, out RaycastHit hit){
            //Debug.DrawRay(pos, dir);
            return Physics.Raycast(pos, dir.normalized, out hit, dir.magnitude, ~0, QueryTriggerInteraction.Ignore);
        }

        public static bool IsLayerInLayerMask(int layer, LayerMask mask)
        {
            return (LayerToLayerMask(layer).value & mask.value) > 0;
        }

        public static bool IsAnyLayerInLayerMask(int[] layers, LayerMask mask)
        {
            bool is_in_layer = false;
            for (int i = 0; i < layers.Length; i++)
                is_in_layer = is_in_layer || IsLayerInLayerMask(layers[i], mask);
            return is_in_layer;
        }

        public static LayerMask LayerToLayerMask(int layer)
        {
            return (LayerMask) 1 << layer;
        }

        public static List<int> LayerMaskToLayers(LayerMask mask)
        {
            uint bits = (uint)mask.value;
            List<int> layers = new List<int>();
            for (int i = 31; bits > 0; i--)
            {
                if ((bits >> i) > 0)
                {
                    bits = (bits << (32 - i)) >> (32 - i);
                    layers.Add(i);
                }
            }
            return layers;
        }
    }

}
