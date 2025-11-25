using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowGridElement : MonoBehaviour
{
    public Vector2 Size { get => GetComponent<RectTransform>().rect.size; }

}
