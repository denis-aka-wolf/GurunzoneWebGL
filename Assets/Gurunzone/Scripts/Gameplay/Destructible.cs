using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gurunzone
{
    public enum AttackTeam
    {
        Neutral=0, //Can be attacked by anyone, but won't be attacked automatically, use for resources
        Ally=10,   //Will be attacked automatically by enemies, cant be attacked by colonists
        Enemy=20,  //Will be attacked automatically by colonists
        CantBeAttacked =50, //Cannot be attacked
    }

    /// <summary>
    /// Destructibles are objects that can be destroyed. They have HP and can be damaged by the player or by animals. 
    /// They often spawn loot items when destroyed (or killed)
    /// </summary>

    [RequireComponent(typeof(Selectable))]
    [RequireComponent(typeof(Interactable))]
    [RequireComponent(typeof(UniqueID))]
    public class Destructible : MonoBehaviour
    {
        [Header("Stats")]
        public int hp = 100;        //Starting and max HP
        public int armor = 0;       //Reduces each attack's damage by the armor value
        public float hp_regen = 0f; //HP regen per game-hour

        [Header("Targeting")]
        public AttackTeam target_team;     //Check above for description of each group
        public GroupData target_group;     //Enemies of the same group won't attack each other.

        [Header("Loot")]
        public CSData[] loots;      //Items spawned on death

        [Header("FX")]
        public float destroy_delay = 0f; //In seconds, use this if you want a death animation before the object disappears
        public GameObject hp_bar;       //Will spawn this HP bar when damaged
        public GameObject hit_fx;       //Prefab spawned then hit
        public GameObject death_fx;     //Prefab spawned then dying
        public AudioClip hit_sound;
        public AudioClip death_sound;

        //Events
        public UnityAction<Selectable> onDamagedBy;
        public UnityAction onDamaged;
        public UnityAction onDeath;

        protected Selectable select;
        protected Interactable interact;
        protected Collider[] colliders;
        protected UniqueID uid;
        protected int max_hp;
        protected float hp_regen_val;
        protected bool dead = false;

        private HPBar hbar = null;

        private static List<Destructible> destruct_list = new List<Destructible>();

        protected virtual void Awake()
        {
            destruct_list.Add(this);
            uid = GetComponent<UniqueID>();
            select = GetComponent<Selectable>();
            interact = GetComponent<Interactable>();
            colliders = GetComponentsInChildren<Collider>();
            max_hp = hp;
        }

        protected virtual void OnDestroy()
        {
            destruct_list.Remove(this);
        }

        protected virtual void Start()
        {
            if (SaveData.Get().IsObjectRemoved(HPUID))
            {
                Destroy(gameObject);
                return;
            }

            if (HasUID() && SaveData.Get().HasCustomInt(HPUID))
            {
                hp = SaveData.Get().GetCustomInt(HPUID);
            }
        }

        protected virtual void Update()
        {
            //Spawn HP bar
            if (HP > 0 && HP < MaxHP && hbar == null && hp_bar != null)
            {
                GameObject hp_obj = Instantiate(hp_bar, transform);
                hbar = hp_obj.GetComponent<HPBar>();
                hbar.target = this;
            }

            //Regen HP
            if (!dead && HPRegen > 0.01f && HP < HPRegen)
            {
                float game_speed = TheGame.Get().GetGameTimeSpeed();
                hp_regen_val += HPRegen * game_speed * Time.deltaTime;
                if (hp_regen_val >= 1f)
                {
                    hp_regen_val -= 1f;
                    HP += 1;
                }
            }
        }

        //Take damage from character source
        public void TakeDamage(Character attacker, int damage)
        {
            if (attacker.Colonist != null)
            {
                attacker.Colonist.Attributes.AddXP(BonusType.AttackValue, damage, interact);
                attacker.Colonist.Attributes.AddXP(BonusType.AttackPercent, damage, interact);
            }

            TakeDamage(attacker.Selectable, damage);
        }

        //Take damage from source
        public void TakeDamage(Selectable attacker, int damage)
        {
            TakeDamage(damage);
            if (!dead && attacker != null)
                onDamagedBy?.Invoke(attacker);
        }

        //Take damage from no sources
        public void TakeDamage(int damage)
        {
            if (!dead)
            {
                int adamage = Mathf.Max(damage - Armor, 1);
                HP -= adamage;
                SaveData.Get().SetCustomInt(HPUID, HP);

                onDamaged?.Invoke();

                if (select.IsNearCamera(20f))
                {
                    if (hit_fx != null)
                        Instantiate(hit_fx, transform.position, Quaternion.identity);

                    FloatNumberFX.Create(adamage, transform.position, Color.red);
                    TheAudio.Get().PlaySFX3D("destruct", hit_sound, transform.position);
                }

                if (HP <= 0)
                    Kill();
            }
        }

        public void Heal(int value)
        {
            if (!dead)
            {
                HP += value;
                HP = Mathf.Min(HP, MaxHP);

                SaveData.Get().SetCustomInt(HPUID, hp);
            }
        }

        //Kill the destructible
        public void Kill()
        {
            if (!dead)
            {
                dead = true;
                HP = 0;

                foreach (Collider collide in colliders)
                    collide.enabled = false;

                SaveData.Get().RemoveSpawnedObject(UID); //Remove object if it was spawned
                SaveData.Get().RemoveObject(UID); //Remove object if it was in initial scene
                SaveData.Get().RemoveCustomInt(HPUID); //Remove HP custom value

                if (onDeath != null)
                    onDeath.Invoke();

                SpawnLoots();
                KillFX();

                select.Remove(destroy_delay);
            }
        }

        private void KillFX()
        {
            //FX
            if (select.IsNearCamera(20f))
            {
                if (death_fx != null)
                    Instantiate(death_fx, transform.position, Quaternion.identity);

                TheAudio.Get().PlaySFX3D("destruct", death_sound, transform.position);
            }
        }

        public void SpawnLoots()
        {
            foreach (CSData item in loots)
            {
                SpawnLoot(item);
            }
        }

        public void SpawnLoot(CSData item, int quantity=1)
        {
            if (item == null || quantity <= 0)
                return;

            Vector3 pos = GetLootRandomPos();
            if (item is ItemData)
            {
                ItemData aitem = (ItemData)item;
                Item.Create(aitem, pos, quantity);
            }
            if (item is SpawnData)
            {
                SpawnData aitem = (SpawnData)item;
                Spawnable.Create(aitem, pos, Quaternion.identity);
            }
            if (item is LootData)
            {
                LootData ldata = (LootData)item;
                ItemData idata = ldata.GetRandomItem();
                SpawnLoot(idata, quantity);
            }
        }

        private Vector3 GetLootRandomPos()
        {
            float radius = Random.Range(0.5f, 1f);
            float angle = Random.Range(0f, 360f) * Mathf.Rad2Deg;
            return transform.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
        }

        //Delayed kill (useful if the attacking character doing an animation before destroying this)
        public void KillIn(float delay)
        {
            StartCoroutine(KillInRun(delay));
        }

        private IEnumerator KillInRun(float delay)
        {
            yield return new WaitForSeconds(delay);
            Kill();
        }

        public bool HasUID()
        {
            return uid.HasUID();
        }

        public bool IsDead()
        {
            return dead;
        }

        public Vector3 GetCenter()
        {
            return transform.position + Vector3.up * 2f; //Bit higher than floor
        }

        public bool CanBeAttacked()
        {
            return target_team != AttackTeam.CantBeAttacked && !dead;
        }

        public bool IsAlly()
        {
            return target_team == AttackTeam.Ally;
        }

        public virtual int HP
        {
            get { return hp; }
            set { hp = value; }
        }

        public virtual int MaxHP
        {
            get { return max_hp; }
            set { max_hp = value; }
        }

        public virtual float HPRegen
        {
            get { return hp_regen; }
            set { hp_regen = value; }
        }

        public virtual int Armor
        {
            get { return armor; }
            set { armor = value; }
        }

        public Selectable Selectable { get { return select; } }
        public Interactable Interactable { get { return interact; } }
        public string UID { get { return uid.uid; } }
        public string HPUID { get { return HasUID() ? uid.uid + "_hp" : ""; } }


        //Get nearest destructible
        public static Destructible GetNearest(Vector3 pos, float range = 999f)
        {
            Destructible nearest = null;
            float min_dist = range;
            foreach (Destructible destruct in destruct_list) 
            {
                if (destruct != null && !destruct.IsDead())
                {
                    float dist = (destruct.transform.position - pos).magnitude;
                    if (dist < min_dist)
                    {
                        min_dist = dist;
                        nearest = destruct;
                    }
                }
            }
            return nearest;
        }

        public static Destructible GetNearest(AttackTeam team, Vector3 pos, float range = 999f)
        {
            return GetNearest(team, null, pos, range);
        }

        public static Destructible GetNearest(AttackTeam team, Destructible skip, Vector3 pos, float range = 999f)
        {
            Destructible nearest = null;
            float min_dist = range;
            foreach (Destructible destruct in destruct_list)
            {
                if (destruct != null && destruct != skip && !destruct.IsDead())
                {
                    float dist = (destruct.transform.position - pos).magnitude;
                    if (dist < min_dist && destruct.target_team == team)
                    {
                        min_dist = dist;
                        nearest = destruct;
                    }
                }
            }
            return nearest;
        }

        public static List<Destructible> GetAll()
        {
            return destruct_list;
        }
    }

}