using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Luminosity.IO;
using ClemCAddons;
using System;

public class JoystickVisualizer : MonoBehaviour
{
    [SerializeField] private JoystickVisualizerMode _mode;
    [SerializeField] private float _targetIndicatorSize = 10;
    [SerializeField] private SpriteInfo _joystickSprites;
    [SerializeField] private SpriteInfoDirection _indicatorSprites;

    private static bool active = false;
    private static Vector3? targetPosition;
    private static bool hasIndicator = false;
    private static Vector2 indicatorDirection;
    private static bool hasSecondaryIndicator = false;
    private static Vector2 secondaryIndicatorDirection;
    private static IndicatorTargetInfo indicatorTargetInfo;
    private static bool bindFirstIndicator;

    public static bool BindFirstIndicator { get => bindFirstIndicator; set => bindFirstIndicator = value; }

    public enum IndicatorTargetInfo
    {
        Both,
        Left,
        Right
    }

    public enum JoystickVisualizerMode
    {
        Position,
        Sprites
    }

    [Serializable]
    public struct SpriteInfo
    {
        public Sprite Neutral;
        public Sprite Up;
        public float UpOffset;
        public SpriteInfo(Sprite neutral, Sprite up, float upOffset)
        {
            Neutral = neutral;
            Up = up;
            UpOffset = upOffset;
        }
    }

    [Serializable]
    public struct SpriteInfoDirection
    {
        public Sprite Neutral;
        public Sprite Left;
        public Sprite Right;
        public Sprite Both;
        public float UpOffset;
        public SpriteInfoDirection(Sprite neutral, Sprite left, Sprite right, Sprite both, float upOffset)
        {
            Neutral = neutral;
            Left = left;
            Right = right;
            UpOffset = upOffset;
            Both = both;
        }
    }

    void Start()
    {
        if (active)
        {
            GetComponent<Image>().enabled = true;
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(0).transform.localPosition = Vector3.zero;
        }
        else
        {
            GetComponent<Image>().enabled = false;
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(0).transform.localPosition = Vector3.zero;
        }
        transform.GetChild(1).gameObject.SetActive(false);
        transform.GetChild(1).transform.localPosition = Vector3.zero;
        transform.GetChild(2).gameObject.SetActive(false);
        transform.GetChild(2).transform.localPosition = Vector3.zero;
    }

    void Update()
    {
        if (active)
        {
            GetComponent<Image>().enabled = true;
            transform.GetChild(0).gameObject.SetActive(true);
            if (targetPosition == null)
                transform.localPosition = Vector3.zero;
            else
                transform.position = targetPosition.Value;
            if (hasIndicator)
            {
                transform.GetChild(1).gameObject.SetActive(true);
            }
            if (hasSecondaryIndicator)
            {
                transform.GetChild(2).gameObject.SetActive(true);
            }
        }
        else
        {
            GetComponent<Image>().enabled = false;
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(0).transform.localPosition = Vector3.zero;
            transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(1).transform.localPosition = Vector3.zero;
            transform.GetChild(2).gameObject.SetActive(false);
            transform.GetChild(2).transform.localPosition = Vector3.zero;
            return;
        }

        switch (_mode)
        {
            case JoystickVisualizerMode.Position:
                RunPositionMode();
                break;
            case JoystickVisualizerMode.Sprites:
                RunSpriteMode();
                break;
        }
        
    }
    
    private Vector2 GetInputs()
    {
        return new Vector2(InputManager.GetAxis("Horizontal") + InputManager.GetAxis("LookHorizontal"), InputManager.GetAxis("Vertical") + InputManager.GetAxis("LookVertical"));
    }

