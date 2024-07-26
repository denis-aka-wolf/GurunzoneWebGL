using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// An item that will appear on the player to display equipped item. Will be attached to a EquipAttach
    /// </summary>

    public class EquipItem : MonoBehaviour
    {
        public EquipSlot slot;          //Which slot this item attaches to?

        [Header("Children Mesh")]
        public GameObject child_left;   //For equipments with more than one mesh (example: Boots)
        public GameObject child_right;

        [HideInInspector]
        public ItemData data;
        [HideInInspector]
        public EquipAttach target;
        [HideInInspector]
        public EquipAttach target_left;
        [HideInInspector]
        public EquipAttach target_right;

        private Vector3 start_scale;

        void Start()
        {
            start_scale = transform.localScale;
        }

        void LateUpdate()
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            transform.position = target.transform.position;
            transform.rotation = target.transform.rotation;
            transform.localScale = start_scale * target.scale;

            if (child_right != null && target_right != null)
            {
                child_right.transform.position = target_right.transform.position;
                child_right.transform.rotation = target_right.transform.rotation;
                child_right.transform.localScale = start_scale * target_right.scale;
            }

            if (child_left != null && target_left != null)
            {
                child_left.transform.position = target_left.transform.position;
                child_left.transform.rotation = target_left.transform.rotation;
                child_left.transform.localScale = start_scale * target_left.scale;
            }

        }

        public virtual float Windup { get{ return 0f; } }
        public virtual float Windout { get{ return 0f; } }
        public virtual string AttackAnim { get{ return ""; } }
        public virtual string GatherAnim { get{ return ""; } }

        public Character GetCharacter()
        {
            if (target != null)
                return target.GetCharacter();
            return null;
        }
    }

}