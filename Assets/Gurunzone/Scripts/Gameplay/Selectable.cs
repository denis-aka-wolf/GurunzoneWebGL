using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gurunzone
{

    public class Selectable : MonoBehaviour
    {
        public float selection_size = 5f;   //Size of the visual circle when selected
        public int select_priority = 0;    //When selecting multiple objects, only the highest priority will be selected

        [Header("Groups")]
        public GroupData[] groups;          //Group to filter this selectable

        [Header("FX")]
        public GameObject outline;          //Outline sub-mesh, optional, will show this mesh when highlighted in a zone.

        private Transform transf;
        private GameObject select_fx;
        private bool selected = false;
        private bool outlined = false;
        private bool removed = false;

        public UnityAction onSelect;
        public UnityAction onUnselect;
        public UnityAction onRemove;

        [HideInInspector]
        public bool selection_active = true; //Set off to prevent selecting

        public static UnityAction onSelectAny;

        private Interactable interact; //May be null
        private Destructible destruct; //May be null
        private Colonist colonist; //May be null
        private UniqueID uid; //May be null

        private static List<Selectable> select_list = new List<Selectable>();
        private static List<Selectable> selected_list = new List<Selectable>();

        private void Awake()
        {
            select_list.Add(this);
            transf = transform;
            interact = GetComponent<Interactable>();
            destruct = GetComponent<Destructible>();
            colonist = GetComponent<Colonist>();
            uid = GetComponent<UniqueID>();
            SetOutline(false);
            InitSelectFX();
        }

        private void OnDestroy()
        {
            select_list.Remove(this);
            selected_list.Remove(this);
        }

        private void Start()
        {
            if (uid != null && uid.HasUID() && SaveData.Get().IsObjectRemoved(uid.uid))
                Destroy(gameObject);
        }

        private void InitSelectFX()
        {
            if (AssetData.Get().selection_fx != null)
            {
                select_fx = Instantiate(AssetData.Get().selection_circle, transform);
                select_fx.transform.localPosition = Vector3.zero;
                select_fx.transform.localScale = Vector3.one * selection_size;
                select_fx.SetActive(false);
            }
        }

        public void Select()
        {
            if (!selected)
            {
                selected = true;
                if (!selected_list.Contains(this))
                    selected_list.Add(this);
                if (select_fx != null && !select_fx.activeSelf)
                    select_fx.SetActive(true);
                onSelect?.Invoke();
                onSelectAny?.Invoke();
            }
        }

        public void Unselect()
        {
            if (selected)
            {
                selected = false;
                selected_list.Remove(this);
                if (select_fx != null && select_fx.activeSelf)
                    select_fx.SetActive(false);
                onUnselect?.Invoke();
            }
        }

        public void SetOutline(bool outline_on)
        {
            outlined = outline_on;
            if (outline != null && outline.activeSelf != outline_on)
                outline.SetActive(outline_on);
        }

        public bool IsInsideBox(Vector3 rect_origin, Vector3 rect_front, Vector3 rect_side)
        {
            Vector3 pos = transf.position;
            Vector3 dir = pos - rect_origin;
            Vector3 extend = rect_front + rect_side;
            if (dir.magnitude > extend.magnitude)
                return false; //Optimization

            Vector3 opposite = rect_origin + extend;
            Vector3 odir = pos - opposite;
            float d1 = Vector3.Dot(dir, rect_front);
            float d2 = Vector3.Dot(dir, rect_side);
            float d3 = Vector3.Dot(odir, -rect_front);
            float d4 = Vector3.Dot(odir, -rect_side);
            return d1 >= 0f && d2 >= 0f && d3 >= 0f && d4 >= 0f;
        }

        public bool IsNearCamera(float distance)
        {
            float dist = (transf.position - TheCamera.Get().GetTargetPos()).magnitude;
            return dist < distance;
        }

        public void Kill(float delay=0f)
        {
            if (destruct != null)
                destruct.Kill();
            else
                Remove(delay);
        }

        public void Remove(float delay = 0f)
        {
            if (!removed)
            {
                removed = true;
                SaveData.Get().RemoveAll(UID);
                uid?.RemoveAllSubUIDs();
                onRemove?.Invoke();
                Destroy(gameObject, delay);
            }
        }

        public void DoSelectFX()
        {
            StartCoroutine(SelectFXRun());
        }

        private IEnumerator SelectFXRun()
        {
            select_fx?.SetActive(false);
            yield return new WaitForSeconds(0.1f);
            select_fx?.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            select_fx?.SetActive(false);
            yield return new WaitForSeconds(0.2f);
            select_fx?.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            select_fx?.SetActive(selected);
        }

        public bool IsSelected()
        {
            return selected;
        }

        public bool IsOutlined()
        {
            return outlined;
        }

        public bool IsInRange(Vector3 pos, float range)
        {
            float dist = (pos - transform.position).magnitude;
            return dist < range;
        }

        public bool HasGroup(GroupData group)
        {
            if (group == null)
                return true;

            foreach (GroupData agroup in groups)
            {
                if (agroup == group)
                    return true;
            }
            return false;
        }

        public int GetSelectionIndex()
        {
            int index = 0;
            foreach (Selectable aselect in selected_list)
            {
                if (this == aselect)
                    return index;
                index++;
            }
            return -1; // not selected
        }

        public Interactable Interactable { get { return interact; } } //May be null
        public Destructible Destructible { get { return destruct; } } //May be null
        public Transform Transform { get { return transf; } } //Optimization
        public string UID { get { return uid != null ? uid.uid : ""; } }

        public static void SelectAll(Vector3 rect_origin, Vector3 rect_front, Vector3 rect_side)
        {
            int highest_priority = int.MinValue;
            List<Selectable> in_box = new List<Selectable>();
            foreach (Selectable select in select_list)
            {
                if (select.IsInsideBox(rect_origin, rect_front, rect_side))
                {
                    in_box.Add(select);
                    highest_priority = Mathf.Max(highest_priority, select.select_priority);
                }
            }

            foreach (Selectable select in in_box)
            {
                if (select.select_priority >= highest_priority)
                    select.Select();
            }
        }

        public static void UnselectAll()
        {
            foreach (Selectable select in select_list)
                select.Unselect();
        }

        public static void SetOutlineAll(bool outline_on)
        {
            foreach (Selectable select in select_list)
                select.SetOutline(outline_on);
        }

        public static Selectable GetRaycast(Ray ray)
        {
            RaycastHit[] hits = Physics.RaycastAll(ray);
            foreach (RaycastHit hit in hits)
            {
                Selectable select = hit.collider?.gameObject.GetComponent<Selectable>();
                if(select != null)
                    return select;
            }
            return null;
        }


        public static Selectable Get(string uid)
        {
            foreach (Selectable select in select_list)
            {
                if (select.UID == uid)
                    return select;
            }
            return null;
        }

        public static Selectable GetNearestBySize(Vector3 pos, float range = 999f)
        {
            Selectable select = GetNearest(pos, range);
            if (select != null && select.selection_active)
            {
                float dist = (select.transf.position - pos).magnitude;
                if (dist < select.selection_size)
                    return select;
            }
            return null;
        }

        public static Selectable GetNearest(GroupData group, Vector3 pos, float range = 999f)
        {
            Selectable nearest = null;
            float min_dist = range;
            foreach (Selectable select in select_list)
            {
                float dist = (select.transf.position - pos).magnitude;
                if (dist < min_dist && select.HasGroup(group) && select.selection_active)
                {
                    min_dist = dist;
                    nearest = select;
                }
            }
            return nearest;
        }

        public static Selectable GetNearest(Vector3 pos, float range = 999f)
        {
            Selectable nearest = null;
            float min_dist = range;
            foreach (Selectable select in select_list)
            {
                float dist = (select.transf.position - pos).magnitude;
                if (dist < min_dist && select.selection_active)
                {
                    min_dist = dist;
                    nearest = select;
                }
            }
            return nearest;
        }

        public static Selectable GetRandom(GroupData group)
        {
            List<Selectable> group_list = GetAllGroup(group);
            if (group_list.Count > 0)
                return group_list[Random.Range(0, group_list.Count)];
            return null;
        }

        public static List<Selectable> GetRandom(GroupData group, int quantity)
        {
            List<Selectable> group_list = GetAllGroup(group);
            List<Selectable> random = new List<Selectable>();
            for (int i = 0; i < quantity; i++)
            {
                if (group_list.Count > 0)
                {
                    Selectable sel = group_list[Random.Range(0, group_list.Count)];
                    random.Add(sel);
                    group_list.Remove(sel);
                }
            }
            return random;
        }

        public static int GetSelectionCount()
        {
            return selected_list.Count;
        }

        public static bool IsColonistSelected()
        {
            foreach (Selectable select in selected_list)
            {
                if (select.colonist != null)
                    return true;
            }
            return false;
        }

        public static List<Selectable> GetAllGroup(GroupData group)
        {
            List<Selectable> valid_list = new List<Selectable>();
            foreach (Selectable select in select_list)
            {
                if (group == null || select.HasGroup(group))
                    valid_list.Add(select);
            }
            return valid_list;
        }

        public static List<Selectable> GetAllSelected()
        {
            return selected_list;
        }

        public static List<Selectable> GetAllActive()
        {
            return select_list;
        }

        public static List<Selectable> GetAll()
        {
            return select_list;
        }

    }
}