    public void RunSpriteMode()
    {
        var joystick = GetInputs().normalized;
        if(joystick != Vector2.zero)
        {
            transform.GetChild(0).localPosition = joystick.ToVector3() * _joystickSprites.UpOffset;
            transform.GetChild(0).rotation = Quaternion.Euler(0,0, Vector2.SignedAngle(Vector2.up, joystick));
            transform.GetChild(0).GetComponent<Image>().sprite = _joystickSprites.Up;
        }
        else
        {
            transform.GetChild(0).localPosition = Vector2.zero;
            transform.GetChild(0).rotation = Quaternion.identity;
            transform.GetChild(0).GetComponent<Image>().sprite = _joystickSprites.Neutral;
        }

        if (hasIndicator)
        {
            transform.GetChild(1).GetComponent<RectTransform>().sizeDelta = Vector2.one * _targetIndicatorSize;
            if (indicatorDirection != Vector2.zero || bindFirstIndicator)
            {
                transform.GetChild(1).gameObject.SetActive(true);
                switch (indicatorTargetInfo)
                {
                    case IndicatorTargetInfo.Both:
                        transform.GetChild(1).GetComponent<Image>().sprite = _indicatorSprites.Both;
                        break;
                    case IndicatorTargetInfo.Left:
                        transform.GetChild(1).GetComponent<Image>().sprite = _indicatorSprites.Left;
                        break;
                    case IndicatorTargetInfo.Right:
                        transform.GetChild(1).GetComponent<Image>().sprite = _indicatorSprites.Right;
                        break;
                }
                if (!bindFirstIndicator)
                {
                    transform.GetChild(1).rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, indicatorDirection.normalized));
                    transform.GetChild(1).position =
                        transform.position +
                            (indicatorDirection * _indicatorSprites.UpOffset).ToVector3();
                }
                else
                {
                    transform.GetChild(1).rotation = transform.GetChild(0).rotation;
                    transform.GetChild(1).position =
                        transform.position +
                            (joystick * _indicatorSprites.UpOffset).ToVector3();
                }
                transform.GetChild(1).GetComponent<Image>().color = Color.white;
            }
            else
            {
                transform.GetChild(1).gameObject.SetActive(_indicatorSprites.Neutral != null);
                transform.GetChild(1).GetComponent<Image>().sprite = _indicatorSprites.Neutral;
                transform.GetChild(1).rotation = Quaternion.identity;
                transform.GetChild(1).position = transform.position;
            }
        }
        if (hasSecondaryIndicator)
        {
            transform.GetChild(2).GetComponent<RectTransform>().sizeDelta = Vector2.one * _targetIndicatorSize;
            if (indicatorDirection != Vector2.zero)
            {
                transform.GetChild(2).gameObject.SetActive(true);
                switch (indicatorTargetInfo)
                {
                    case IndicatorTargetInfo.Both:
                        transform.GetChild(2).GetComponent<Image>().sprite = _indicatorSprites.Both;
                        break;
                    case IndicatorTargetInfo.Left:
                        transform.GetChild(2).GetComponent<Image>().sprite = _indicatorSprites.Left;
                        break;
                    case IndicatorTargetInfo.Right:
                        transform.GetChild(2).GetComponent<Image>().sprite = _indicatorSprites.Right;
                        break;
                }
                transform.GetChild(2).rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.up, indicatorDirection.normalized));
                transform.GetChild(2).GetComponent<Image>().color = Color.white;
                transform.GetChild(2).position =
                    transform.position +
                        (indicatorDirection * _indicatorSprites.UpOffset).ToVector3();
            }
            else
            {
                transform.GetChild(2).gameObject.SetActive(_indicatorSprites.Neutral != null);
                transform.GetChild(2).GetComponent<Image>().sprite = _indicatorSprites.Neutral;
                transform.GetChild(2).rotation = Quaternion.identity;
                transform.GetChild(2).position = transform.position;
            }
        }
    }

    public void RunPositionMode()
    {
        transform.GetChild(0).localPosition = Vector2.Lerp(transform.GetChild(0).localPosition,
                transform.GetChild(0).GetLocalPosition(transform.position +
                    (GetInputs().normalized * GetComponent<RectTransform>().sizeDelta).ToVector3())
                , Time.deltaTime * 20
        );
        if (hasIndicator)
        {
            transform.GetChild(1).position =
                transform.position +
                    (indicatorDirection * GetComponent<RectTransform>().sizeDelta).ToVector3();
        }
        if (hasSecondaryIndicator)
        {
            transform.GetChild(2).position =
                transform.position +
                    (secondaryIndicatorDirection * GetComponent<RectTransform>().sizeDelta).ToVector3();
        }
    }

    public static void Show(Vector2? position = null, bool usingIndicator = false, Vector2 defaultIndicatorDirection = default, bool usingSecondaryIndicator = false, Vector2 defaultSecondaryIndicatorDirection = default, bool isFirstIndicatorBinded = false, IndicatorTargetInfo defaultIndicatorTargetInfo = IndicatorTargetInfo.Both)
    {
        active = true;
        targetPosition = position;
        hasIndicator = usingIndicator;
        indicatorDirection = defaultIndicatorDirection;
        hasSecondaryIndicator = usingSecondaryIndicator;
        secondaryIndicatorDirection = defaultSecondaryIndicatorDirection;
        bindFirstIndicator = isFirstIndicatorBinded;
        indicatorTargetInfo = defaultIndicatorTargetInfo;
    }

    public static void Hide()
    {
        active = false;
        Debug.Log("hidden");
    }

    public static void  UpdateIndicator(Vector2 direction, IndicatorTargetInfo indicatorTargetInfo, Vector2 secondaryDirection = default)
    {
        indicatorDirection = direction;
        secondaryIndicatorDirection = secondaryDirection;
        JoystickVisualizer.indicatorTargetInfo = indicatorTargetInfo;
    }
}
