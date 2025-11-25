using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;

[ExecuteInEditMode]
public class UIEffects : MonoBehaviour
{
    [SerializeField] private bool _active = false;
    [SerializeField] private Effect _mode;
    [SerializeField] private float _strength = 1;
    [SerializeField] private float _speed = 1;
    [Header("Debug")]
    [SerializeField] private bool _runInEditor = false;

    private RectTransform rectTransform;

    private Vector2 basePosition;
    private Vector2 baseScale;
    private Quaternion baseRotation;

    private int direction = 1;


    private float position = 0;

    public enum Effect
    {
        Shake,
        Float,
        Scale
    }

    public void Activate()
    {
        if(!_active)
            basePosition = rectTransform.anchoredPosition;
        _active = true;
    }

    public void Desactivate()
    {
        _active = false;
        rectTransform.anchoredPosition = basePosition;
        rectTransform.localRotation = baseRotation;
        rectTransform.localScale = baseScale;
    }
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        basePosition = rectTransform.anchoredPosition;
        baseRotation = rectTransform.localRotation;
        baseScale = rectTransform.localScale;
    }

    void Update()
    {
        if (!_runInEditor && !Application.isPlaying)
            return;
        if (!_active)
        {
            rectTransform.anchoredPosition = basePosition;
            rectTransform.localRotation = baseRotation;
            return;
        }
        switch (_mode)
        {
            case Effect.Shake:
                rectTransform.anchoredPosition = basePosition + Vector2.right * direction * Random.Range(0, _strength);
                rectTransform.localRotation = baseRotation * Quaternion.Euler(0, 0, Random.Range(0, _strength) * direction);
                if (ClemCAddons.Utilities.Timer.MinimumDelay(("UIEffect"+gameObject.GetInstanceID()).GetHashCode(), (1000 / _speed.Max(1)).Round()))
                {
                    direction *= -1;
                }
                break;
            case Effect.Float:
                if (direction == 1)
                    position += Time.deltaTime * _speed;
                else
                    position -= Time.deltaTime * _speed;
                if (position >= 1)
                {
                    position = 1;
                    direction = -1;
                }
                if (position <= -1)
                {
                    position = -1;
                    direction = 1;
                }
                rectTransform.anchoredPosition = basePosition + Vector2.up * position * rectTransform.sizeDelta.y * 0.05f * _strength;
                break;
            case Effect.Scale:
                if (direction == 1)
                    position += Time.deltaTime * _speed;
                else
                    position -= Time.deltaTime * _speed;
                if (position >= 1)
                {
                    position = 1;
                    direction = -1;
                }
                if (position <= -1)
                {
                    position = -1;
                    direction = 1;
                }
                rectTransform.localScale = baseScale + Vector2.one * position * 0.05f * _strength;
                break;
        }
    }
}
