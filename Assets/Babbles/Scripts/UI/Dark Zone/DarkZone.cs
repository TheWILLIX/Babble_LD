using ClemCAddons.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace DarkZone
{
    public class DarkZone : MonoBehaviour
    {
        [SerializeField] private Color _backgroundColor = new Color(0, 0, 0, 1);
        [SerializeField] private Color _color = new Color(0,0,0,1);
        [SerializeField,VectorRange(0,1,0,1,true)] private Vector2[] _steps;
        [SerializeField] private int[] _defaultRadiuses;
        [SerializeField, Range(1, 0)] private float _lighten = 1;
        private Darkener _darkener;
        private bool _isInZone = false;
        private List<KeyValuePair<int, KeyValuePair<Transform, int[]>>> _lights = new List<KeyValuePair<int, KeyValuePair<Transform, int[]>>>();
        private List<int> _permanentLights = new List<int>();

        void Start()
        {
            _darkener = FindObjectOfType<Darkener>();
        }

        void OnTriggerEnter(Collider collider)
        {
            if (collider.CompareTag("Player"))
            {
                DarkFunctions.CurrentDarkZone = this;
                _isInZone = true;
                DarkFunctions.StartLight(collider.transform, DarkFunctions.LightType.player);
            }
        }

        void OnTriggerExit(Collider collider)
        {
            if (collider.CompareTag("Player"))
            {
                DarkFunctions.CurrentDarkZone = null;
                _isInZone = false;
                _darkener.ClearScreen();
            }
        }

        public int AddLight(Transform transform, int[] radius = null, bool permanent = false, int customUID = -1)
        {
            int UID = 0;
            if (_isInZone || permanent)
            {
                if (radius == null)
                {
                    radius = _defaultRadiuses;
                }
                if(customUID != -1)
                {
                    UID = customUID;
                } else
                {
                    while (_lights.Exists(t => t.Key == UID))
                    {
                        UID++;
                    }
                }
                _lights.Add(new KeyValuePair<int, KeyValuePair<Transform, int[]>>(UID, new KeyValuePair<Transform, int[]>(transform, radius)));
                if (permanent)
                {
                    _permanentLights.Add(UID);
                }
                return UID;
            }
            return -1;
        }

        public void RemoveLight(int UID)
        {
            if(_permanentLights.FindIndex(t => t == UID) == -1)
            {
                _lights.RemoveAll(t => t.Key == UID);
            }
        }

        public void SpawnLights()
        {
            if (_isInZone)
            {
                if (Camera.main != null)
                {
                    Vector2[] tempSteps = new Vector2[_steps.Length];
                    for (int i = 0; i < tempSteps.Length; i++)
                    {
                        tempSteps[i] = _steps[i] * _lighten;
                    }
                    Color tempColor = _backgroundColor;
                    tempColor.a *= _lighten;
                    _darkener.Fill(tempColor, true);
                    _darkener.ClearIntermediaryTexture();
                    for (int i = 0; i < _lights.Count; i++)
                    {
                        Vector3 posOnScreen = Camera.main.WorldToScreenPoint(_lights[i].Value.Key.position);
                        posOnScreen.x = posOnScreen.x * _darkener.TextureSize.x / Camera.main.pixelWidth;
                        posOnScreen.y = posOnScreen.y * _darkener.TextureSize.y / Camera.main.pixelHeight;
                        _darkener.UpdateInLayeredGradient(new Vector2Int((int)posOnScreen.x, (int)posOnScreen.y), _lights[i].Value.Value, _color, tempSteps, true, false, true, true);
                    }
                    _darkener.ApplyIntermediaryTexture();
                } else
                {
                    Color tempColor = _backgroundColor;
                    tempColor.a *= _lighten * 0.9f;
                    _darkener.Fill(tempColor, true);
                }
            }
        }

        void Update()
        {
            if (_isInZone)
            {
                if(_lights.Count > 0)
                {
                    SpawnLights();
                } else if (Camera.main != null)
                {
                    Vector2[] tempSteps = new Vector2[_steps.Length];
                    for (int i = 0; i < tempSteps.Length; i++)
                    {
                        tempSteps[i] = _steps[i] * _lighten;
                    }
                    Color tempColor = _backgroundColor;
                    tempColor.a *= _lighten;
                    _darkener.Fill(tempColor, true);
                } else
                {
                    Color tempColor = _backgroundColor;
                    tempColor.a *= _lighten * 0.9f;
                    _darkener.Fill(tempColor, true);
                }
            }
        }

        public void ClearLights()
        {
            _darkener.ClearScreen();
        }
    }
}