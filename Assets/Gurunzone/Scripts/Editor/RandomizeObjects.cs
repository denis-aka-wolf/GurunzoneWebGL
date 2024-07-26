using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Gurunzone.EditorTool
{

    /// <summary>
    /// Offset all selected objects position in X and Z by a random value so that they look more natural
    /// (useful when copying a lot of tree groups and you dont want their position pattern to repeat)
    /// </summary>

    public class RandomizeObjects : ScriptableWizard
    {
        public float noise_dist = 1f;

        [MenuItem("Gurunzone/Randomize Objects", priority = 302)]
        static void ScriptableWizardMenu()
        {
            ScriptableWizard.DisplayWizard<RandomizeObjects>("Randomize Objects", "Randomize Objects");
        }

        void DoRandomize()
        {
            Undo.RegisterCompleteObjectUndo(Selection.transforms, "randomize");
            foreach (Transform transform in Selection.transforms)
            {
                DoRandomize(transform);
            }
        }

        void DoRandomize(Transform transform)
        {
            Vector3 offset = new Vector3(Random.Range(-noise_dist, noise_dist), 0f, Random.Range(-noise_dist, noise_dist));
            transform.position += offset;
        }

        void OnWizardCreate()
        {
            DoRandomize();
        }

        void OnWizardUpdate()
        {
            helpString = "Use this to add a random offset to the position of all selected objects.";
        }
    }

}