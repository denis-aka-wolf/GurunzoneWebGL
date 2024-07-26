using System.Collections;
using UnityEngine;

namespace Gurunzone.EditorTool
{
    /// <summary>
    /// Default Settings file for the CreatObject editor script
    /// </summary>
    
    [CreateAssetMenu(fileName = "CreateObjectSettings", menuName = "Gurunzone/CreateObjectSettings", order = 100)]
    public class CreateObjectSettings : ScriptableObject
    {

        [Header("Save Folders")]
        public string prefab_folder = "Gurunzone/Prefabs";
        public string prefab_equip_folder = "Gurunzone/Prefabs/Equip";
        public string items_folder = "Gurunzone/Resources/Items";
        public string constructions_folder = "Gurunzone/Resources/Constructions";

        [Header("Default Values")]
        public Material outline;
        public GameObject death_fx;
        public AudioClip craft_audio;
        public AudioClip build_audio;
        public GameObject build_fx;

    }

}