using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalFlock : MonoBehaviour
{
    [Range(0, 100)]
    public int Velocity = 10;
    public int NumSpawn = 10;
    [Range(0, 100)]
    public int GoalFrequency = 8;
    [Range(1.0f, 10.0f)]
    public float TurnSpeed = 5.0f;
    [Range(1, 20)]
    public int TurnProximity = 10;

    public GameObject[] Prefabs;
    GameObject BoidManager;
    public GameObject FreedomBox;
    public GameObject goalPositionDebug;
    public bool DebugVisibility = false;
    [HideInInspector]
    public GameObject[] AllFish;

#if UNITY_EDITOR
    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += Callback;
    }

    private void Callback()
    {
        if (this == null)
        {
            UnityEditor.EditorApplication.delayCall -= Callback;
            return; // MissingRefException if managed in the editor - uses the overloaded Unity == operator.
        }

        if (DebugVisibility)
        {
            goalPositionDebug.GetComponent<MeshRenderer>().enabled = true;
            DebugVisibility = true;
        }
        else
        {
            goalPositionDebug.GetComponent<MeshRenderer>().enabled = false;
            DebugVisibility = false;
        }
    }
#endif

    private void Awake()
    {
        BoidManager = this.transform.gameObject;
    }

    // Start is called before the first frame update
    void Start()
    {
        DebugVisibility = false;
        goalPositionDebug.transform.position = GetRandomPointInsideCollider(FreedomBox.GetComponent<BoxCollider>());

        AllFish = new GameObject[NumSpawn];

        for (int i = 0; i < NumSpawn; i++)
        {
            int PrefabNr = Random.Range(0, Prefabs.Length);
            Vector3 pos = GetRandomPointInsideCollider(BoidManager.GetComponent<BoxCollider>());

            GameObject newFish = Instantiate(Prefabs[PrefabNr], pos, Quaternion.identity, FreedomBox.transform);            
            newFish.AddComponent<Flock>();
            AllFish[i] = newFish;
        }
    }
    // Update is called once per frame
    void Update()
    {
         if (Random.Range(0, 10000) < GoalFrequency)
         {
            goalPositionDebug.transform.position = GetRandomPointInsideCollider(FreedomBox.GetComponent<BoxCollider>());
         }
    }

    public Vector3 GetRandomPointInsideCollider(BoxCollider boxCollider)
    {
        Vector3 extents = boxCollider.size / 2f;
        Vector3 point = new Vector3(
                                    Random.Range(-extents.x, extents.x),
                                    Random.Range(-extents.y, extents.y),
                                    Random.Range(-extents.z, extents.z)) + boxCollider.center;

        return boxCollider.transform.TransformPoint(point);
    }
}
