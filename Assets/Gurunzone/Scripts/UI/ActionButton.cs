using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Gurunzone
{
    public class ActionButton : MonoBehaviour
    {
        public ActionBasic action;

        public UnityAction<ActionBasic> onClick;

        void Start()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

        void Update()
        {

        }

        public void OnClick()
        {
            onClick?.Invoke(action);
        }
    }
}
