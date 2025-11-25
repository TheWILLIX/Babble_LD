using ClemCAddons;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ExternalVisiblity : MonoBehaviour
{
    [SerializeField] private string _type;
    private bool active = true;
    private Wearable target;
    private bool current;

    void Start()
    {
    }

    void Update()
    {
        if(active && target != null && target.LifeTime < 10)
        {
            if(ClemCAddons.Utilities.Timer.MinimumDelay(("ExtVisibility"+gameObject.GetInstanceID()).GetHashCode(), (target.LifeTime * 100).Round(), false))
            {
                ActiveAll(transform, current);
                current = !current;
            }
        }
        var r = FindObjectsOfType<ExceptionItem>().ToList().Find(t => t.Type == _type);
        if (active && r != null == active)
            return;
        active = r != null;
        if (active)
        {
            current = false;
            target = r.GetComponent<Wearable>();
        }
        ActiveAll(transform, active);
    }

    private void ActiveAll(Transform transform, bool active)
    {
        var r = transform.GetComponentsInChildren<Renderer>();
        foreach (var obj in r)
        {
            obj.enabled = active;
        }
    }
}
