using ClemCAddons.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakerFloat : MonoBehaviour
{
    private RectTransform rectTransform;
    private bool direction;
    private float position = 0;
    private float rotation = 0;
    private bool hiding;
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = transform.parent.GetComponent<RectTransform>().sizeDelta * 0.1f;
        Lerper.ConstantLerp(rectTransform.sizeDelta, transform.parent.GetComponent<RectTransform>().sizeDelta, 0.5f, (Vector2 v) => { if(rectTransform!=null) rectTransform.sizeDelta = v; });
    }

    void Update()
    {
        if (hiding)
        {
            position -= Time.deltaTime * 10;
            rectTransform.localPosition = Vector2.up * position * rectTransform.sizeDelta.y * 0.05f;
            rectTransform.sizeDelta *= (1-Time.deltaTime*5);
            return;
        }
        if (rotation < 1)
        {
            rotation += Time.deltaTime * 3;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, 360 * rotation));
        }
        else
            transform.rotation = Quaternion.identity;
        if (direction)
            position += Time.deltaTime * 1f;
        else
            position -= Time.deltaTime * 1f;
        if(position >= 1)
        {
            position = 1;
            direction = false;
        }
        if(position <= -1)
        {
            position = -1;
            direction = true;
        }
        rectTransform.localPosition = Vector2.up * position * rectTransform.sizeDelta.y * 0.05f;
    }

    public void Hide()
    {
        _ = GameTools.DelayedCall(500, () => Destroy(transform.parent.gameObject));
        hiding = true;
    }
}
