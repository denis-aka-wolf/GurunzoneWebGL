using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Gurunzone
{

    public class GameTool
    {
        public static Coroutine StartCoroutine(IEnumerator routine)
        {
            return GameToolMono.Inst.StartCoroutine(routine);
        }

        public static void StopCoroutine(Coroutine routine)
        {
            GameToolMono.Inst.StopCoroutine(routine);
        }

        public static Coroutine WaitFor(float duration, UnityAction callback)
        {
            return StartCoroutine(WaitForRoutine(duration, callback));
        }

        public static Coroutine WaitUntil(Func<bool> condition, UnityAction callback)
        {
            return StartCoroutine(WaitUntilRoutine(condition, callback));
        }

        private static IEnumerator WaitForRoutine(float duration, UnityAction callback)
        {
            yield return new WaitForSeconds(duration);
            callback?.Invoke();
        }

        private static IEnumerator WaitUntilRoutine(Func<bool> condition, UnityAction callback)
        {
            yield return new WaitUntil(condition);
            callback?.Invoke();
        }
    }

    public class GameToolMono : MonoBehaviour
    {
        private static GameToolMono _instance;

        public Coroutine StartRoutine(IEnumerator routine)
        {
            return StartCoroutine(routine);
        }

        public void StopRoutine(Coroutine routine)
        {
            StopCoroutine(routine);
        }

        public static GameToolMono Inst
        {
            get
            {
                if (_instance == null)
                {
                    GameObject ntool = new GameObject("GameTool");
                    _instance = ntool.AddComponent<GameToolMono>();
                }
                return _instance;
            }
        }
    }

}
