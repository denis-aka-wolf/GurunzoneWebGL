using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Character_Edit
{
    [ExecuteInEditMode]
    public class TractorEdit : MonoBehaviour
    {
		
		[Range(0, 100)]
		public int DirtSlider;
		
        void OnValidate()
        {
			SetDirtForGameObject(DirtSlider);
        }
       		
		public void SetDirtForGameObject(int value)
        {
            foreach (Transform t in this.transform)
            {
                SkinnedMeshRenderer[] meshRenderer = t.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (SkinnedMeshRenderer smr in meshRenderer)
                {
                    if (smr != null && smr.sharedMesh.blendShapeCount > 0)
                    {
                        for (int i = 0; i < smr.sharedMesh.blendShapeCount; i++)
                        {
                            if (smr.sharedMesh.GetBlendShapeName(i) == "Dirt")
                            {
                                smr.SetBlendShapeWeight(i, value);
                            }
                        }
                    }
                }
            }
        }
    }
}

