using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Gurunzone
{
    [RequireComponent(typeof(Selectable))]
    public class TowerAttack : MonoBehaviour
    {
        public AttackTeam attack_team;      //Will it attack enemies or allies?
        public int attack_damage = 10;       //Basic damage 
        public float attack_range = 20f;   //How far can you attack
        public float attack_cooldown = 2f;  //Seconds of waiting in between each attack

        public Transform shoot_root;
        public GameObject projectile_prefab;

        private Selectable select;
        private float timer = 0f;

        private void Awake()
        {
            select = GetComponent<Selectable>();
        }

        private void Update()
        {
            if (TheGame.Get().IsPaused())
                return;

            float speed_mult = TheGame.Get().GetSpeedMultiplier();
            timer += speed_mult * Time.deltaTime;

            if (timer > attack_cooldown)
            {
                timer = 0f;
                ShootNearestEnemy();
            }
        }

        public void ShootNearestEnemy()
        {
            Destructible nearest = Destructible.GetNearest(attack_team, select.Destructible, transform.position, attack_range);
            Shoot(nearest);
        }

        public void Shoot(Destructible target)
        {
            if (target != null && projectile_prefab != null)
            {
                int damage = GetAttackDamage();
                Vector3 pos = GetShootPos();
                Vector3 dir = target.GetCenter() - pos;
                GameObject proj = Instantiate(projectile_prefab, pos, Quaternion.LookRotation(dir.normalized, Vector3.up));
                Projectile project = proj.GetComponent<Projectile>();
                project.target = target;
                project.shooter = select;
                project.dir = dir.normalized;
                project.damage = damage;
            }
        }

        public Vector3 GetShootPos()
        {
            if (shoot_root != null)
                return shoot_root.position;
            return transform.position + Vector3.up * 2f;
        }

        public int GetAttackDamage()
        {
            int damage = attack_damage;
            float mult = 1f + TechManager.Get().GetTechBonus(BonusType.AttackPercent, select);
            return Mathf.RoundToInt(damage * mult + TechManager.Get().GetTechBonus(BonusType.AttackValue, select));
        }
    }
}
