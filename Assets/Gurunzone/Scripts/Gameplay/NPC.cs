using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    [RequireComponent(typeof(Character))]
    [RequireComponent(typeof(UniqueID))]
    public class NPC : CSObject
    {
        public NPCData data;

        private Selectable select;
        private Character character;
        private UniqueID uid;

        private float update_timer = 0f;

        private static List<NPC> npc_list = new List<NPC>();

        protected override void Awake()
        {
            base.Awake();
            npc_list.Add(this);
            select = GetComponent<Selectable>();
            character = GetComponent<Character>();
            uid = GetComponent<UniqueID>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            npc_list.Remove(this);
        }

        //AfterLoad happens after TheGame Start() function
        protected override void AfterLoad()
        {
            base.AfterLoad();

            if (SaveData.Get().HasNPC(UID) && SData.scene == SceneNav.GetCurrentScene())
            {
                if (!string.IsNullOrEmpty(SData.action))
                {
                    ActionBasic action = ActionBasic.Get(SData.action);
                    Interactable target = Interactable.Get(SData.target);
                    character.Order(action, target);
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
            SData.action = character.GetAction() ? character.GetAction().id : "";
            SData.target = character.GetActionTarget() ? character.GetActionTarget().UID : "";
            SData.tpos = character.GetMoveTargetPos();
        }

        public Selectable Selectable { get { return select; } }
        public Character Character { get { return character; } }
        public SaveNPCData SData { get { return SaveData.Get().GetNPC(uid.uid); } } //SData is the saved data linked to this object
        public string UID { get { return uid.uid; } }

        public static NPC Get(NPCData data)
        {
            foreach (NPC character in npc_list)
            {
                if (character.data == data)
                    return character;
            }
            return null;
        }

        public static List<NPC> GetAllGroup(GroupData group)
        {
            List<NPC> valid_list = new List<NPC>();
            foreach (NPC character in npc_list)
            {
                if (character.data.HasGroup(group) || character.Selectable.HasGroup(group))
                    valid_list.Add(character);
            }
            return valid_list;
        }

        public static new List<NPC> GetAll()
        {
            return npc_list;
        }

        public static NPC Spawn(string uid, SaveNPCData data)
        {
            NPCData ndata = NPCData.Get(data.id);
            SaveCharacterData cdata = SaveData.Get().GetCharacter(uid);
            if (ndata != null && cdata != null && data.scene == SceneNav.GetCurrentScene())
            {
                GameObject obj = Instantiate(ndata.prefab, cdata.pos, cdata.rot);
                NPC npc = obj.GetComponent<NPC>();
                npc.data = ndata;
                UniqueID uniqueid = obj.GetComponent<UniqueID>();
                uniqueid.uid = uid;
                return npc;
            }
            return null;
        }

        public static NPC Create(NPCData data, Vector3 pos)
        {
            return Create(data, pos, data.prefab.transform.rotation);
        }

        public static NPC Create(NPCData data, Vector3 pos, Quaternion rot)
        {
            GameObject obj = Instantiate(data.prefab, pos, rot);
            NPC npc = obj.GetComponent<NPC>();
            npc.data = data;
            UniqueID uniqueid = obj.GetComponent<UniqueID>();
            uniqueid.uid = UniqueID.GenerateUniqueID();
            SaveData sdata = SaveData.Get();
            SaveCharacterData chdata = sdata.GetCharacter(uniqueid.uid);
            chdata.scene = SceneNav.GetCurrentScene();
            chdata.pos = pos;
            SaveNPCData npdata = sdata.GetNPC(uniqueid.uid);
            npdata.id = data.id;
            npdata.scene = SceneNav.GetCurrentScene();
            npdata.spawned = true;
            return npc;
        }

        public static NPC Create(CraftGroupData data, Vector3 pos, Quaternion rot)
        {
            NPCData cdata = (NPCData)data.GetRandomData();
            return Create(cdata, pos, rot);
        }
    }
}
