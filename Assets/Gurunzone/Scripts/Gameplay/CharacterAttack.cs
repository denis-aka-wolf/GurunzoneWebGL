using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gurunzone
{
    public enum AttackType
    {
        None = 0,
        Melee = 5,
        Ranged = 10,
    }

    /// <summary>
    /// Class to allow a character to attack, can target a Destructible or CharacterDestructible
    /// </summary>

    [RequireComponent(typeof(Character))]
    public class CharacterAttack : MonoBehaviour
    {
        public AttackType attack_type = AttackType.Melee;
        public int attack_damage = 5;       //Basic damage without equipment
        public float attack_range = 1.2f;   //How far can you attack (melee)
        public float attack_cooldown = 1f;  //Seconds of waiting in between each attack
        public float attack_windup = 0.7f;  //Timing (in secs) between the start of the attack and the hit
        public float attack_windout = 0.4f; //Timing (in secs) between the hit and the end of the attack
        //public float attack_energy = 1f;    //Energy cost to attack (not implemented yet)
        public GameObject default_projectile; //Default projectile prefab (ranged attack only)

        public UnityAction<Destructible> onAttack;
        public UnityAction<Destructible> onAttackHit;

        private Character character;
        private Selectable select;
        private Destructible destruct; //Can be null

        private Coroutine attack_routine = null;
        private float attack_timer = 0f;
        private bool is_attacking = false;

        void Awake()
        {
            character = GetComponent<Character>();
            select = GetComponent<Selectable>();
            destruct = GetComponent<Destructible>();
        }

        void Update()
        {
            if (TheGame.Get().IsPaused())
                return;

            if (IsDead() || character.IsWaiting())
                return;

            //Attack when target is in range
            float mult = TheGame.Get().GetSpeedMultiplier();
            attack_timer += Time.deltaTime * mult;

        }

        public void Kill()
        {
            select.Kill();
        }

        //Perform one attack
        public void AttackStrike(Destructible target)
        {
            if (!character.IsWaiting() && CanAttack())
            {
                attack_timer = -10f; //Willbe set to 0f after the strike
                attack_routine = StartCoroutine(AttackRun(target));
            }
        }

        //Melee or ranged targeting one target
        private IEnumerator AttackRun(Destructible target)
        {
            character.Wait();
            is_attacking = true;
            
            //Start animation
            if (onAttack != null)
                onAttack.Invoke(target);

            //Face target
            character.FaceToward(target.transform.position);

            //Wait for windup
            float windup = GetAttackWindup();
            yield return new WaitForSeconds(windup);

            DoAttackStrike(target);

            //Reset timer
            attack_timer = 0f;

            //Wait for the end of the attack before character can move again
            float windout = GetAttackWindout();
            yield return new WaitForSeconds(windout);

            character.StopWait();
            is_attacking = false;
        }

        private void DoAttackStrike(Destructible target)
        {
            if (target == null)
                return;

            //Ranged attack
            bool is_ranged = attack_type == AttackType.Ranged || HasRangedWeapon();
            ItemEquipData equipped = GetBestWeapon();
            ItemProjData projectile = GetRangedProjectile(equipped);
            GameObject proj_default = GetDefaultProjectile();
            if (is_ranged && (projectile != null || proj_default != null))
            {
                int damage = GetAttackDamage(target);
                if (projectile != null)
                    damage += projectile.damage_bonus;

                Vector3 pos = GetProjectileSpawnPos();
                Vector3 dir = target.GetCenter() - pos;
                GameObject prefab = projectile != null ? projectile.projectile_prefab : proj_default;
                GameObject proj = Instantiate(prefab, pos, Quaternion.LookRotation(dir.normalized, Vector3.up));
                Projectile project = proj.GetComponent<Projectile>();
                project.target = target;
                project.shooter = select;
                project.shooter_character = character;
                project.dir = dir.normalized;
                project.damage = damage;

                //Remove projectile
                if (projectile != null && character.Equip != null)
                    character.Equip.EquipData.AddItem(projectile.id, -1);
            }

            //Melee attack
            else if (IsAttackTargetInRange(target.Interactable))
            {
                target.TakeDamage(character, GetAttackDamage(target));

                if (onAttackHit != null)
                    onAttackHit.Invoke(target);
            }
        }

        //Cancel current attack
        public void CancelAttack()
        {
            if (is_attacking)
            {
                is_attacking = false;
                attack_timer = 0f;
                character.StopMove();
                if (attack_routine != null)
                    StopCoroutine(attack_routine);
            }
        }

        //Is the character in attack mode?
        public bool IsAttacking()
        {
            return is_attacking;
        }

        //Is the character in fighting mode?
        public bool IsFighting()
        {
            return is_attacking || character.GetCurrentAction() is ActionAttack;
        }

        //Can it be attacked at all?
        public bool CanAttack(Destructible target)
        {
            return target != null && target.CanBeAttacked()
                && target.target_team != GetAttackGroup()
                && (target.target_group == null || target.target_group != GetTeamGroup());
        }

        public AttackTeam GetAttackGroup()
        {
            if (destruct != null)
                return destruct.target_team;
            return AttackTeam.CantBeAttacked;
        }

        public GroupData GetTeamGroup()
        {
            if (destruct != null)
                return destruct.target_group;
            return null;
        }

        public int GetAttackDamage(Destructible target)
        {
            int damage = attack_damage;
            ItemEquipData equipped = GetBestWeapon();
            if (equipped != null)
                damage = equipped.attack_damage;

            float mult = 1f + character.GetBonusValue(BonusType.AttackPercent, target.Interactable);
            int bonus = Mathf.RoundToInt(character.GetBonusValue(BonusType.AttackValue, target.Interactable));
            return Mathf.RoundToInt(damage * mult) + bonus;
        }

        public float GetAttackRange()
        {
            ItemEquipData equipped = GetBestWeapon();
            if (equipped != null)
                return equipped.attack_range;
            return attack_range;
        }

        public float GetAttackSpeedMultiplier()
        {
            float att_speed = 1f + character.GetBonusValue(BonusType.AttackSpeed);
            return att_speed * TheGame.Get().GetSpeedMultiplier();
        }

        public float GetAttackCooldown()
        {
            return attack_cooldown / GetAttackSpeedMultiplier();
        }

        public float GetAttackWindup()
        {
            float windup = attack_windup;
            EquipWeapon weapon = GetWeaponObject();
            if (weapon != null && weapon.Windup > 0.001f)
                windup = weapon.Windup;
            return windup / GetAttackSpeedMultiplier();
        }

        public float GetAttackWindout()
        {
            float windout = attack_windout;
            EquipWeapon weapon = GetWeaponObject();
            if (weapon != null && weapon.Windout > 0.001f)
                windout = weapon.Windout;
            return windout / GetAttackSpeedMultiplier();
        }

        public Vector3 GetProjectileSpawnPos()
        {
            ItemEquipData weapon = GetBestWeapon();
            if (weapon != null)
            {
                EquipAttach attach = character.Equip?.GetEquipAttachment(weapon.equip_slot);
                if (attach != null)
                    return attach.transform.position;
            }
            return transform.position + Vector3.up;
        }

        public bool HasRangedWeapon()
        {
            ItemEquipData equipped = GetBestWeapon();
            return equipped != null && equipped.IsRanged();
        }

        public bool HasRangedProjectile(ItemEquipData weapon)
        {
            ItemData projectile = GetRangedProjectile(weapon);
            return projectile != null || weapon.projectile_default != null;
        }

        public ItemProjData GetRangedProjectile(ItemEquipData weapon)
        {
            if (weapon != null && weapon.IsRanged())
            {
                ItemData item = character.Equip?.GetEquippedItem(weapon.projectile_group);
                ItemProjData projectile = ItemProjData.Get(item?.id);
                if (projectile != null)
                    return projectile;
            }
            return null;
        }

        public GameObject GetDefaultProjectile()
        {
            ItemEquipData equipped = GetBestWeapon();
            if (equipped != null)
                return equipped.projectile_default;
            return default_projectile;
        }

        public float GetTargetAttackRange(Interactable target)
        {
            return GetAttackRange() + target.use_range;
        }

        public bool IsAttackTargetInRange(Interactable target)
        {
            if (target != null)
            {
                float dist = (target.transform.position - transform.position).magnitude;
                return dist < GetTargetAttackRange(target);
            }
            return false;
        }

        public ItemEquipData GetBestWeapon()
        {
            if (character.Equip == null)
                return null;

            int max_value = 0;
            ItemEquipData best = null;
            foreach (ItemEquipData item in character.Equip.GetItems())
            {
                if (item.IsWeapon())
                {
                    if (!item.IsRanged() || HasRangedProjectile(item))
                    {
                        if (item.attack_damage > max_value)
                        {
                            max_value = item.attack_damage;
                            best = item;
                        }
                    }
                }
            }
            return best;
        }

        public EquipWeapon GetWeaponObject()
        {
            ItemEquipData weapon = GetBestWeapon();
            if (weapon != null && character.Equip != null)
            {
                EquipItem equip = character.Equip.GetEquipItemObject(weapon.equip_slot);
                if (equip != null && equip is EquipWeapon)
                    return (EquipWeapon)equip;
            }
            return null;
        }

        public bool IsRangedAttack()
        {
            ItemEquipData weapon = GetBestWeapon();
            return weapon != null && weapon.IsRanged();
        }

        public bool IsCooldownReady()
        {
            return attack_timer > GetAttackCooldown();
        }

        public bool CanAttack()
        {
            return attack_type != AttackType.None;
        }

        public bool IsDead()
        {
            return character.IsDead();
        }

        public Character GetCharacter()
        {
            return character;
        }

    }
}
