using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Character_Edit
{
    [ExecuteInEditMode]
    public class CharacterEdit : MonoBehaviour
    {
        [Range(0, 100)]
        public int CharacterSmileSlider;

        [Range(0, 100)]
        public int CharacterFrownSlider;

        [Range(0, 100)]
        public int CharacterSizeSlider;

        void OnValidate()
        {
            SetSmileValueForGameObject(CharacterSmileSlider);
            SetFrownValueForGameObject(CharacterFrownSlider);
            SetSizeValueForGameObject(CharacterSizeSlider);
        }

        public void SetSmileValueForGameObject(int value)
        {
            foreach (Transform t in this.transform)
            {
                SkinnedMeshRenderer meshRenderer = t.GetComponent<SkinnedMeshRenderer>();
                if (meshRenderer != null && meshRenderer.sharedMesh.blendShapeCount > 0)
                {
                    for (int i = 0; i < meshRenderer.sharedMesh.blendShapeCount; i++)
                    {
                        if (meshRenderer.sharedMesh.GetBlendShapeName(i) == "Smile")
                        {
                            meshRenderer.SetBlendShapeWeight(i, value);
                        }
                    }
                }
            }
        }

        public void SetFrownValueForGameObject(int value)
        {
            foreach (Transform t in this.transform)
            {
                SkinnedMeshRenderer meshRenderer = t.GetComponent<SkinnedMeshRenderer>();
                if (meshRenderer != null && meshRenderer.sharedMesh.blendShapeCount > 0)
                {
                    for (int i = 0; i < meshRenderer.sharedMesh.blendShapeCount; i++)
                    {
                        if (meshRenderer.sharedMesh.GetBlendShapeName(i) == "Frown")
                        {
                            meshRenderer.SetBlendShapeWeight(i, value);
                        }
                    }
                }
            }
        }

        public void SetSizeValueForGameObject(int value)
        {
            foreach (Transform t in this.transform)
            {
                SkinnedMeshRenderer meshRenderer = t.GetComponent<SkinnedMeshRenderer>();
                if (meshRenderer != null && meshRenderer.sharedMesh.blendShapeCount > 0)
                {
                    for (int i = 0; i < meshRenderer.sharedMesh.blendShapeCount; i++)
                    {
                        if (meshRenderer.sharedMesh.GetBlendShapeName(i) == "Size")
                        {
                            meshRenderer.SetBlendShapeWeight(i, value);
                        }
                    }
                }
            }
        }
    }
}

