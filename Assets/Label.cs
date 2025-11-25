using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using UnityEngine.UI;

[ExecuteInEditMode]
public class Label : MonoBehaviour
{
    [SerializeField] private bool enableEditorUpdate;
    [SerializeField] private Vector2 basePosition;
    [SerializeField] private Quaternion baseRotation;
    [SerializeField] private Vector2 targetPosition;
    [SerializeField] private Quaternion targetRotation;
    [SerializeField] private bool isAtTarget;
    [SerializeField] private float speed = 4;
    [SerializeField] private Sprite spriteInactive;
    [SerializeField] private Sprite spriteActive;

    [SerializeField] private Sprite alternativeSpriteInactive;
    [SerializeField] private Sprite alternativeSpriteActive;


    [Header("EN")]
    [SerializeField] private Sprite spriteInactiveEN;
    [SerializeField] private Sprite spriteActiveEN;
    [SerializeField] private Sprite alternativeSpriteInactiveEN;
    [SerializeField] private Sprite alternativeSpriteActiveEN;


    private bool isAlternative;

    private static Label activeLabel;

    private RectTransform rectTransform;
    private Image image;

    private RectTransform RectTransform
    {
        get
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
            return rectTransform;
        }
    }
    private Image Image
    {
        get
        {
            if(image == null)
                image = GetComponent<Image>();
            return image;
        }
    }

    void Update()
    {
        if (!Application.isPlaying && !enableEditorUpdate)
            return;
        RectTransform.anchoredPosition = Vector3.Lerp(RectTransform.anchoredPosition, isAtTarget ? targetPosition : basePosition, Time.deltaTime * speed);
        RectTransform.localRotation = Quaternion.Lerp(RectTransform.localRotation, isAtTarget ? targetRotation : baseRotation, Time.deltaTime * speed);
        Image.enabled = !RectTransform.anchoredPosition.ApproximatelyEqual(basePosition, 0.05f);

        if(Multilang.Instance.CurrentLanguage == "FR")
        {
            if (isAlternative == true)
            {
                if (activeLabel == this)
                {
                    Image.sprite = alternativeSpriteActive;
                }
                else
                {
                    Image.sprite = alternativeSpriteInactive;
                }
            }
            else
            {
                if (activeLabel == this)
                {
                    Image.sprite = spriteActive;
                }
                else
                {
                    Image.sprite = spriteInactive;
                }
            }
        }
        else if (Multilang.Instance.CurrentLanguage == "EN")
        {
            if (isAlternative == true)
            {
                if (activeLabel == this)
                {
                    Image.sprite = alternativeSpriteActiveEN;
                }
                else
                {
                    Image.sprite = alternativeSpriteInactiveEN;
                }
            }
            else
            {
                if (activeLabel == this)
                {
                    Image.sprite = spriteActiveEN;
                }
                else
                {
                    Image.sprite = spriteInactiveEN;
                }
            }
        }

       // Image.sprite = activeLabel == this ? ( isAlternative ? alternativeSpriteActive  : spriteActive) : (isAlternative ? alternativeSpriteInactive : spriteInactive);
    }


    public void SetTargeted(bool target)
    {   
        isAtTarget = target;
    }

    public void SetActive(bool active, bool isAlternative = false)
    {
        this.isAlternative = isAlternative;
        if (active)
            activeLabel = this;
        else
            activeLabel = null;
    }

    public void ResetLabel()
    {
        isAlternative = false;
    }
}
