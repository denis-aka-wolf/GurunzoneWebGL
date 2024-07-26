using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{

    [System.Serializable]
    public enum BonusType
    {
        None = 0,

        AttackValue = 10,
        AttackPercent = 11, //Value in percentage

        ArmorValue = 12, 
        ArmorPercent = 13,  //Value in percentage

        AttackSpeed = 14, //Value in percentage
        MoveSpeed = 16, //Value in percentage
        
        GatherSpeed = 20, //Value in percentage
        FactorySpeed = 22, //Value in percentage
        BuildSpeed = 24, //Value in percentage

        ColonySize = 30,    
        InventorySize = 32, 
    }

    /// <summary>
    /// Data file bonus effects (ongoing effect applied to the character when equipping an item or near a construction)
    /// </summary>
    
    public class BonusEffectData
    {
        public BonusType type;
        public float value;
        public float duration; //In game-hours


        public BonusEffectData(BonusType type, float value, float duration)
        {
            this.type = type;
            this.value = value;
            this.duration = duration;
        }
    }

}