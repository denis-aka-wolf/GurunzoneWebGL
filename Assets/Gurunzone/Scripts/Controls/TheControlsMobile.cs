using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Gurunzone
{
    /// <summary>
    /// Controls manager for mobile touch camera (2 fingers)
    /// </summary>

    public class TheControlsMobile : MonoBehaviour
    {
        public float move_speed = 20f;
        public float rotate_speed = 10f;
        public float zoom_speed = 10f;

        private bool is_cam_mode = false;
        private Vector2 pan_value = Vector2.zero;
        private float zoom_value = 0f;
        private float rotate_value = 0f;

        private Vector2 prev_touch1 = Vector2.zero;
        private Vector2 prev_touch2 = Vector2.zero;

        private static TheControlsMobile instance = null;

        void Awake()
        {
            instance = this;
        }

        void Update()
        {
            if (!TheGame.IsMobile())
                return;

            TheControls controls = TheControls.Get();

            pan_value = Vector2.zero;
            zoom_value = 0f;
            rotate_value = 0f;

            if (Input.touchCount == 2)
            {
                Vector2 pos1 = Input.GetTouch(0).position;
                Vector2 pos2 = Input.GetTouch(1).position;
                if (is_cam_mode)
                {
                    //Pan
                    Vector2 dir1 = pos1 - prev_touch1;
                    Vector2 dir2 = pos2 - prev_touch2;
                    float dot = Vector2.Dot(dir1.normalized, dir2.normalized);
                    Vector2 dir = Mathf.Clamp01(dot) * (dir1 + dir2) / 2f;
                    if(dir1.magnitude > dir2.magnitude * 0.5f && dir2.magnitude > dir1.magnitude * 0.5f)
                        pan_value += -dir / (float)Screen.height;

                    //Zoom
                    float distance = Vector2.Distance(pos1, pos2);
                    float prev_distance = Vector2.Distance(prev_touch1, prev_touch2);
                    zoom_value = (distance - prev_distance) / (float)Screen.height;

                    //Rotate
                    var pDir = prev_touch2 - prev_touch1;
                    var cDir = pos2 - pos1;
                    float angle = Vector2.SignedAngle(pDir, cDir);
                    rotate_value = Mathf.Clamp(angle, -45f, 45f) * -Mathf.Deg2Rad;
                }
                prev_touch1 = pos1;
                prev_touch2 = pos2;
                is_cam_mode = true; //Wait one frame to make sure distance has been calculated once
            }
            else
            {
                is_cam_mode = false;
            }

        }

        public int CountActiveTouch()
        {
            return Input.touchCount;
        }

        public bool IsDoubleTouch()
        {
            return Input.touchCount >= 2;
        }
        
        public Vector2 GetTouchMove()
        {
            return pan_value * move_speed;
        }

        public float GetTouchZoom()
        {
            return zoom_value * zoom_speed;
        }

        public float GetTouchRotate()
        {
            return rotate_value * rotate_speed;
        }

        public static TheControlsMobile Get()
        {
            return instance;
        }
    }

}