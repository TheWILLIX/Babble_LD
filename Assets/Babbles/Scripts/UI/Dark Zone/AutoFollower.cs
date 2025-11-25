using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using UnityEngine.Serialization;

namespace DarkZone
{
    using static DarkFunctions;
    public static class DarkFunctions
    {
        internal static AutoFollower _current;

        public static DarkZone CurrentDarkZone { get => _current.CurrentDarkZone; set => _current.CurrentDarkZone = value; }

        public static void StartLight(Transform transform, LightType lightType, bool permanent = false, int[] radiuses = null)
        {
            _current.StartLight(transform, lightType, permanent, radiuses);
        }

        public static void EndLight(int id)
        {
            _current.EndLight(id);
        }

        public enum LightType
        {
            player,
            external
        }

        public struct Light
        {
            public Transform Transform;
            public float Duration;
            public int UID;
            public Light(Transform transform, float duration, int uid = 0)
            {
                Transform = transform;
                Duration = duration;
                UID = uid;
            }
        }
    }
    public class AutoFollower : MonoBehaviour
    {
        [SerializeField] private float player;
        [SerializeField] private float external;
        private DarkZone _currentDarkZone;
        private Light[] _currentLights = new Light[] { };

        public DarkZone CurrentDarkZone { get => _currentDarkZone; set => _currentDarkZone = value; }

        void Awake()
        {
            _current = this;
        }

        void Update()
        {
            if (_currentDarkZone != null)
            {
                for(int i = _currentLights.Length-1; i >= 0; i--)
                {
                    if (_currentLights[i].Duration > 0)
                    {
                        _currentLights[i].Duration = Mathf.Max(0, _currentLights[i].Duration - Time.deltaTime);
                        if (_currentLights[i].Duration <= 0)
                        {
                            EndLight(i);
                        }
                    }
                }
            }
        }

        internal void StartLight(Transform transform, LightType lightType, bool permanent = false, int[] radiuses = null)
        {
            float duration = 0f;
            switch (lightType)
            {
                case LightType.player:
                {
                    duration = player;
                    break;
                }
                case LightType.external:
                {
                    duration = external;
                    break;
                }
                default: break;
            }
            if (permanent)
            {
                var r = FindObjectsOfType<DarkZone>();
                int UID = Random.Range(-100000, -2); // so permanent use different adresses from non-permanent
                foreach(var t in r)
                {
                    t.AddLight(transform, radiuses, permanent, UID);
                }
                _currentLights = _currentLights.Add(new Light(transform, duration, UID));
            } else
            {
                _currentLights = _currentLights.Add(new Light(transform, duration, _currentDarkZone.AddLight(transform, radiuses)));
            }
        }

        internal void EndLight(int id)
        {
            _currentLights[id].Duration = 0;
            _currentDarkZone.RemoveLight(_currentLights[id].UID);
            _currentDarkZone.ClearLights();
        }
    }
}