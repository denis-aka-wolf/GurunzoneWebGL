using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Generic asset data (only one file)
    /// </summary>

    [CreateAssetMenu(fileName = "AssetData", menuName = "Gurunzone/AppData/AssetData", order = 0)]
    public class AssetData : ScriptableObject
    {
        [Header("Systems Prefabs")]
        public GameObject ui_canvas;        //Loads the UI prefab
        public GameObject audio_manager;    //Loads the Audio Manager

        [Header("Zones")]
        public GameObject zone_prefab;      //Zone spawned when creating a zone

        [Header("FX")]
        public GameObject selection_fx;     //Selection FX and visuals
        public GameObject selection_circle;
        public GameObject float_number;
        

        public static AssetData Get()
        {
            return TheData.Get().assets;
        }
    }

}
