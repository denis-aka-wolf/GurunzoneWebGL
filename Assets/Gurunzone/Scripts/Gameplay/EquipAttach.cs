using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{

    /// <summary>
    /// A location on a character to attach equipment (like hand, head, feet, ...)
    /// </summary>

    public class EquipAttach : MonoBehaviour
    {
        public EquipSlot slot;      //Which slot is this? 
        public float scale = 1f;    //Resizes the equipped mesh, default should be 1

        private Character character;

        private void Awake()
        {
            character = GetComponentInParent<Character>();
        }

        public Character GetCharacter()
        {
            return character;
        }

    }

}