using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{

    /// <summary>
    /// Manager script that will load all scriptable objects for use at runtime
    /// </summary>

    public class TheData : MonoBehaviour
    {
        public GameData data;
        public AssetData assets;

        [Header("Resources Sub Folder")]
        public string load_folder = "";

        private static TheData instance;

        void Awake()
        {
            instance = this;

            //Load managers
            if (!FindObjectOfType<TheUI>())
                Instantiate(assets.ui_canvas);
            if (!FindObjectOfType<TheAudio>())
                Instantiate(assets.audio_manager);
            if (!FindObjectOfType<SelectionFX>())
                Instantiate(assets.selection_fx);

            Load();
        }

        private void Load()
        {
            SpawnData.Load(load_folder);
            CraftData.Load(load_folder);
            ItemData.Load(load_folder);
            ColonistData.Load(load_folder);
            ColonistSkinData.Load(load_folder);
            CraftGroupData.Load(load_folder);
            AttributeData.Load(load_folder);
            ConstructionData.Load(load_folder);
            TechData.Load(load_folder);
            NPCData.Load(load_folder);
            QuestData.Load(load_folder);
            LevelData.Load(load_folder);
            ActionBasic.Load(load_folder);
            WorkBasic.Load(load_folder);
        }

        public static TheData Get()
        {
            if (instance == null)
                instance = FindObjectOfType<TheData>();
            return instance;
        }
    }

}