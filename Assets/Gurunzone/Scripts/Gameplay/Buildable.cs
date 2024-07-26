using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Gurunzone
{
    public enum BuildableType
    {
        Default = 0,
        Grid = 10,
        Snap = 20, //Snap to an existing object
    }

    /// <summary>
    /// Buildable is the base script for objects that can be placed on the map and have a build mode (transparent version of model that follow mouse)
    /// These objects are placed manually by the player, it will follow the mouse until clicked.
    /// </summary>

    public class Buildable : MonoBehaviour
    {
        [Header("Buildable")]
        public BuildableType type;
        public float grid_size = 1f;
        public GroupData snap_group;

        [Header("Build Obstacles")]
        public LayerMask floor_layer = 1 << 9; //Will build on top of this layer
        public LayerMask obstacle_layer = 1; //Cant build if of those obstacles
        public float build_obstacle_radius = 1; //Can't build if obstacles within radius
        public float build_ground_dist = 0.4f; //Ground must be at least this distance on all 4 sides to build (prevent building on a wall or in the air)
        public bool trigger_is_obstacle = true; //If true, colliders that are set to IsTrigger will also be obstacles when building
        public bool require_flat_floor = true;        //If true, must be built on flat surface

        [Header("FX")]
        public AudioClip build_audio;
        public GameObject build_fx;

        public UnityAction onBuild;

        protected Selectable selectable; //Can be nulls
        protected Destructible destruct; //Can be nulls
        protected UniqueID uid; //Can be nulls

        private bool build_mode = false; //Building mode means player is selecting where to build it, but it doesnt really exists yet
        private bool visible_set = true;
        private bool killed = false;
        private Color prev_color = Color.white;
        private float manual_rotate = 0f;
        private float update_timer = 0f;

        private List<Collider> colliders = new List<Collider>();
        private List<MeshRenderer> renders = new List<MeshRenderer>();
        private List<Material> materials = new List<Material>();
        private List<Material> materials_transparent = new List<Material>();
        private List<Color> materials_color = new List<Color>();

        void Awake()
        {
            selectable = GetComponent<Selectable>();
            destruct = GetComponent<Destructible>();
            uid = GetComponent<UniqueID>();
            renders.AddRange(GetComponentsInChildren<MeshRenderer>());

            foreach (MeshRenderer render in renders)
            {
                foreach (Material material in render.sharedMaterials)
                {
                    bool valid_mat = material && MaterialTool.HasColor(material);
                    Material material_normal = valid_mat ? new Material(material) : null;
                    Material material_trans = valid_mat ? new Material(material) : null;
                    if (material_trans != null)
                        MaterialTool.ChangeRenderMode(material_trans, BlendMode.Fade);
                    materials.Add(material_normal);
                    materials_transparent.Add(material_trans);
                    materials_color.Add(valid_mat ? material.color : Color.white);
                }
            }

            foreach (Collider collide in GetComponentsInChildren<Collider>())
            {
                if (collide.enabled && !collide.isTrigger)
                {
                    colliders.Add(collide);
                }
            }
        }

        void Start()
        {
            TheControls.Get().onClick += OnClick;
            TheControls.Get().onRelease += OnRelease;
            TheControls.Get().onRClick += OnRightClick;
        }

        private void OnDestroy()
        {
            TheControls.Get().onClick -= OnClick;
            TheControls.Get().onRelease -= OnRelease;
            TheControls.Get().onRClick -= OnRightClick;
        }

        void Update()
        {
            if (build_mode && !killed)
            {
                TheControls controls = TheControls.Get();

                //Rotate
                manual_rotate += controls.GetRotateBuild() * 100f * Time.deltaTime;

                //Mouse/Touch controls
                transform.position = controls.GetMouseWorldPos();
                transform.rotation = Quaternion.Euler(0f, manual_rotate, 0f) * TheCamera.Get().GetFacingRotation();

                //Snap to grid
                FindAutoPosition();

                //Show/Hide on mobile
                if (TheGame.IsMobile())
                    SetBuildVisible(controls.IsMouseHold());

                //Build color
                bool can_build = CheckIfCanBuild();
                Color color = can_build ? Color.white : Color.red * 0.9f;
                SetModelColor(new Color(color.r, color.g, color.b, 0.5f), !can_build);
            }

            update_timer += Time.deltaTime;
            if (update_timer > 0.5f)
            {
                update_timer = Random.Range(-0.02f, 0.02f);
                SlowUpdate(); //Optimization
            }

        }

        private void SlowUpdate()
        {
            
        }

        public void StartBuild()
        {
            if (killed)
                return;

            build_mode = true;

            if (selectable != null)
                selectable.enabled = false;
            if (destruct)
                destruct.enabled = false;

            foreach (Collider collide in colliders)
                collide.isTrigger = true;

            TheControls.Get().SetBuildMode(this);
            SetBuildVisible(!TheGame.IsMobile());
        }

        public void FinishBuild()
        {
            if (killed)
                return;

            gameObject.SetActive(true);
            build_mode = false;

            foreach (Collider collide in colliders)
                collide.isTrigger = false;

            if (selectable != null)
                selectable.enabled = true;

            if (destruct)
                destruct.enabled = true;

            SetModelColor(Color.white, false);
            TheControls.Get().SetSelectMode();

            if (build_fx != null)
                Instantiate(build_fx, transform.position, Quaternion.identity);
            TheAudio.Get().PlaySFX3D("build", build_audio, transform.position);

            if (onBuild != null)
                onBuild.Invoke();
        }

        public void CancelBuild()
        {
            if (build_mode)
            {
                TheControls.Get().SetSelectMode();
                Selectable.SetOutlineAll(false);
                killed = true;
                Destroy(gameObject);
            }
        }

        //Call this function to rotate building manually (like by using a key)
        public void RotateManually(float angle_y)
        {
            if (build_mode)
            {
                manual_rotate += angle_y;
            }
        }

        private void SetBuildVisible(bool visible)
        {
            if (visible_set != visible)
            {
                visible_set = visible;
                foreach (MeshRenderer mesh in renders)
                    mesh.enabled = visible;
            }
        }

        private void SetModelColor(Color color, bool replace)
        {
            if (color != prev_color)
            {
                int index = 0;
                foreach (MeshRenderer render in renders)
                {
                    Material[] mesh_materials = render.sharedMaterials;
                    for (int i = 0; i < mesh_materials.Length; i++)
                    {
                        if (index < materials.Count && index < materials_transparent.Count)
                        {
                            Material mesh_mat = mesh_materials[i];
                            Material ref_mat = color.a < 0.99f ? materials_transparent[index] : materials[index];
                            if (ref_mat != null)
                            {
                                ref_mat.color = materials_color[index] * color;
                                if (replace)
                                    ref_mat.color = color;
                                if (ref_mat != mesh_mat)
                                    mesh_materials[i] = ref_mat;
                            }
                        }
                        index++;
                    }
                    render.sharedMaterials = mesh_materials;
                }
            }

            prev_color = color;
        }

        //Check if possible to build at current position
        public bool CheckIfCanBuild()
        {
            bool dont_overlap = !CheckIfOverlap();
            bool flat_ground = CheckIfFlatGround();
            bool valid_ground = CheckValidFloor();
            bool valid_snap = CheckValidSnap();
            bool gameplay = TheControls.Get().IsInGameplay();
            //Debug.Log(dont_overlap + " " + flat_ground + " " + accessible + " " + valid_ground);
            return dont_overlap && flat_ground && valid_ground && valid_snap && gameplay;
        }

        //Check if overlaping another object (cant build)
        public bool CheckIfOverlap()
        {
            List<Collider> overlap_colliders = new List<Collider>();
            LayerMask olayer = obstacle_layer & ~floor_layer;  //Remove floor layer from obstacles

            //Check collision with bounding box
            foreach (Collider collide in colliders)
            {
                Collider[] over = Physics.OverlapBox(transform.position, collide.bounds.extents, Quaternion.identity, olayer);
                foreach (Collider overlap in over)
                {
                    if (!overlap.isTrigger)
                        overlap_colliders.Add(overlap);
                }
            }

            //Check collision with radius (includes triggers)
            if (build_obstacle_radius > 0.01f)
            {
                QueryTriggerInteraction trigger = trigger_is_obstacle ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;
                Collider[] over = Physics.OverlapSphere(transform.position, build_obstacle_radius, olayer, trigger);
                overlap_colliders.AddRange(over);
            }

            //Check collision list
            foreach (Collider overlap in overlap_colliders)
            {
                if (overlap != null)
                {
                    //Dont overlap with player and dont overlap with itself
                    Buildable buildable = overlap.GetComponentInParent<Buildable>();
                    if (buildable != this)
                        return true;
                }
            }

            return false;
        }

        //Check if there is a flat floor underneath (can't build a steep cliff)
        public bool CheckIfFlatGround()
        {
            if (!require_flat_floor)
                return true; //Dont check for flat ground

            Vector3 center = transform.position + Vector3.up * build_ground_dist;
            Vector3 p0 = center;
            Vector3 p1 = center + Vector3.right * build_obstacle_radius;
            Vector3 p2 = center + Vector3.left * build_obstacle_radius;
            Vector3 p3 = center + Vector3.forward * build_obstacle_radius;
            Vector3 p4 = center + Vector3.back * build_obstacle_radius;
            Vector3 dir = Vector3.down * (build_ground_dist + build_ground_dist);

            RaycastHit h0, h1, h2, h3, h4;
            bool f0 = PhysicsTool.RaycastCollision(p0, dir, out h0);
            bool f1 = PhysicsTool.RaycastCollision(p1, dir, out h1);
            bool f2 = PhysicsTool.RaycastCollision(p2, dir, out h2);
            bool f3 = PhysicsTool.RaycastCollision(p3, dir, out h3);
            bool f4 = PhysicsTool.RaycastCollision(p4, dir, out h4);

            return f0 && f1 && f2 && f3 && f4;
        }

        //Check if ground is valid layer, this one check only the first hit collision (more strict)
        public bool CheckValidFloor()
        {
            Vector3 center = transform.position + Vector3.up * build_ground_dist;
            Vector3 p0 = center;
            Vector3 p1 = center + Vector3.right * build_obstacle_radius;
            Vector3 p2 = center + Vector3.left * build_obstacle_radius;
            Vector3 p3 = center + Vector3.forward * build_obstacle_radius;
            Vector3 p4 = center + Vector3.back * build_obstacle_radius;
            Vector3 dir = Vector3.down * (build_ground_dist + build_ground_dist);

            RaycastHit h0, h1, h2, h3, h4;
            bool f0 = PhysicsTool.RaycastCollision(p0, dir, out h0);
            bool f1 = PhysicsTool.RaycastCollision(p1, dir, out h1);
            bool f2 = PhysicsTool.RaycastCollision(p2, dir, out h2);
            bool f3 = PhysicsTool.RaycastCollision(p3, dir, out h3);
            bool f4 = PhysicsTool.RaycastCollision(p4, dir, out h4);
            f0 = f0 && PhysicsTool.IsLayerInLayerMask(h0.collider.gameObject.layer, floor_layer);
            f1 = f1 && PhysicsTool.IsLayerInLayerMask(h1.collider.gameObject.layer, floor_layer);
            f2 = f2 && PhysicsTool.IsLayerInLayerMask(h2.collider.gameObject.layer, floor_layer);
            f3 = f3 && PhysicsTool.IsLayerInLayerMask(h3.collider.gameObject.layer, floor_layer);
            f4 = f4 && PhysicsTool.IsLayerInLayerMask(h4.collider.gameObject.layer, floor_layer);

            return f1 || f2 || f3 || f4 || f0; //Floor must be valid only on one side
        }

        //Check if its still valid floor after built, this one ignore itself and check only the layer (less strict)
        public bool CheckValidFloorBuilt()
        {
            Vector3 center = transform.position + Vector3.up * build_ground_dist;
            Vector3 p0 = center;
            Vector3 p1 = center + Vector3.right * build_obstacle_radius;
            Vector3 p2 = center + Vector3.left * build_obstacle_radius;
            Vector3 p3 = center + Vector3.forward * build_obstacle_radius;
            Vector3 p4 = center + Vector3.back * build_obstacle_radius;
            Vector3 dir = Vector3.down * (build_ground_dist + build_ground_dist);

            RaycastHit h0, h1, h2, h3, h4;
            bool f0 = PhysicsTool.RaycastCollisionLayer(p0, dir, floor_layer, out h0);
            bool f1 = PhysicsTool.RaycastCollisionLayer(p1, dir, floor_layer, out h1);
            bool f2 = PhysicsTool.RaycastCollisionLayer(p2, dir, floor_layer, out h2);
            bool f3 = PhysicsTool.RaycastCollisionLayer(p3, dir, floor_layer, out h3);
            bool f4 = PhysicsTool.RaycastCollisionLayer(p4, dir, floor_layer, out h4);

            return f1 || f2 || f3 || f4 || f0;
        }

        private bool CheckValidSnap()
        {
            if (type != BuildableType.Snap)
                return true;

            Selectable target = FindSnapTarget(transform.position);
            return target != null;
        }

        private void FindAutoPosition()
        {
            if (IsGrid())
            {
                transform.position = FindGridPosition(transform.position);
                transform.rotation = FindGridRotation(transform.rotation);
            }

            if (IsSnap())
            {
                transform.position = FindSnapPosition(transform.position);
                transform.rotation = FindSnapRotation(transform.position, transform.rotation);
            }

            transform.position = FindBuildPosition(transform.position, floor_layer);
        }

        //Find the height to build
        public Vector3 FindBuildPosition(Vector3 pos, LayerMask mask)
        {
            float offset = build_obstacle_radius;
            Vector3 center = pos + Vector3.up * offset;
            Vector3 p0 = center;
            Vector3 p1 = center + Vector3.right * build_obstacle_radius;
            Vector3 p2 = center + Vector3.left * build_obstacle_radius;
            Vector3 p3 = center + Vector3.forward * build_obstacle_radius;
            Vector3 p4 = center + Vector3.back * build_obstacle_radius;
            Vector3 dir = Vector3.down * (offset + build_ground_dist);

            RaycastHit h0, h1, h2, h3, h4;
            bool f0 = PhysicsTool.RaycastCollisionLayer(p0, dir, mask, out h0);
            bool f1 = PhysicsTool.RaycastCollisionLayer(p1, dir, mask, out h1);
            bool f2 = PhysicsTool.RaycastCollisionLayer(p2, dir, mask, out h2);
            bool f3 = PhysicsTool.RaycastCollisionLayer(p3, dir, mask, out h3);
            bool f4 = PhysicsTool.RaycastCollisionLayer(p4, dir, mask, out h4);

            Vector3 dist_dir = Vector3.down * build_obstacle_radius;
            if (f0 && h0.distance < dist_dir.magnitude) { dist_dir = Vector3.down * h0.distance; }
            if (f1 && h1.distance < dist_dir.magnitude) { dist_dir = Vector3.down * h1.distance; }
            if (f2 && h2.distance < dist_dir.magnitude) { dist_dir = Vector3.down * h2.distance; }
            if (f3 && h3.distance < dist_dir.magnitude) { dist_dir = Vector3.down * h3.distance; }
            if (f4 && h4.distance < dist_dir.magnitude) { dist_dir = Vector3.down * h4.distance; }

            return center + dist_dir;
        }

        public Vector3 FindGridPosition(Vector3 pos)
        {
            if (grid_size >= 0.1f)
            {
                float x = Mathf.RoundToInt(pos.x / grid_size) * grid_size;
                float z = Mathf.RoundToInt(pos.z / grid_size) * grid_size;
                return new Vector3(x, pos.y, z);

            }
            return pos;
        }

        public Quaternion FindGridRotation(Quaternion rot)
        {
            Vector3 euler = rot.eulerAngles;
            float angle = Mathf.RoundToInt(euler.y / 90f) * 90f;
            return Quaternion.Euler(euler.x, angle, euler.z);
        }

        public Vector3 FindSnapPosition(Vector3 pos)
        {
            Selectable target = FindSnapTarget(pos);
            if (target != null)
                return target.transform.position;
            return pos;
        }

        public Quaternion FindSnapRotation(Vector3 pos, Quaternion rot)
        {
            Selectable target = FindSnapTarget(pos);
            if (target != null)
                return target.transform.rotation;
            return rot;
        }

        private Selectable FindSnapTarget(Vector3 pos)
        {
            return Selectable.GetNearest(snap_group, pos, 5f);
        }

        private void OnClick(Vector3 wpos)
        {
            //Build on desktop
            if (!TheGame.IsMobile() && build_mode && CheckIfCanBuild())
                FinishBuild();
        }

        private void OnRelease(Vector3 wpos)
        {
            //Build on mobile
            if (TheGame.IsMobile() && build_mode && CheckIfCanBuild())
                FinishBuild();
        }

        private void OnRightClick(Vector3 wpos)
        {
            if (build_mode)
                CancelBuild();
        }

        public bool IsBuilding()
        {
            return build_mode;
        }

        public bool IsGrid()
        {
            return type == BuildableType.Grid;
        }

        public bool IsSnap()
        {
            return type == BuildableType.Snap;
        }

        public Destructible GetDestructible()
        {
            return destruct; //May be null
        }

        public Selectable Selectable { get { return selectable; } }
        public string UID { get { return uid != null ? uid.uid : ""; } }

        public Selectable GetSelectable()
        {
            return selectable;
        }

        public string GetUID()
        {
            if (uid != null)
                return uid.uid;
            return "";
        }
    }

}