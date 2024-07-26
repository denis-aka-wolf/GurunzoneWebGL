using UnityEngine;
using UnityEngine.Events;

namespace Gurunzone
{
    /// <summary>
    /// Animal wandering behavior script
    /// </summary>

    [RequireComponent(typeof(Character))]
    public class Wander : MonoBehaviour
    {
        [Header("Wander")]
        public float wander_range = 10f;    //How far from the starting pos can it wander
        public float wander_interval = 10f; //Interval between changing wander position

        private Character character;
        private Selectable selectable;
        private Destructible destruct;
        private Animator animator;

        private Vector3 start_pos;
        private Vector3 wander_target;
        private float state_timer = 0f;

        void Awake()
        {
            character = GetComponent<Character>();
            selectable = GetComponent<Selectable>();
            destruct = GetComponent<Destructible>(); //Can be null
            animator = GetComponentInChildren<Animator>(); //Can be null
            start_pos = transform.position;
            state_timer = 99f; //Find wander right away
            transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        }

        void Start()
        {
            if (destruct != null)
                destruct.onDeath += OnDeath;
        }

        private void Update()
        {
            //Animations
            bool paused = TheGame.Get().IsPaused();
            if (animator != null)
                animator.enabled = !paused;

            if (TheGame.Get().IsPaused())
                return;

            float mult = TheGame.Get().GetSpeedMultiplier();
            state_timer += mult * Time.deltaTime;

            if (state_timer > wander_interval)
            {
                state_timer = Random.Range(-1f, 1f);
                FindWanderTarget();
                character.MoveTo(wander_target);
            }

            //Animations
            if (animator != null && animator.enabled)
            {
                animator.SetBool("move", IsMoving());
            }
        }

        private void FindWanderTarget()
        {
            float range = Random.Range(0f, wander_range);
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 spos = start_pos;
            Vector3 pos = spos + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * range;
            wander_target = pos;
        }

        private void OnDeath()
        {
            if (animator != null)
                animator.SetTrigger("death");
        }

        public bool IsDead()
        {
            return character.IsDead();
        }

        public bool IsMoving()
        {
            return character.IsMoving();
        }
    }

}
