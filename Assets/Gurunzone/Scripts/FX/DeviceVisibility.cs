using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{

    public class DeviceVisibility : MonoBehaviour
    {
        public bool desktop = true;
        public bool mobile = true;

        void Start()
        {
            bool ismobile = TheGame.IsMobile();
            if (ismobile && !mobile)
                gameObject.SetActive(false);
            else if (!ismobile && !desktop)
                gameObject.SetActive(false);
        }
    }
}
