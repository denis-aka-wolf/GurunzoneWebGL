using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Main camera
    /// </summary>
    
    public class TheCamera : MonoBehaviour
    {
        public float move_accel = 50f;
        public float move_speed = 20f;
        public float move_drag_speed = 10f;
        public float rotate_speed = 90f;
        public float zoom_speed = 0.5f;
        public float zoom_in_max = 0.5f;
        public float zoom_out_max = 1f;
        public bool smooth_camera = true;

        private Camera cam;
        private Plane target_plane;

        private Vector3 velocity;
        private Vector3 rotated_offset;
        private Vector3 current_offset;
        private float current_rotate = 0f;
        private float current_zoom = 0f;
        private Vector3 target_pos;
        private Vector3 target_speed;
        private Transform cam_transform;
        

        private static TheCamera instance;

        void Awake()
        {
            instance = this;
            cam = GetComponent<Camera>();

            GameObject cam_target = new GameObject("CameraTarget");
            cam_transform = cam_target.transform;
            cam_transform.position = transform.position;
            cam_transform.rotation = transform.rotation;

            target_plane = new Plane(Vector3.up, 0f);
            Ray ray = new Ray(cam_transform.position, cam_transform.forward);
            target_plane.Raycast(ray, out float d);
            target_pos = ray.GetPoint(d);
            rotated_offset = cam_transform.position - target_pos;
            current_offset = rotated_offset;
        }

        void LateUpdate()
        {
            TheControls controls = TheControls.Get();
            TheControlsMobile mcontrols = TheControlsMobile.Get();

            if (!Application.isFocused)
                return;

            //move target
            Vector2 move = controls.GetMove() + controls.GetEdgeMove() + mcontrols.GetTouchMove();
            Vector3 cam_move = GetRotation() * new Vector3(move.x, 0f, move.y);
            target_speed = Vector3.MoveTowards(target_speed, cam_move,  move_accel * Time.deltaTime);
            float speed = (GetZoomPercent() + 0.5f) * move_speed;
            target_pos += target_speed * speed * Time.deltaTime;

            Vector2 scroll_move = controls.GetMouseDrag();
            Vector3 cam_scroll_move = GetRotation() * new Vector3(-scroll_move.x, 0f, -scroll_move.y);
            float scrool_speed = (GetZoomPercent() + 0.5f) * move_drag_speed;
            target_pos += cam_scroll_move * scrool_speed * Time.deltaTime;

            //Rotate
            float rotate = controls.GetRotateCam() + mcontrols.GetTouchRotate();
            current_rotate += rotate * -rotate_speed;

            //Zoom 
            if (controls.GetSelectMode() != SelectMode.Build && controls.IsInGameplay())
            {
                float zoom = controls.GetMouseScroll() + mcontrols.GetTouchZoom();
                current_zoom += zoom * zoom_speed; //Mouse scroll zoom
                current_zoom = Mathf.Clamp(current_zoom, -zoom_out_max, zoom_in_max);
            }

            UpdateCamera();
        }

        private void UpdateCamera()
        {
            rotated_offset = Quaternion.Euler(0, current_rotate * Time.deltaTime, 0) * rotated_offset;
            cam_transform.RotateAround(target_pos, Vector3.up, current_rotate * Time.deltaTime);
            current_offset = rotated_offset - rotated_offset * current_zoom;
            current_rotate = 0f;

            Vector3 tpos = target_pos + current_offset;
            cam_transform.position = tpos;

            //Move to target position
            if (smooth_camera)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, cam_transform.rotation, move_speed * Time.deltaTime);
                transform.position = Vector3.SmoothDamp(transform.position, cam_transform.position, ref velocity, 1f / move_speed);
            }
            else
            {
                transform.rotation = cam_transform.rotation;
                transform.position = cam_transform.position;
            }
        }
		
		public void Move(Vector3 dir)
		{
			target_pos += dir;
		}
		
		public void Rotate(float rotate)
		{
			current_rotate += rotate;
		}

        public void MoveToTarget(Vector3 pos)
        {
            Vector3 tpos = pos + current_offset;
            cam_transform.position = tpos;
            transform.position = tpos;
            target_pos = pos;
        }

        public void SmoothToTarget(Vector3 pos, float speed = 10f)
        {
            target_pos = Vector3.Lerp(target_pos, pos, speed * Time.deltaTime);
            transform.position = target_pos + current_offset;
            cam_transform.position = target_pos + current_offset;
        }

        public Vector3 GetTargetPos()
        {
            return target_pos;
        }

        public Quaternion GetRotation()
        {
            return Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }

        public Vector3 GetFacingFront()
        {
            Vector3 dir = transform.forward;
            dir.y = 0f;
            return dir.normalized;
        }

        public Vector3 GetFacingRight()
        {
            Vector3 dir = transform.right;
            dir.y = 0f;
            return dir.normalized;
        }

        public Quaternion GetFacingRotation()
        {
            Vector3 facing = GetFacingFront();
            return Quaternion.LookRotation(facing.normalized, Vector3.up);
        }

        public float GetZoomPercent()
        {
            return (current_zoom - zoom_in_max) / (-zoom_out_max - zoom_in_max);
        }

        public bool IsInside(Vector2 screen_pos)
        {
            return cam.pixelRect.Contains(screen_pos);
        }

        public static Camera GetCamera()
        {
            return instance.cam;
        }

        public static TheCamera Get()
        {
            if (instance == null)
                instance = FindObjectOfType<TheCamera>();
            return instance;
        }
    }
}
