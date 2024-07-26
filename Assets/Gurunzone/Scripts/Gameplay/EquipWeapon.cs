using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// List EquipItem but for weapons
    /// A Weapon that will appear on the player to display equipped weaopn. Will be attached to a EquipAttach
    /// </summary>

    public class EquipWeapon : EquipItem
    {
        [Header("Animation Override")]
        public string attack_animation; //If set to null, will not be overriden and will use default value on CharacterAttack
        public string gather_animation; //If set to null, will not be overriden and will use default value on CharacterAttack

        [Header("Timing Override")]
        public float windup = 0f; //If set to 0, will not be overriden and will use default value
        public float windout = 0f; //If set to 0, will not be overriden and will use default value

        public override float Windup { get { return windup; } }
        public override float Windout { get { return windout; } }
        public override string AttackAnim { get { return attack_animation; } }
        public override string GatherAnim { get { return gather_animation; } }
    }
}
