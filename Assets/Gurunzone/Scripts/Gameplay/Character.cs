using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gurunzone
{
    //Class to queue Actions and their target
    public class CharacterOrder
    {
        public ActionBasic action;
        public Interactable target;
        public Vector3 pos;
        public bool automatic = false;

        public CharacterOrder() { }
        public CharacterOrder(Vector3 p) { pos = p; }
        public CharacterOrder(ActionBasic a, Interactable t, Vector3 p, bool auto)
        {
            action = a; target = t; automatic = auto; pos = p;
        }
    }

    /// <summary>
    /// Character is a generic class for any kind of characters, it can move and execute Actions.
    /// </summary>

    [RequireComponent(typeof(Selectable))]
    [RequireComponent(typeof(UniqueID))]
    public class Character : MonoBehaviour
    {
        [Header("Move")]
        public float move_speed = 10f;      //How fast the colonist moves?
        public float rotate_speed = 250f;   //How fast it can rotate

        [Header("Ground/Collision")]
        public float front_detect_dist = 0.5f; //Margin distance between the character and obstacles, used to detect if character is fronted.
        public float ground_detect_dist = 0.1f; //Margin distance between the character and the ground, used to detect if character is grounded.
        public LayerMask ground_layer = (1 << 9);     //What is considered ground?

        public UnityAction onStartAction;
        public UnityAction onStopAction;
        public UnityAction onDeath;

        private Selectable select;
        private Destructible destruct; //may be null
        private UniqueID uid;
        private Colonist colonist; //may be null
        private Inventory inventory; // may be null

        private CharacterAttack attack; //may be null
        private CharacterEquip equip; // may be null
        private CharacterPathfind pathfind; // may be null
        private CharacterAnim anim; // may be null

        private bool is_moving = false;
        private Vector3 moving;
        private Vector3 facing;
        private Vector3 move_target;
        private Interactable move_action_target;
        private int action_target_pos;
        private bool move_action_auto = false;

        private ActionBasic current_action = null;
        private ActionBasic next_action = null;
        private Interactable action_target = null;
        private Vector3 last_target_pos;
        private float action_progress = 0f;
        private bool action_auto = false;

        private bool is_grounded = false;
        private bool is_fronted = false;
        private bool is_waiting = false;
        private bool is_dead = false;
        private float update_timer = 0f;

        private LinkedList<CharacterOrder> action_queue = new LinkedList<CharacterOrder>();
        private Dictionary<BonusType, BonusEffectData> bonus_effects = new Dictionary<BonusType, BonusEffectData>();

        private static List<Character> character_list = new List<Character>();

        private void Awake()
        {
            character_list.Add(this);
            select = GetComponent<Selectable>();
            destruct = GetComponent<Destructible>();
            inventory = GetComponent<Inventory>();
            uid = GetComponent<UniqueID>();
            colonist = GetComponent<Colonist>();
            attack = GetComponent<CharacterAttack>();
            equip = GetComponent<CharacterEquip>();
            pathfind = GetComponent<CharacterPathfind>();
            anim = GetComponentInChildren<CharacterAnim>();
            move_target = transform.position;
            facing = transform.forward;
            if (destruct)
                destruct.onDeath += OnDeath;
            if (anim)
                anim.onTrigger += OnTriggerAnim;
        }

        private void OnDestroy()
        {
            character_list.Remove(this);
        }

        private void Start()
        {
            
        }

        private void Update()
        {
            if (TheGame.Get().IsPaused())
                return;

            if (is_dead)
                return;

            //Save position
            SData.pos = transform.position;
            SData.rot = transform.rotation;

            //Actions
            UpdateAction();

            //Move
            UpdateMove();
            UpdateRotation();

            //GRounded
            DetectGrounded();
            DetectFronted();

            //Check if actions completed
            UpdateCheckComplete();

            //Bonus effects
            UpdateBonus();

            //Order queue
            if (IsIdle() && action_queue.Count > 0)
                ExecuteNextOrder();

            //Slow update
            update_timer += Time.deltaTime;
            if (update_timer > 0.5f)
            {
                update_timer = 0f;
                SlowUpdate();
            }
        }

        private void UpdateMove()
        {
            if (!is_moving || is_waiting)
                return;

            if (pathfind != null && pathfind.force_navmesh && !pathfind.HasPath())
                return; //No path found, navmesh is forced, dont move

            //Speed mult
            float speed_mult = TheGame.Get().GetSpeedMultiplier();
            float move_speed_mult = speed_mult * GetBonusMult(BonusType.MoveSpeed);

            //Find move direction
            Vector3 tpos = move_target;
            if (pathfind != null && pathfind.HasPath())
                tpos = pathfind.GetNextTarget(); //Move to pathfind target instead

            Vector3 move_dir = tpos - transform.position;
            Vector3 tmove = move_dir.normalized * Mathf.Min(move_dir.magnitude, 1f) * move_speed * move_speed_mult;

            //Adjust height
            bool success = PhysicsTool.FindGroundPosition(transform.position, ground_layer, out Vector3 gpos);
            Vector3 hdir = gpos - transform.position;
            if (success)
                tmove += hdir.normalized * Mathf.Min(hdir.magnitude, 1f) * move_speed * move_speed_mult;

            //Apply movement
            moving = Vector3.Lerp(moving, tmove, 10f * Time.deltaTime);
            transform.position += moving * Time.deltaTime;

            //Find Facing direction
            Vector3 tface = new Vector3(tmove.x, 0f, tmove.z);
            if (tface.magnitude > 0.1f)
                facing = tface.normalized;

            //Debug.DrawLine(move_target, move_target + Vector3.up * 5, Color.blue);
        }

        private void UpdateRotation()
        {
            //Apply Rotation
            float speed_mult = TheGame.Get().GetSpeedMultiplier();
            float move_speed_mult = speed_mult * GetBonusMult(BonusType.MoveSpeed);
            Quaternion targ_rot = Quaternion.LookRotation(facing, Vector3.up);
            Quaternion nrot = Quaternion.RotateTowards(transform.rotation, targ_rot, rotate_speed * move_speed_mult * Time.deltaTime);
            transform.rotation = nrot;
        }

        private void UpdateCheckComplete()
        {
            if (!is_moving || is_waiting)
                return;

            //Reached action Target
            if (HasSelectTarget() && HasReachedTarget())
                InteractTarget(move_action_target, move_action_auto);

            //Reached move Target
            else if (!HasSelectTarget() && HasReachedTarget())
                StopMove();

            //Reached attack target
            else if(IsAttackTargetInRange())
                InteractTarget(move_action_target, move_action_auto);

            //Obstacles
            else if(is_fronted)
                Stop();

            //Pathfind Failed, Stop
            if (is_moving && pathfind != null && pathfind.force_navmesh && pathfind.HasFailed())
                Stop();
        }

        private void UpdateBonus()
        {
            float game_speed = TheGame.Get().GetGameTimeSpeed();
            List<BonusType> remove_list = new List<BonusType>();
            foreach (KeyValuePair<BonusType, BonusEffectData> pair in bonus_effects)
            {
                if (pair.Value.duration <= 0f)
                    remove_list.Add(pair.Key);
                pair.Value.duration -= game_speed * Time.deltaTime;
            }

            foreach (BonusType bonus in remove_list)
                bonus_effects.Remove(bonus);
        }

        private void UpdateAction()
        {
            if (current_action == null)
                return;

            //Stop action if condition become false
            if (!current_action.CanDoAction(this, action_target))
            {
                Stop();
                return;
            }

            //Update action
            current_action.UpdateAction(this, action_target);
        }

        private void SlowUpdate()
        {
            //Update move target
            if (move_action_target != null)
                move_target = move_action_target.GetInteractPosition(ground_layer, action_target_pos);

            //Update target pos if its moving
            if (action_target != null)
                last_target_pos = action_target.GetInteractPosition(ground_layer, action_target_pos);

            //Update Pathfind
            pathfind?.UpdateMoveTo(move_target);

            //Update inventory max
            if (inventory != null)
                inventory.SetCargoBonus(Mathf.RoundToInt(GetBonusValue(BonusType.InventorySize)));
        }

        private void DetectGrounded()
        {
            Vector3 center = transform.position + Vector3.up;
            is_grounded = PhysicsTool.DetectGround(transform, center, 1f + ground_detect_dist, ground_layer);
        }

        private void DetectFronted()
        {
            Vector3 center = transform.position + Vector3.up;
            is_fronted = PhysicsTool.RaycastCollisionLayer(center, facing.normalized * front_detect_dist, ground_layer, out RaycastHit hit);
        }

        public void MoveTo(Vector3 pos)
        {
            is_moving = true;
            move_target = pos;
            move_action_target = null;
            action_target = null;
            current_action = null;
            move_action_auto = false;
            pathfind?.MoveTo(move_target);
        }

        public void MoveToTarget(Interactable target, bool auto = false)
        {
            if (target.IsInteractFull())
                return;

            is_moving = true;
            move_action_target = target;
            action_target = null;
            current_action = null;
            move_action_auto = auto;
            action_target_pos = target.GetInteractPositionIndex(this, ground_layer);
            move_target = target.GetInteractPosition(ground_layer, action_target_pos);
            pathfind?.MoveTo(move_target);
        }

        public void InteractTarget(Interactable target, bool auto = false)
        {
            StopMove();

            if(target != null)
                target.Interact(this);

            if (next_action == null)
                next_action = GetPriorityAction(target);

            if (next_action != null)
            {
                StartAction(next_action, target, auto);
            }
        }

        public void AttackTarget(Interactable target)
        {
            ActionAttack attack = ActionBasic.Get<ActionAttack>();
            if(attack.CanDoAction(this, target))
                Order(attack, target);
        }

        //Cancel current actions, and replace with a new one
        public void Order(ActionBasic action, Interactable target, bool auto = false)
        {
            CancelOrders();
            ExecuteOrder(action, target, auto);
        }

        //Do action now, but resume previous order right after
        public void OrderInterupt(ActionBasic action, Interactable target, bool auto = false)
        {
            if (current_action != null)
            {
                CharacterOrder current = new CharacterOrder(current_action, action_target, move_target, action_auto);
                action_queue.AddFirst(current);
            }

            Vector3 pos = target != null ? target.transform.position : transform.position;
            CharacterOrder order = new CharacterOrder(action, target, pos, auto);
            action_queue.AddFirst(order);

            StopAction();
            ExecuteNextOrder();
        }

        //Do action after current one
        public void OrderNext(ActionBasic action, Interactable target, bool auto = false)
        {
            Vector3 pos = target != null ? target.transform.position : transform.position;
            CharacterOrder order = new CharacterOrder(action, target, pos, auto);
            action_queue.AddLast(order);
            if (IsIdle())
            {
                ExecuteNextOrder();
            }
        }

        public void OrderMoveTo(Vector3 pos)
        {
            CancelOrders();
            MoveTo(pos);
        }

        public void OrderMoveToNext(Vector3 pos)
        {
            CharacterOrder order = new CharacterOrder();
            order.pos = pos;
            action_queue.AddLast(order);
        }

        public void CancelOrders()
        {
            action_queue.Clear();
            StopAction();
        }

        public void CancelAutoOrders()
        {
            if (action_queue.Count > 0)
            {
                for (LinkedListNode<CharacterOrder> node = action_queue.First; node != null; node = node.Next)
                {
                    if (node.Value.automatic)
                        action_queue.Remove(node);
                }
            }
            if (action_auto)
                StopAction();
        }

        public int CountQueuedOrders()
        {
            return action_queue.Count;
        }

        public void StartAction(ActionBasic action, Interactable target, bool auto = false)
        {
            if (action != null && action.CanDoAction(this, target))
            {
                current_action = action;
                next_action = null;
                action_auto = auto;
                action_target = target;
                action_progress = 0f;
                current_action.StartAction(this, target);
                onStartAction?.Invoke();
            }
        }

        public void Stop()
        {
            StopAction();
            StopMove();
        }

        public void StopAction()
        {
            if (next_action != null)
                StopMove();
            if (current_action != null)
                current_action.StopAction(this, action_target);
            current_action = null;
            action_target = null;
            next_action = null;
            action_auto = false;
            onStopAction?.Invoke();
        }
        
        public void StopMove()
        {
            is_moving = false;
            move_action_target = null;
            move_action_auto = false;
            move_target = transform.position;
            moving = Vector3.zero;
            pathfind?.Stop();
            StopAnimate();
        }

        private void ExecuteNextOrder()
        {
            if (action_queue.Count > 0)
            {
                CharacterOrder order = action_queue.First.Value;
                action_queue.RemoveFirst();

                if (order.action != null && order.action.CanDoAction(this, order.target))
                {
                    ExecuteOrder(order.action, order.target, order.automatic);
                }
                else if (order.target != null)
                {
                    MoveToTarget(order.target, order.automatic);
                }
                else
                {
                    MoveTo(order.pos);
                }
            }
        }

        private void ExecuteOrder(ActionBasic action, Interactable target, bool auto = false)
        {
            if (action != null && action.CanDoAction(this, target))
            {
                next_action = action;

                if (target != null && target != Selectable)
                    MoveToTarget(target, auto); //Action has target, first move closer
                else
                    InteractTarget(target, auto); //Action has no target (ex: eat)
            }
            else if(target != null)
            {
                MoveToTarget(target, auto);
            }
        }

        public ActionBasic GetPriorityAction(Interactable tselect)
        {
            foreach (ActionBasic action in tselect.actions)
            {
                if (action != null && action.CanDoAction(this, tselect))
                {
                    return action;
                }
            }
            return null;
        }

        public void SetActionProgress(float value)
        {
            action_progress = value;
        }

        public void AddActionProgress(float value)
        {
            action_progress += value;
        }

        public void RunActionTrigger(string trigger)
        {
            if (current_action != null)
            {
                current_action.TriggerAction(this, action_target, trigger);
            }
        }

        public void Wait()
        {
            is_waiting = true;
        }

        public void StopWait()
        {
            is_waiting = false;
        }

        public void WaitFor(float duration, UnityAction callback = null)
        {
            if (!is_waiting)
            {
                StopMove();
                is_waiting = true;
                StartCoroutine(RunWaitRoutine(duration, callback));
            }
        }
        
        private IEnumerator RunWaitRoutine(float action_duration, UnityAction callback = null)
        {
            is_waiting = true;
            
            float mult = TheGame.Get().GetSpeedMultiplier();
            float duration = mult > 0.001f ? action_duration / mult : 0f;
            yield return new WaitForSeconds(duration);

            is_waiting = false;
            if (callback != null)
                callback.Invoke();
        }

        public void PlayAnim(string anim_id)
        {
            anim?.PlayAnim(anim_id);
        }

        public void Animate(string anim_id, bool active)
        {
            anim?.Animate(anim_id, active);
        }

        public void StopAnimate()
        {
            anim?.StopAnimate();
        }

        public void FaceToward(Vector3 pos)
        {
            facing = pos - transform.position;
            facing.y = 0f;
            facing.Normalize();
        }

        public void SetFacing(Vector3 dir)
        {
            facing = dir;
        }

        public void ShowTool(GroupData tool)
        {
            equip?.ShowTool(tool);
        }

        public void HideTool(GroupData tool)
        {
            equip?.HideTool(tool);
        }

        public void HideTools()
        {
            equip?.HideTools();
        }

        public void Kill()
        {
            if (!is_dead)
            {
                OnDeath();
                select.Kill(1f);
            }
        }

        private void OnDeath()
        {
            CancelOrders();
            is_dead = true;
            SaveData.Get().RemoveCharacter(uid.uid);
            SaveData.Get().RemoveObject(uid.uid);
            onDeath?.Invoke();
        }

        private void OnTriggerAnim(string id)
        {
            RunActionTrigger(id);
        }
        
        public void SetTempBonusEffect(BonusType type, float value, float duration)
        {
            if (bonus_effects.ContainsKey(type))
            {
                if(bonus_effects[type].value < value || bonus_effects[type].duration < duration)
                    bonus_effects[type] = new BonusEffectData(type, value, duration);
            }
            else
            {
                bonus_effects[type] = new BonusEffectData(type, value, duration);
            }
        }

        public void RemoveTempBonusEffect(BonusType type)
        {
            bonus_effects.Remove(type);
        }

        public float GetTempBonusEffectValue(BonusType type)
        {
            if (bonus_effects.ContainsKey(type))
                return bonus_effects[type].value;
            return 0f;
        }

        public float GetTempBonusEffectDuration(BonusType type)
        {
            if (bonus_effects.ContainsKey(type))
                return bonus_effects[type].duration;
            return 0f;
        }

        //Bonus Raw values
        public float GetBonusValue(BonusType type, Interactable target = null, CraftData itarget = null)
        {
            float bbonus = GetTempBonusEffectValue(type);                                        //Temporary Bonus (buff)
            float ebonus = Equip ? Equip.GetEquipBonus(type, target, itarget) : 0f;              //Equip Bonus
            float tbonus = Colonist != null ? Colonist.GetTechBonus(type, target, itarget) : 0f;  //Tech Bonus
            float cbonus = Colonist != null ? Colonist.GetClassBonus(type, target, itarget) : 0f; //Class bonus
            float lbonus = Colonist != null ? Colonist.GetLevelBonus(type, target, itarget) : 0f; //Level bonus
            return bbonus + ebonus + tbonus + cbonus + lbonus;
        }

        //Bonus multiplier
        public float GetBonusMult(BonusType type, Interactable target = null, CraftData itarget = null)
        {
            return 1f + GetBonusValue(type, target, itarget);
        }

        public bool HasReachedTarget()
        {
            Vector3 dir_total = GetMoveTargetPos() - transform.position;
            float threshold = pathfind != null ? pathfind.path_threshold : ground_detect_dist;
            return dir_total.magnitude < threshold;
        }

        public bool IsAttackTargetInRange()
        {
            if (attack != null && IsAttackMode())
            {
                return attack.IsAttackTargetInRange(move_action_target);
            }
            return false;
        }

        public bool HasSelectTarget()
        {
            return move_action_target != null;
        }

        public bool IsAttackMode()
        {
            bool attack_next = next_action != null && next_action is ActionAttack;
            bool attack_now = current_action != null && current_action is ActionAttack;
            return attack_next || attack_now;
        }

        public bool IsTargetUnreachable()
        {
            if (pathfind != null)
            {
                Interactable target = GetTarget();
                float max_offset = target != null ? (target.use_range + 0.01f) : 1f;
                return pathfind.IsTargetUnreachable(max_offset);
            }
            return false; //Pathfinding is off
        }

        public Interactable GetTarget()
        {
            if(current_action != null && action_target != null)
                return action_target;
            return move_action_target;
        }

        public Vector3 GetMoveTargetPos()
        {
            if (pathfind != null && pathfind.force_navmesh && pathfind.HasPath())
                return pathfind.GetFinalDestination();
            return move_target;
        }

        public int GetTargetPosIndex()
        {
            return action_target_pos;
        }

        public bool IsReallyMoving()
        {
            return is_moving && !is_waiting && moving.magnitude > 0.1f;
        }

        public bool IsMoving()
        {
            return is_moving;
        }

        public bool IsIdle()
        {
            return current_action == null && !is_moving;
        }

        public bool IsFighting()
        {
            if (attack != null)
                return attack.IsFighting();
            return false;
        }

        public bool IsCustomAnim()
        {
            return anim != null ? anim.IsCustomAnim() : false;
        }

        public bool IsDead()
        {
            return is_dead;
        }

        public Vector3 GetMove()
        {
            return moving;
        }

        public Vector3 GetFacing()
        {
            return facing;
        }

        public bool IsGrounded()
        {
            return is_grounded;
        }

        public bool IsFronted()
        {
            return is_fronted;
        }

        public bool IsWaiting()
        {
            return is_waiting;
        }

        public bool IsAutoAction()
        {
            return current_action != null && action_auto;
        }

        //Current action or next action (moving to target)
        public ActionBasic GetAction()
        {
            return current_action != null ? current_action : next_action;
        }

        public Interactable GetActionTarget()
        {
            return current_action != null ? action_target : move_action_target;
        }

        //Only current action (null if moving toward it)
        public ActionBasic GetCurrentAction()
        {
            return current_action;
        }

        public Interactable GetCurrentActionTarget()
        {
            return action_target;
        }

        public Vector3 GetLastTargetPos()
        {
            return last_target_pos;
        }

        public float GetActionProgress()
        {
            return action_progress;
        }

        public bool IsActive()
        {
            return !IsDead() && gameObject.activeSelf;
        }

        public bool IsSleeping()
        {
            return current_action != null && current_action == ActionBasic.Get<ActionSleep>();
        }

        public Selectable Selectable { get { return select; } }
        public Destructible Destructible { get { return destruct; } } // May Be Null
        public Colonist Colonist { get { return colonist; } } // May Be Null
        public Inventory Inventory { get { return inventory; } } // May Be Null
        public CharacterEquip Equip { get { return equip; } } // May Be Null
        public CharacterAttack Attack { get { return attack; } } // May Be Null
        public SaveCharacterData SData { get { return SaveData.Get().GetCharacter(uid.uid); } } //SData is the saved data linked to this object
        public string UID { get { return uid.uid; } }

        //Interacting with, or moving toward
        public static int CountTargetingTarget(Interactable target)
        {
            int count = 0;
            foreach (Character character in character_list)
            {
                if (character.GetTarget() == target)
                    count++;
            }
            return count;
        }

        //Interacting with (already reached the target)
        public static int CountInteractTarget(Selectable target)
        {
            int count = 0;
            foreach (Character character in character_list)
            {
                if (character.GetCurrentActionTarget() == target)
                    count++;
            }
            return count;
        }

        public static Character Get(string uid)
        {
            foreach (Character character in character_list)
            {
                if (character.uid.uid == uid)
                    return character;
            }
            return null;
        }
        
        public static Character GetNearest(Vector3 pos, float range=999f, Character avoid=null)
        {
            float min_dist = range;
            Character nearest = null;
            foreach (Character character in character_list)
            {
                float dist = (character.transform.position - pos).magnitude;
                if (dist < min_dist && character != avoid)
                {
                    min_dist = dist;
                    nearest = character;
                }
            }
            return nearest;
        }

        public static List<Character> GetAllTargeting(Interactable select)
        {
            List<Character> targeting_list = new List<Character>();
            foreach (Character character in character_list)
            {
                if (character.GetTarget() == select)
                    targeting_list.Add(character);
            }
            return targeting_list;
        }

        public static List<Character> GetAll()
        {
            return character_list;
        }

        public static Character Spawn(string uid, SaveCharacterData data)
        {
            ColonistData bdata = ColonistData.Get(data.id);
            SaveCharacterData cdata = SaveData.Get().GetCharacter(uid);
            if (bdata != null && cdata != null && data.scene == SceneNav.GetCurrentScene())
            {
                GameObject obj = Instantiate(bdata.prefab, cdata.pos, cdata.rot);
                Character character = obj.GetComponent<Character>();
                UniqueID uniqueid = obj.GetComponent<UniqueID>();
                uniqueid.uid = uid;
                return character;
            }
            return null;
        }

        public static Character Create(SpawnData data, Vector3 pos, Quaternion rot)
        {
            GameObject obj = Instantiate(data.prefab, pos, rot);
            Character character = obj.GetComponent<Character>();
            UniqueID uniqueid = obj.GetComponent<UniqueID>();
            uniqueid.uid = UniqueID.GenerateUniqueID();
            SaveData sdata = SaveData.Get();
            SaveCharacterData chdata = sdata.GetCharacter(uniqueid.uid);
            chdata.id = data.id;
            chdata.scene = SceneNav.GetCurrentScene();
            chdata.pos = pos;
            chdata.rot = rot;
            chdata.spawned = true;
            return character;
        }

        public static Character Create(SpawnGroupData data, Vector3 pos, Quaternion rot)
        {
            SpawnData cdata = (SpawnData) data.GetRandomData();
            return Create(cdata, pos, rot);
        }
    }
}
