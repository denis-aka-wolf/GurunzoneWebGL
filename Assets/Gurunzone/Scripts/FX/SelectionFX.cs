using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Selection Sprite
    /// </summary>

    public class SelectionFX : MonoBehaviour
    {
        private SpriteRenderer render;

        void Awake()
        {
            render = GetComponentInChildren<SpriteRenderer>();
            render.enabled = false;
        }

        void Update()
        {
            TheControls controls = TheControls.Get();
            render.enabled = controls.IsSelecting();
            Vector3 p1 = controls.GetSelectionStart();
            Vector3 p2 = controls.GetSelectionEnd();
            Vector3 dir = p2 - p1;
            Vector3 center = (p1 + p2) / 2f;

            Quaternion rot = TheCamera.Get().GetRotation();
            dir = Quaternion.Inverse(rot) * dir;

            transform.position = center;
            transform.rotation = rot;
            transform.localScale = new Vector3(dir.x, 1f, dir.z);

        }
    }
}
