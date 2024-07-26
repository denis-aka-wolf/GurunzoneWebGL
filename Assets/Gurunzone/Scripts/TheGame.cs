using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gurunzone
{
    /// <summary>
    /// Main game manager
    /// </summary>

    public class TheGame : MonoBehaviour
    {
        //non-static UnityActions only work in a game scene that uses TheGame.cs
        public UnityAction<string> beforeSave; //Right after calling Save(), before writing the file on disk
        public UnityAction<bool> onPause; //When pausing/unpausing the game
        public UnityAction onStart; //After the scene loading finished, after all Start() functions have run
        public UnityAction onStartLoad; //After the scene loading finished, after all Start() functions have run, only if its an existing game.
        public UnityAction onStartNewGame; //After the scene loading finished, after all Start() functions have run, only if its a new game.
        public UnityAction onNewDay; //When a new day starts

        //static UnityActions work in any scene (including Menu scenes that don't have TheGame.cs)
        public static UnityAction afterLoad; //Right after calling Load(), after loading the PlayerData but before changing scene
        public static UnityAction afterNewGame; //Right after calling NewGame(), after creating the PlayerData but before changing scene

        private bool paused = false;
        private bool paused_gameplay = false;
        private bool scene_transition = false;
        private float speed_multiplier = 1f;
        private float game_speed_per_hour = 1f;
        private float game_speed_per_sec = 0.002f;
        private bool inited = false;
        private int prev_day;

        private static TheGame instance;

        void Awake()
        {
            instance = this;
            SaveData.LoadLast();
        }

        private void Start()
        {
            SaveData pdata = SaveData.Get();
            GameObject spawn_parent = new GameObject("SaveFileSpawns");
            string scene = SceneNav.GetCurrentScene();
            prev_day = pdata.day;

            //Set camera
            TheCamera.Get().transform.position = pdata.camera_pos;
            TheCamera.Get().transform.rotation = pdata.camera_rot;

            //Spawn buildings
            foreach (KeyValuePair<string, SaveConstructionData> elem in pdata.buildings)
            {
                if (elem.Value.spawned)
                {
                    Construction building = Construction.Spawn(elem.Key, elem.Value);
                    building?.transform.SetParent(spawn_parent.transform);
                }
            }

            //Spawn Characters
            foreach (KeyValuePair<string, SaveCharacterData> elem in pdata.characters)
            {
                if (elem.Value.spawned)
                {
                    Character character = Character.Spawn(elem.Key, elem.Value);
                    character?.transform.SetParent(spawn_parent.transform);
                }
            }

            //Spawn colonist
            foreach (KeyValuePair<string, SaveColonistData> elem in pdata.colonists)
            {
                if (elem.Value.spawned)
                {
                    Colonist colonist = Colonist.Spawn(elem.Key, elem.Value);
                    colonist?.transform.SetParent(spawn_parent.transform);
                }
            }

            //Spawn NPC
            foreach (KeyValuePair<string, SaveNPCData> elem in pdata.npcs)
            {
                if (elem.Value.spawned)
                {
                    NPC npc = NPC.Spawn(elem.Key, elem.Value);
                    npc?.transform.SetParent(spawn_parent.transform);
                }
            }

            //Spawn others
            foreach (KeyValuePair<string, SaveSpawnedData> elem in pdata.spawned_objects)
            {
                if (elem.Value.spawned)
                {
                    GameObject obj = Spawnable.Spawn(elem.Key, elem.Value);
                    obj?.transform.SetParent(spawn_parent.transform);
                }
            }

            //Spawn zones
            foreach (KeyValuePair<string, SaveZoneData> elem in pdata.zones)
            {
                if (elem.Value.spawned)
                {
                    Zone zone = Zone.Spawn(elem.Key, elem.Value);
                    zone?.transform.SetParent(spawn_parent.transform);
                }
            }

            //Spawn items
            foreach (KeyValuePair<string, SaveItemData> elem in pdata.items)
            {
                if (elem.Value.spawned)
                {
                    Item item = Item.Spawn(elem.Key, elem.Value);
                    item?.transform.SetParent(spawn_parent.transform);
                }
            }

            //Set characters position
            foreach (KeyValuePair<string, SaveCharacterData> elem in pdata.characters)
            {
                Character character = Character.Get(elem.Key);
                if (character != null)
                {
                    character.transform.position = elem.Value.pos;
                    character.transform.rotation = elem.Value.rot;
                    character.SetFacing(character.transform.forward);
                    character.StopMove();
                }
            }

            //Set current scene
            pdata.current_scene = scene;
        }

        private void AfterStart()
        {
            inited = true;
            onStart?.Invoke();

            //New game
            SaveData pdata = SaveData.Get();
            if (pdata.IsNewGame())
            {
                pdata.play_time = 0.01f; //Initialize play time to 0.01f to make sure onStartNewGame never get called again
                onStartNewGame?.Invoke();
            }
            else
            {
                onStartLoad?.Invoke();
            }

            //Audio
            AudioClip[] playlist = GameData.Get().music_playlist;
            if (playlist.Length > 0)
                TheAudio.Get().PlayMusic("music", playlist[Random.Range(0, playlist.Length)]);
        }

        void Update()
        {
            if (!inited)
                AfterStart();

            if (IsPaused())
                return;

            //Game speed
            game_speed_per_hour = speed_multiplier * GameData.Get().game_speed;
            game_speed_per_sec = game_speed_per_hour / 3600f;

            //Game time
            SaveData pdata = SaveData.Get();
            pdata.day_time += game_speed_per_sec * Time.deltaTime;
            if (pdata.day_time >= 24f)
            {
                pdata.day_time = 0f;
                pdata.day++; //New day
            }

            //Play time
            pdata.play_time += Time.deltaTime;

            //NewDay
            if (pdata.day > prev_day)
            {
                prev_day = pdata.day;
                onNewDay?.Invoke();
            }
        }

        //Set to 1f for default speed
        public void SetGameSpeedMultiplier(float mult)
        {
            speed_multiplier = mult;
        }

        public float GetSpeedMultiplier()
        {
            return speed_multiplier;
        }

        //Game hours per real time hours
        public float GetGameTimeSpeedPerHour()
        {
            return game_speed_per_hour;
        }

        //Game hours per real time seconds
        public float GetGameTimeSpeed()
        {
            return game_speed_per_sec;
        }

        //Get timestamp since the start of the game in game-hours
        public float GetTimestamp()
        {
            SaveData sdata = SaveData.Get();
            return sdata.day * 24f + sdata.day_time;
        }

        public bool IsNight()
        {
            SaveData pdata = SaveData.Get();
            return pdata.day_time >= 18f || pdata.day_time < 6f;
        }

        //Pause - Unpause
        public void Pause()
        {
            paused = true;
        }

        public void Unpause()
        {
            paused = false;
        }

        public void PauseScript()
        {
            paused_gameplay = true;
        }

        public void UnpauseScript()
        {
            paused_gameplay = false;
        }

        public bool IsPaused()
        {
            return paused || paused_gameplay || speed_multiplier < 0.001f;
        }

        //-- Scene transition -----

        public void TransitionToScene(string scene, int entry_index)
        {
            if (!scene_transition)
            {
                if (SceneNav.DoSceneExist(scene))
                {
                    scene_transition = true;
                    StartCoroutine(GoToSceneRoutine(scene, entry_index));
                }
                else
                {
                    Debug.Log("Scene don't exist: " + scene);
                }
            }
        }

        private IEnumerator GoToSceneRoutine(string scene, int entry_index)
        {
            BlackPanel.Get().Show();
            yield return new WaitForSeconds(1f);
            TheGame.GoToScene(scene, entry_index);
        }

        public static void GoToScene(string scene, int entry_index = 0)
        {
            if (!string.IsNullOrEmpty(scene))
            {
                SaveData pdata = SaveData.Get();
                if (pdata != null)
                {
                    pdata.current_scene = scene;
                }

                SceneNav.GoTo(scene);
            }
        }

        //---- Load / Save -----

        //Save is not static, because a scene and save file must be loaded before you can save
        public void Save()
        {
            Save(SaveData.Get().filename);
        }

        public bool Save(string filename)
        {
            if (!SaveSystem.IsValidFilename(filename))
                return false; //Failed

            SaveData.Get().current_scene = SceneNav.GetCurrentScene();
            SaveData.Get().camera_pos = TheCamera.Get().transform.position;
            SaveData.Get().camera_rot = TheCamera.Get().transform.rotation;

            beforeSave?.Invoke(filename);

            SaveData.Save(filename, SaveData.Get());
            return true;
        }

        public static void Load()
        {
            Load(SaveData.GetLastSave());
        }

        public static bool Load(string filename)
        {
            if (!SaveSystem.IsValidFilename(filename))
                return false; //Failed

            SaveData.Unload(); //Make sure to unload first, or it won't load if already loaded
            SaveData.AutoLoad(filename);

            afterLoad?.Invoke();

            SceneNav.GoTo(SaveData.Get().current_scene);
            return true;
        }

        public static void NewGame()
        {
            NewGame(SaveData.GetLastSave(), SceneNav.GetCurrentScene());
        }

        public static bool NewGame(string filename, string scene)
        {
            if (!SaveSystem.IsValidFilename(filename))
                return false; //Failed

            SaveData.NewGame(filename);

            afterNewGame?.Invoke();

            SceneNav.GoTo(scene);
            return true;
        }

        public static void DeleteGame(string filename)
        {
            SaveData.Delete(filename);
        }

        //---------

        public static bool IsMobile()
        {
#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN
            return true;
#elif UNITY_WEBGL
            return WebGLTool.isMobile();
#else
            return false;
#endif
        }

        public static TheGame Get()
        {
            if (instance == null)
                instance = FindObjectOfType<TheGame>();
            return instance;
        }
    }
}
