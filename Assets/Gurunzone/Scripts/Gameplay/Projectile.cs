using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// A projectile shot with a ranged weapon (This script is not implemented yet)
    /// </summary>

    public class Projectile : MonoBehaviour
    {
        public int damage = 1;
        public float speed = 20f;
        public float duration = 20f;
        public float gravity = 0.1f;

        public AudioClip shoot_sound;

        [HideInInspector]
        public Vector3 dir;

        [HideInInspector]
        public Selectable shooter;

        [HideInInspector]
        public Character shooter_character;

        [HideInInspector]
        public Destructible target;

        private Vector3 curve_dir = Vector3.zero;
        private float curve_dist = 0f;
        private float timer = 0f;

        void Start()
        {
            TheAudio.Get().PlaySFX3D("projectile", shoot_sound, transform.position);
        }

        void Update()
        {
            if (TheGame.Get().IsPaused())
                return;

            if (curve_dist > 0.01f && (timer * speed) < curve_dist)
            {
                //Initial curved dir (only in freelook mode)
                float value = Mathf.Clamp01(timer * speed / curve_dist);
                Vector3 cdir = (1f - value) * curve_dir + value * dir;
                transform.position += cdir * speed * Time.deltaTime;
                transform.rotation = Quaternion.LookRotation(cdir.normalized, Vector2.up);
            }
            else
            {
                //Regular dir
                transform.position += dir * speed * Time.deltaTime;
                dir += gravity * Vector3.down * Time.deltaTime;
                transform.rotation = Quaternion.LookRotation(dir.normalized, Vector2.up);
            }

            timer += Time.deltaTime;
            if (timer > duration)
                Destroy(gameObject);
        }

        public void SetInitialCurve(Vector3 dir, float dist = 10f)
        {
            curve_dir = dir;
            curve_dist = dist * 1.25f; //Add offset for more accuracy
        }

        private void OnTriggerEnter(Collider collision)
        {
            Destructible destruct = collision.GetComponent<Destructible>();
            bool right_target = (target == destruct || target == null);
            if (destruct != null && right_target)
            {
                if(shooter_character != null)
                    destruct.TakeDamage(shooter_character, damage);
                else
                    destruct.TakeDamage(shooter, damage);
                Destroy(gameObject);
            }

        }
    }

}