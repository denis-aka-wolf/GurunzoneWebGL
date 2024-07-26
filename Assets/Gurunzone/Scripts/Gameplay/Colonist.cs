using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gurunzone
{
    /// <summary>
    /// Colonist are characters controlled by the player, that can be managed, auto work, and can have attributes
    /// </summary>

    [RequireComponent(typeof(Character))]
    [RequireComponent(typeof(UniqueID))]
    public class Colonist : CSObject
    {
        public ColonistData data;       //Link to data, will be set auto when using Colonist.Create
        public ColonistSkinData skin;   //Link to skin

        [Header("FX")]
        public AudioClip select_audio;
        public AudioClip order_move_audio;
        public AudioClip order_target_audio;

        public UnityAction<Colonist, WorkBasic> onStartWork; 
        public UnityAction<Colonist> onStopWork; 

        private Selectable select;
        private Character character;
        private Inventory inventory;  //may be null
        private ColonistAttribute attributes; //may be null
        private UniqueID uid;

        private WorkBasic current_work = null;
        private Interactable work_target = null;
        private float update_timer = 0f;

        private static List<Colonist> colonist_list = new List<Colonist>();
        private static List<Colonist> selected_list = new List<Colonist>();

        protected override void Awake()
        {
            base.Awake();
            colonist_list.Add(this);
            select = GetComponent<Selectable>();
            character = GetComponent<Character>();
            inventory = GetComponent<Inventory>();
            attributes = GetComponent<ColonistAttribute>();
            uid = GetComponent<UniqueID>();
            ColonistManager.Get()?.RegisterColonist(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            colonist_list.Remove(this);
            selected_list.Remove(this);
            character.onDeath += OnDeath;
            ColonistManager.Get()?.UnregisterColonist(this);
            TheControls.Get().onRClick -= OnRightClick;
            TheControls.Get().onRClickFloor -= OnRClickFloor;
            TheControls.Get().onRClickSelect -= OnRClickSelect;
        }

        protected override void Start()
        {
            base.Start();
            select.onSelect += OnSelect;
            select.onUnselect += OnUnselect;
            TheControls.Get().onRClick += OnRightClick;
            TheControls.Get().onRClickFloor += OnRClickFloor;
            TheControls.Get().onRClickSelect += OnRClickSelect;
        }

        //AfterNew happens after TheGame Start() function, only for new games
        protected override void AfterNew()
        {
            base.AfterNew();

            //Set name and scene
            if (string.IsNullOrEmpty(SData.name))
            {
                SData.skin = skin.id;
                SData.name = skin.GetRandomUniqueName();
                SData.scene = SceneNav.GetCurrentScene();
            }
        }

        //AfterLoad happens after TheGame Start() function, only when loading game
        protected override void AfterLoad()
        {
            base.AfterLoad();

            //Resume Actions
            if (SaveData.Get().HasColonist(UID) && SData.scene == SceneNav.GetCurrentScene())
            {
                if (!string.IsNullOrEmpty(SData.work))
                {
                    WorkBasic work = WorkBasic.Get(SData.work);
                    StartWork(work, work.FindBestTarget(transform.position));
                }
                else if (!string.IsNullOrEmpty(SData.action))
                {
                    ActionBasic action = ActionBasic.Get(SData.action);
                    Interactable target = Interactable.Get(SData.target);
                    Order(action, target);
                }
                else if(character.IsIdle())
                {
                    Vector3 diff = SData.tpos - transform.position;
                    if (diff.magnitude > 0.5f)
                        character.MoveTo(SData.tpos);
                }
            }
        }

        protected override void Update()
        {
            base.Update();

            if (TheGame.Get().IsPaused())
                return;

            if (character.IsDead())
                return;

            //Current Work
            current_work?.UpdateWork(this);

            //Growth
            UpdateGrowth();

            update_timer += Time.deltaTime;
            if (update_timer > 0.5f)
            {
                update_timer = 0f;
                SlowUpdate();
            }
        }

        private void SlowUpdate()
        {
            //Save
            SData.action = GetAction() ? GetAction().id : "";
            SData.work = GetWork() ? GetWork().id : "";
            SData.target = GetActionTarget() ? GetActionTarget().UID : "";
            SData.tpos = character.GetMoveTargetPos();
        }

        private void UpdateGrowth()
        {
            if (data.grow_to == null || skin.grow_to == null)
                return;

            if (data.grow_to.Length == 0 || skin.grow_to.Length == 0)
                return;

            SData.growth += TheGame.Get().GetGameTimeSpeed() * Time.deltaTime;
            if (SData.growth > data.grow_time)
            {
                ColonistData cdata = data.grow_to[Random.Range(0, data.grow_to.Length)];
                ColonistSkinData sdata = skin.grow_to[Random.Range(0, skin.grow_to.Length)];

                GameObject cobj = Instantiate(sdata.skin_prefab, transform.position, transform.rotation);
                Colonist colonist = cobj.GetComponent<Colonist>();
                UniqueID uid = cobj.GetComponent<UniqueID>();
                uid.uid = UID;
                colonist.data = cdata;
                colonist.skin = sdata;
                SData.growth = 0f;
                SData.id = cdata.id;
                SData.skin = sdata.id;
                SData.spawned = true;

                StopWork();
                Destroy(gameObject);
            }
        }

        public void StartWork(WorkBasic work, Interactable target)
        {
            if (work != null && CanDoWork(work, target))
            {
                StopWork();
                current_work = work;
                work_target = target;
                work.StartWork(this);
                onStartWork?.Invoke(this, work);
            }
        }

        public void StopWork()
        {
            if (current_work != null)
                current_work.StopWork(this);
            StopAutoAction();
            current_work = null;
            work_target = null;
            onStopWork?.Invoke(this);
        }

        public void Order(ActionBasic action)
        {
            //Not target, target self
            Order(action, null);
        }

        public void Order(ActionBasic action, Interactable selectable)
        {
            StopWork();
            character.Order(action, selectable);
        }

        public void OrderInterupt(ActionBasic action, Interactable selectable)
        {
            StopWork();
            character.OrderInterupt(action, selectable);
        }

        public void OrderNext(ActionBasic action, Interactable selectable)
        {
            StopWork();
            character.OrderNext(action, selectable);
        }

        public void AutoOrder(ActionBasic action, Interactable selectable)
        {
            character.Order(action, selectable, true);
        }

        public void AutoOrderInterupt(ActionBasic action, Interactable selectable)
        {
            character.OrderInterupt(action, selectable, true);
        }

        public void AutoOrderNext(ActionBasic action, Interactable selectable)
        {
            character.OrderNext(action, selectable, true);
        }

        public void StopAutoAction()
        {
            character.CancelAutoOrders();
        }

        public override void Kill()
        {
            base.Kill();
            SaveData.Get().RemoveColonist(uid.uid);
            character.Kill();
        }

        private void OnDeath()
        {
            StopWork();
        }

        public void UseItem(Inventory inventory, GroupData group)
        {
            ItemSet item = inventory.GetItem(ItemType.Consumable, group);
            if (item != null)
            {
                UseItem(inventory, item.item);
            }
        }

        public void UseItem(Inventory inventory, GroupData group, AttributeType attribute)
        {
            ItemSet item = inventory.GetItem(ItemType.Consumable, group, attribute);
            if (item != null)
            {
                UseItem(inventory, item.item);
            }
        }

        public void UseItem(Inventory inventory, ItemData item)
        {
            if (inventory.HasItem(item))
            {
                if (item.Type == ItemType.Consumable)
                {
                    inventory.AddItem(item, -1);
                    ItemUseData uitem = (ItemUseData)item;

                    attributes.AddAttribute(AttributeType.Health, uitem.health);
                    attributes.AddAttribute(AttributeType.Energy, uitem.energy);
                    attributes.AddAttribute(AttributeType.Hunger, uitem.hunger);
                    attributes.AddAttribute(AttributeType.Thirst, uitem.thirst);
                    attributes.AddAttribute(AttributeType.Happiness, uitem.happiness);
                }
            }
        }

        public void Interact(Interactable tselect)
        {
            if (tselect == null)
                return;

            StopWork();

            bool hold_shift = TheControls.Get().IsHoldShift();
            ActionBasic action = character.GetPriorityAction(tselect);

            if (hold_shift)
                OrderNext(action, tselect);
            else
                Order(action, tselect);

            tselect.Target(character);
            TheAudio.Get().PlaySFX("order", order_target_audio, 0.4f);
        }

        public void MoveTo(Vector3 wpos)
        {
            StopWork();

            Vector3 pos = GetFormationPos(wpos);
            bool hold_shift = TheControls.Get().IsHoldShift();
            if (hold_shift)
                character.OrderMoveToNext(pos);
            else
                character.OrderMoveTo(pos);

            TheAudio.Get().PlaySFX("order", order_move_audio, 0.4f);
        }

        private void OnRightClick(Vector3 wpos)
        {
            
        }

        private void OnRClickFloor(Vector3 wpos)
        {
            if (select.IsSelected())
            {
                MoveTo(wpos);
            }
        }

        private void OnRClickSelect(Selectable tselect, Vector3 wpos)
        {
            if (select.IsSelected())
            {
                if(tselect.Interactable != null && tselect != select)
                    Interact(tselect.Interactable);
                else
                    MoveTo(wpos);
            }
        }

        private void OnSelect()
        {
            TheAudio.Get().PlaySFX("select", select_audio, 0.4f);
            if (!selected_list.Contains(this))
                selected_list.Add(this);
        }

        private void OnUnselect()
        {
            selected_list.Remove(this);
        }

        public Vector3 GetFormationPos(Vector3 pos)
        {
            int index = GetSelectionIndex();
            int count = GetSelectionCount();
            if (count >= 2)
            {
                Vector3 cpos = GetCentralPos();
                Vector3 dir = (pos - cpos);
                dir.y = 0f;
                dir.Normalize();
                Vector3 perp = new Vector3(-dir.z, 0f, dir.x);

                float side = (index % 2 == 0) ? -1f : 1f;
                float row = (index / 2);
                return pos + (perp * side * 2f) - (dir * row * 4f);
            }
            return pos;
        }

        public Vector3 GetCentralPos()
        {
            Vector3 pos = Vector3.zero;
            foreach (Colonist achar in selected_list)
            {
                pos += achar.transform.position;
            }
            if(selected_list.Count > 0)
                return pos / selected_list.Count;
            return pos;
        }

        public int GetSelectionIndex()
        {
            int index = 0;
            foreach (Colonist achar in selected_list)
            {
                if (this == achar)
                    return index;
                index++;
            }
            return -1; // not selected
        }

        public int GetSelectionCount()
        {
            return selected_list.Count;
        }

        //Does not return all bonus (like tech, equipment), only returns class bonus
        public float GetClassBonus(BonusType type, Interactable target, CraftData itarget)
        {
            float bonus = 0f;
            bool is_any = target == null && itarget == null;
            bool is_valid_select = target != null && target.Selectable.HasGroup(data.bonus_target);
            bool is_valid_item = itarget != null && itarget.HasGroup(data.bonus_target);

            //Only one of the target need to be valid, or have no target
            if (data.bonus == type && (is_any || is_valid_select || is_valid_item))
                bonus = data.bonus_value;
            return bonus;
        }

        //Returns bonus from leveling up (not other bonuses)
        public float GetLevelBonus(BonusType type, Interactable target, CraftData itarget)
        {
            return attributes.GetLevelBonus(type, target, itarget);
        }

        //Does not return all bonus (like class, equipment), only returns tech bonus
        public float GetTechBonus(BonusType type, Interactable target, CraftData itarget)
        {
            return TechManager.Get().GetTechBonus(type, Selectable, target, itarget);
        }

        public bool CanDoWork(WorkBasic work, Interactable target)
        {
            return work != null && SData.IsActionEnabled(work.id) && work.CanDoWork(this, target);
        }

        public ActionBasic GetAction()
        {
            return character.GetAction();
        }

        public Interactable GetActionTarget()
        {
            return character.GetActionTarget();
        }

        public ActionBasic GetCurrentAction()
        {
            return character.GetCurrentAction();
        }

        public Interactable GetCurrentActionTarget()
        {
            return character.GetCurrentActionTarget();
        }

        public WorkBasic GetWork()
        {
            return current_work;
        }

        public Interactable GetWorkTarget()
        {
            return work_target;
        }

        public bool IsWorking()
        {
            return current_work != null;
        }

        public bool IsIdle()
        {
            return character.IsIdle();
        }

        public bool IsManual()
        {
            return !IsAuto(); //Manual order
        }
        
        public bool IsAuto()
        {
            return character.IsAutoAction(); //Automatic order
        }

        public bool IsActive()
        {
            return character.IsActive();
        }

        public bool IsDead()
        {
            return character.IsDead();
        }

        public void Rename(string name)
        {
            SData.name = name;
        }

        public string GetName()
        {
            return SData.name;
        }

        public Selectable Selectable { get { return select; }}
        public Character Character {  get { return character; }}
        public Inventory Inventory {  get { return inventory; }}
        public ColonistAttribute Attributes {  get { return attributes; }}
        public SaveColonistData SData { get { return SaveData.Get().GetColonist(uid.uid); }} //SData is the saved data linked to this object
        public string UID { get { return uid.uid; } }

        public static int CountPopulation()
        {
            return colonist_list.Count;
        }

        public static int CountWorkingOn(Interactable target)
        {
            return ColonistManager.Get().CountWorkingOn(target);
        }

        public static Colonist Get(ColonistData data)
        {
            foreach (Colonist colonist in colonist_list)
            {
                if (colonist.data == data)
                    return colonist;
            }
            return null;
        }

        public static List<Colonist> GetAllGroup(GroupData group)
        {
            List<Colonist> valid_list = new List<Colonist>();
            foreach (Colonist colonist in colonist_list)
            {
                if (colonist.data.HasGroup(group) || colonist.Selectable.HasGroup(group))
                    valid_list.Add(colonist);
            }
            return valid_list;
        }

        public static new List<Colonist> GetAll()
        {
            return colonist_list;
        }

        public static Colonist Spawn(string uid, SaveColonistData data)
        {
            ColonistData cdata = ColonistData.Get(data.id);
            SaveCharacterData sdata = SaveData.Get().GetCharacter(uid);
            if (cdata != null && sdata != null && data.scene == SceneNav.GetCurrentScene())
            {
                ColonistSkinData skin = ColonistSkinData.Get(data.skin);
                GameObject obj = Instantiate(cdata.GetPrefab(skin), sdata.pos, sdata.rot);
                Colonist colonist = obj.GetComponent<Colonist>();
                colonist.data = cdata;
                UniqueID uniqueid = obj.GetComponent<UniqueID>();
                uniqueid.uid = uid;
                return colonist;
            }
            return null;
        }

        public static Colonist Create(ColonistData data, Vector3 pos)
        {
            return Create(data, pos, data.prefab.transform.rotation);
        }

        public static Colonist Create(ColonistData data, Vector3 pos, Quaternion rot)
        {
            ColonistSkinData skin = data.GetRandomSkin();
            return Create(data, skin, pos ,rot);
        }

        public static Colonist Create(ColonistData data, ColonistSkinData skin, Vector3 pos, Quaternion rot)
        {
            GameObject prefab = data.GetPrefab(skin);
            GameObject obj = Instantiate(prefab, pos, rot);
            Colonist colonist = obj.GetComponent<Colonist>();
            colonist.data = data;
            colonist.skin = skin;
            UniqueID uniqueid = obj.GetComponent<UniqueID>();
            uniqueid.uid = UniqueID.GenerateUniqueID();
            SaveData sdata = SaveData.Get();
            SaveCharacterData chdata = sdata.GetCharacter(uniqueid.uid);
            chdata.scene = SceneNav.GetCurrentScene();
            chdata.pos = pos;
            SaveColonistData codata = sdata.GetColonist(uniqueid.uid);
            codata.id = data.id;
            codata.scene = SceneNav.GetCurrentScene();
            codata.spawned = true;
            codata.skin = data.GetSkinID(skin);
            codata.name = data.GetRandomUniqueName(skin);
            return colonist;
        }

        public static Colonist Create(CraftGroupData data, Vector3 pos, Quaternion rot)
        {
            ColonistData cdata = (ColonistData) data.GetRandomData();
            return Create(cdata, pos, rot);
        }
    }
}
