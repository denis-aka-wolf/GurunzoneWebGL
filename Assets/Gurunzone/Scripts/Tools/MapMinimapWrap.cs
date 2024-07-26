using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if MAP_MINIMAP
using MapMinimap;
#endif

namespace Gurunzone
{
    /// <summary>
    /// Wrapper class for Map Minimap
    /// </summary>

    public class MapMinimapWrap : MonoBehaviour
    {

#if MAP_MINIMAP

        static MapMinimapWrap()
        {
            TheGame.afterLoad += ReloadMM;
            TheGame.afterNewGame += NewMM;
        }

        void Awake()
        {
            SaveData.LoadLast(); //Make sure the game is loaded

            TheGame the_game = FindObjectOfType<TheGame>();
            MapManager map_manager = FindObjectOfType<MapManager>();

            if (map_manager != null)
            {
                map_manager.show_player_warning = false;
                map_manager.onOpenMap += OnOpen;
                map_manager.onCloseMap += OnClose;
            }
            else
            {
                Debug.LogError("Map Minimap: Integration failed - Make sure to add the MapManager to the scene");
            }

            if (the_game != null)
            {
                the_game.beforeSave += SaveDQ;
                LoadMM();
            }
        }

        private void Start()
        {
            Minimap minimap = Minimap.Get();
            if (minimap)
            {
                minimap.map_zoom = 0f;
                minimap.open_on_click = false;
                minimap.GetViewer().onClickHold = OnClickMap;
            }
        }

        private void Update()
        {
            
        }

        private void OnClickMap(Vector2 map_pos)
        {
            Minimap minimap = Minimap.Get();
            if (minimap)
            {
                Vector3 center = minimap.GetViewer().GetCurrentWorldPos();
                MapZone zone = MapZone.Get(center);
                Vector3 wpos = MapTool.MapToWorldPos(zone, map_pos);
                TheCamera.Get().SmoothToTarget(wpos);
            }
        }

        private static void ReloadMM()
        {
            MapData.Unload();
            LoadMM();
        }

        private static void NewMM()
        {
            SaveData pdata = SaveData.Get();
            if (pdata != null)
            {
                MapData.Unload();
                MapData.NewGame(pdata.filename);
            }
        }

        private static void LoadMM()
        {
            SaveData pdata = SaveData.Get();
            if (pdata != null)
            {
                MapData.AutoLoad(pdata.filename);
            }
        }

        private void SaveDQ(string filename)
        {
            if (MapData.Get() != null && !string.IsNullOrEmpty(filename))
            {
                MapData.Save(filename, MapData.Get());
            }
        }

        private void OnOpen()
        {
            TheGame.Get().PauseScript();
        }

        private void OnClose()
        {
            TheGame.Get().UnpauseScript();
        }

#endif

    }
}

