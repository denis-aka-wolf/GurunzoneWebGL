using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gurunzone
{

    /// <summary>
    /// Apply visual effects for night and light fx
    /// </summary>

    public class LightFX : MonoBehaviour
    {
        public float light_ambient_day = 1f;
        public float light_directional_day = 1f;
        public float light_ambient_night = 0.5f;
        public float light_directional_night = 0.5f;
        public float shadow_night = 0.2f;

        public Color color_day = Color.white;
        public Color color_night = Color.white;

        public float transition_speed = 0.2f;
        public bool rotate_shadows;

        private Light dir_light;
        private Quaternion start_rot;

        void Start()
        {
            dir_light = GetDirectionalLight();
            start_rot = dir_light.transform.rotation;
            UpdateLights(100f);
        }

        void Update()
        {
            UpdateLights(transition_speed);
        }

        private void UpdateLights(float speed)
        {
            bool is_night = TheGame.Get().IsNight();
            float light_mult = 1f;
            float target = is_night ? light_ambient_night : light_ambient_day;
            float light_angle = SaveData.Get().day_time * 360f / 24f;
            RenderSettings.ambientIntensity = Mathf.MoveTowards(RenderSettings.ambientIntensity, target * light_mult, speed * Time.deltaTime);
            if (dir_light != null && dir_light.type == LightType.Directional)
            {
                float dtarget = is_night ? light_directional_night : light_directional_day;
                dir_light.intensity = Mathf.MoveTowards(dir_light.intensity, dtarget * light_mult, speed * Time.deltaTime);
                dir_light.shadowStrength = Mathf.MoveTowards(dir_light.shadowStrength, is_night ? shadow_night : 1f, speed * Time.deltaTime);
                dir_light.color = Vector4.MoveTowards(dir_light.color, is_night ? color_night : color_day, speed * Time.deltaTime);
                if (rotate_shadows)
                    dir_light.transform.rotation = Quaternion.Euler(0f, light_angle + 180f, 0f) * start_rot;
            }
        }

        public Light GetDirectionalLight()
        {
            foreach (Light light in FindObjectsOfType<Light>())
            {
                if (light.type == LightType.Directional)
                    return light;
            }
            return null;
        }
    }

}