using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{
    /// <summary>
    /// SpawnGroupData will spawn a random object in the list, instead of a specific one, when spawned
    /// </summary>

    [CreateAssetMenu(fileName = "SpawnGroupData", menuName = "Gurunzone/AppData/SpawnGroupData", order = 10)]
    public class SpawnGroupData : SpawnData
    {
        [Header("Spawn Data")]
        public SpawnData[] values;

        public SpawnData GetRandomData()
        {
            if (values.Length > 0)
                return values[Random.Range(0, values.Length)];
            return null;
        }

        public SpawnData GetRandomUniqueData(List<SpawnData> existing)
        {
            List<SpawnData> valid_items = new List<SpawnData>();
            foreach (SpawnData data in values)
            {
                if (!existing.Contains(data))
                    valid_items.Add(data);
            }
            if (valid_items.Count > 0)
                return valid_items[Random.Range(0, valid_items.Count)];
            return null;
        }

    }
}
