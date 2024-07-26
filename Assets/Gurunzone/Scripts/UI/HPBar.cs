using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gurunzone
{
    public class HPBar : MonoBehaviour
    {
        public Image fill;

        [HideInInspector]
        public Destructible target;

        private CanvasGroup canvas_group;

        void Start()
        {
            canvas_group = GetComponentInChildren<CanvasGroup>();
            fill.fillAmount = 1f;
            canvas_group.alpha = 0f;
        }

        void Update()
        {
            Vector3 dir = TheCamera.Get().GetFacingFront();
            transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

            if (target == null || target.IsDead())
            {
                Destroy(gameObject);
            }
            else
            {
                fill.fillAmount = target.HP / (float) target.MaxHP;
                canvas_group.alpha = fill.fillAmount < 0.999f ? 1f : 0f;
            }
        }
    }

}
