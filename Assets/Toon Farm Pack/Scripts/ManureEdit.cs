using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Character_Edit
{
    [ExecuteInEditMode]
    public class ManureEdit : MonoBehaviour
    {
		
		[Range(0, 100)]
		public int ManureSlider;
		
        void OnValidate()
        {
			SetManureForGameObject(ManureSlider);
        }
       		
		public void SetManureForGameObject(int value)
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
                            if (smr.sharedMesh.GetBlendShapeName(i) == "Manure")
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

