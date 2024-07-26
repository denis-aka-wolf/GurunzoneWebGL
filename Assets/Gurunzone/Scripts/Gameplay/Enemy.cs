using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gurunzone
{
    public enum EnemyState
    {
        Wander = 0,
        Alerted = 2,
        Attack = 5,
        MoveTo = 10,
        Dead = 20,
    }

    public enum EnemyBehavior
    {
        None = 0,   //Custom behavior
        Defensive = 10, //Attack when attacked
        Aggressive = 20, //Attack on sight
    }

    public enum WanderBehavior
    {
        None = 0, //Dont wander
        WanderNear = 10, //Will wander near starting position
        WanderFar = 20, //Will wander beyond starting position
    }

    /// <summary>
    /// Give orders to the character to attack constructions and colonists
    /// </summary>

    [RequireComponent(typeof(Character))]
    [RequireComponent(typeof(CharacterAttack))]
    public class Enemy : MonoBehaviour
    {
        [Header("Behavior")]
        public WanderBehavior wander = WanderBehavior.WanderNear;
        public EnemyBehavior behavior = EnemyBehavior.Aggressive;

        [Header("Move")]
        public float walk_speed = 10f;
        public float run_speed = 10f;
        public float wander_range = 20f;
        public float wander_interval = 10f;

        [Header("Vision")]
        public float detect_range = 20f;
        public float detect_angle = 360f;
        public float detect_360_range = 2f;
        public float follow_range = 100f;
        public float reaction_time = 0.5f; //How fast it detects threats

        public UnityAction onAttack;
        public UnityAction onDamaged;
        public UnityAction onDeath;

        private EnemyState state;
        private Selectable select;
        private Character character;
        private Destructible destruct;
        private Animator animator;

        private Interactable attack_target = null;
        private Vector3 wander_target;
        private Vector3 start_pos;

        private bool is_running = false;
        private float state_timer = 0f;
        private bool force_action = false;
        private float update_timer = 0f;

        private static List<Enemy> enemies_list = new List<Enemy>();

        void Awake()
        {
            enemies_list.Add(this);
            select = GetComponent<Selectable>();
            character = GetComponent<Character>();
            destruct = GetComponent<Destructible>();
            animator = GetComponentInChildren<Animator>();
            start_pos = transform.position;
            state_timer = 99f; //Find wander right away
            update_timer = Random.Range(-1f, 1f);

            if (wander != WanderBehavior.None)
                transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        }

        void OnDestroy()
        {
            enemies_list.Remove(this);
        }

        void Start()
        {
            character.Attack.onAttack += OnAttack;
            destruct.onDamaged += OnDamaged;
            destruct.onDamagedBy += OnDamagedBy;
            destruct.onDeath += OnDeath;
        }

        private void Update()
        {
            if (TheGame.Get().IsPaused())
                return;

            if (state == EnemyState.Dead || behavior == EnemyBehavior.None)
                return;

            state_timer += Time.deltaTime;
            is_running = state == EnemyState.Attack;
            character.move_speed = is_running ? run_speed : walk_speed;

            //States
            if (state == EnemyState.Wander)
            {
                if (state_timer > wander_interval && wander != WanderBehavior.None)
                {
                    state_timer = Random.Range(-1f, 1f);
                    FindWanderTarget();
                    character.MoveTo(wander_target);
                }
            }

            if (state == EnemyState.Alerted)
            {
                if (attack_target == null)
                {
                    character.Stop();
                    ChangeState(EnemyState.Wander);
                    return;
                }

                character.FaceToward(attack_target.transform.position);

                if (state_timer > reaction_time)
                {
                    ReactToThreat();
                }
            }

            if (state == EnemyState.Attack)
            {
                if (attack_target == null || !character.IsAttackMode())
                {
                    StopAction();
                    return;
                }
            }

            if (state == EnemyState.MoveTo)
            {
                if (character.HasReachedTarget())
                    StopAction();
            }

            update_timer += Time.deltaTime;
            if (update_timer > 0.5f)
            {
                update_timer = Random.Range(-0.1f, 0.1f);
                SlowUpdate(); //Optimization
            }

            //Animations
            if (animator != null && animator.enabled)
            {
                animator.SetBool("move", IsMoving());
                animator.SetBool("run", IsRunning());
            }
        }

        private void SlowUpdate()
        {
            if (state == EnemyState.Wander)
            {
                //These behavior trigger a reaction on sight, while the "Defense" behavior only trigger a reaction when attacked
                if (behavior == EnemyBehavior.Aggressive)
                {
                    DetectThreat(detect_range);

                    if (attack_target != null)
                    {
                        character.Stop();
                        ChangeState(EnemyState.Alerted);
                    }
                }
            }

            if (state == EnemyState.Attack)
            {
                //Change attack target
                if (!force_action && !character.Attack.IsAttacking() && state_timer > 5f)
                {
                    DetectThreat(detect_range);
                    ReactToThreat();
                }

                //Stop action if too far
                if (!force_action && attack_target != null && state_timer > 5f)
                {
                    Vector3 targ_dir = attack_target.transform.position - transform.position;
                    Vector3 start_dir = start_pos - transform.position;

                    if (targ_dir.magnitude > detect_range || start_dir.magnitude > follow_range)
                    {
                        StopAction();
                    }
                }
            }
        }

        //Detect if the player is in vision
        private void DetectThreat(float range)
        {
            Vector3 pos = transform.position;
            float min_dist = range;

            //React to other characters/destructibles
            foreach (Interactable interact in Interactable.GetAll())
            {
                //Find destructible to attack
                if (HasAttackBehavior() && interact != select.Interactable)
                {
                    Destructible destruct = interact.Destructible;
                    if (destruct != null && destruct.CanBeAttacked() && (destruct.target_team == AttackTeam.Ally || destruct.target_team == AttackTeam.Enemy)) //Attack by default (not neutral)
                    {
                        if (destruct.target_team == AttackTeam.Ally || destruct.target_group != this.destruct.target_group) //Is not in same team
                        {
                            Vector3 dir = interact.GetInteractCenter() - pos;
                            if (dir.magnitude < min_dist)
                            {
                                float dangle = detect_angle / 2f; // /2 for each side
                                float angle = Vector3.Angle(transform.forward, dir.normalized);
                                if (angle < dangle || dir.magnitude < detect_360_range)
                                {
                                    attack_target = interact;
                                    min_dist = dir.magnitude;
                                }
                            }
                        }
                    }
                }
            }
        }

        //React to player if seen by enemy
        private void ReactToThreat()
        {
            if (IsDead())
                return;

            if (attack_target != null && HasAttackBehavior())
            {
                ChangeState(EnemyState.Attack);
                character.AttackTarget(attack_target);
            }
            else
            {
                ChangeState(EnemyState.Wander);
            }
        }

        private void FindWanderTarget()
        {
            float range = Random.Range(0f, wander_range);
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 spos = wander == WanderBehavior.WanderFar ? transform.position : start_pos;
            Vector3 pos = spos + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * range;
            wander_target = pos;
        }

        public void AttackTarget(Interactable target)
        {
            if (target != null)
            {
                ChangeState(EnemyState.Attack);
                this.attack_target = target;
                force_action = true;
                character.AttackTarget(target);
            }
        }

        public void MoveToTarget(Vector3 pos, bool run)
        {
            is_running = run;
            force_action = true;
            ChangeState(EnemyState.MoveTo);
            character.MoveTo(pos);
        }

        public void StopAction()
        {
            character.Stop();
            is_running = false;
            force_action = false;
            attack_target = null;
            ChangeState(EnemyState.Wander);
        }

        public void ChangeState(EnemyState state)
        {
            this.state = state;
            state_timer = 0f;
        }

        private void OnDamaged()
        {
            if (IsDead())
                return;

            if (onDamaged != null)
                onDamaged.Invoke();

            if (animator != null)
                animator.SetTrigger("damaged");
        }

        private void OnDamagedBy(Selectable attacker)
        {
            if (IsDead() || state_timer < 2f)
                return;

            if (attacker != null && attacker.Interactable != null)
            {
                attack_target = attacker.Interactable;
                ReactToThreat();
            }
        }

        private void OnDeath()
        {
            state = EnemyState.Dead;

            if (onDeath != null)
                onDeath.Invoke();

            if (animator != null)
                animator.SetTrigger("death");
        }

        void OnAttack(Destructible target)
        {
            if (animator != null)
                animator.SetTrigger("attack");
        }

        public bool HasAttackBehavior()
        {
            return behavior == EnemyBehavior.Aggressive || behavior == EnemyBehavior.Defensive;
        }
        
        public bool IsDead()
        {
            return character.IsDead();
        }

        public bool IsMoving()
        {
            return character.IsMoving();
        }

        public bool IsRunning()
        {
            return character.IsMoving() && is_running;
        }

        public string GetUID()
        {
            return character.UID;
        }

        public Selectable Selectable { get { return select; } }
        public Character Character { get { return character; } }

        public static List<Enemy> GetAll()
        {
            return enemies_list;
        }
    }
}
