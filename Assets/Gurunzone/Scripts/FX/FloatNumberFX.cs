using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gurunzone
{

    public class FloatNumberFX : MonoBehaviour
    {
        public Text number;
        public float duration = 2f;


        void Start()
        {
            Destroy(gameObject, duration);
        }

        private void Update()
        {
            Vector3 cam_dir = TheCamera.Get().transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(-cam_dir, Vector3.up);
        }

        public static Vector3 RandomPos(Vector3 pos)
        {
            float random_offset = 2f;
            float x = Random.Range(-random_offset, random_offset);
            float y = Random.Range(-random_offset, random_offset);
            float z = Random.Range(-random_offset, random_offset);
            Vector3 offset = new Vector3(x, y, z);
            return pos + offset;
        }

        public static FloatNumberFX Create(int value, Vector3 pos)
        {
            return Create(value.ToString(), pos);
        }

        public static FloatNumberFX Create(int value, Vector3 pos, Color color)
        {
            FloatNumberFX fx = Create(value.ToString(), pos);
            fx.number.color = color;
            return fx;
        }

        public static FloatNumberFX Create(string value, Vector3 pos)
        {
            Vector3 cam_dir = TheCamera.Get().transform.position - pos;
            GameObject obj = Instantiate(AssetData.Get().float_number, RandomPos(pos), Quaternion.LookRotation(-cam_dir, Vector3.up));
            FloatNumberFX fx = obj.GetComponent<FloatNumberFX>();
            fx.number.text = value;
            return fx;
        }
    }
}
