using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// Just holds the SpawnData of an object, useful for the Spawner.cs, to prevent spawn too many of same object
    /// </summary>

    [RequireComponent(typeof(UniqueID))]
    public class Spawnable : CSObject
    {
        public SpawnData data;

        public static GameObject Spawn(string uid, SaveSpawnedData data)
        {
            SpawnData sdata = SpawnData.Get(data.id);
            if (sdata != null && data.scene == SceneNav.GetCurrentScene())
            {
                GameObject obj = Instantiate(sdata.prefab, data.pos, data.rot);
                UniqueID uniqueid = obj.GetComponent<UniqueID>();
                uniqueid.uid = uid;
                return obj;
            }
            return null;
        }

        public static GameObject Create(SpawnData data, Vector3 pos)
        {
            return Create(data, pos, data.prefab.transform.rotation);
        }

        //Make sure prefab has a UniqueID
        public static GameObject Create(SpawnData data, Vector3 pos, Quaternion rot)
        {
            GameObject obj = Instantiate(data.prefab, pos, rot);
            Spawnable spawnable = obj.GetComponent<Spawnable>();
            if(spawnable != null)
                spawnable.data = data;
            UniqueID uid = obj.GetComponent<UniqueID>();
            uid.uid = UniqueID.GenerateUniqueID();
            SaveData sdata = SaveData.Get();
            SaveSpawnedData spdata = sdata.GetSpawnedObject(uid.uid); //Create obj in save file
            spdata.id = data.id;
            spdata.scene = SceneNav.GetCurrentScene();
            spdata.pos = pos;
            spdata.rot = rot;
            spdata.spawned = true;
            return obj;
        }
    }
}
